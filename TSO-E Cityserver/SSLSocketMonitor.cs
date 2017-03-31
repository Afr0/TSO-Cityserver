using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using MiscUtil.IO;
using MiscUtil.Conversion;
using TSO_E_Cityserver.Packets;

namespace TSO_E_Cityserver
{
    /// <summary>
    /// A basic SSLSocket class. 
    /// From: http://codereview.stackexchange.com/questions/15550/asynchronous-sslstream
    /// </summary>
    public class SslSocketMonitor
    {
        private const int ListenLength = 500;
        private int m_Port;
        private ManualResetEvent m_ConnectionResetEvent = new ManualResetEvent(false);

        public event EventHandler<Client> Connected;
        public event EventHandler<Client> Disconnected;
        public event EventHandler<Client> ReceivedData;

        //This is the value set in HostOnlinePDU specifying the host's maximum packet size.
        private static int BUFFER_SIZE = 32767;
        //Aries packet type and size for the current packet being processed.
        private uint m_PacketType, m_PacketSize;
        private object m_LockingObj = new object();

        private Socket m_Socket;

        public SslSocketMonitor(int port)
        {
            m_Port = port;
        }

        /// <summary>
        /// Initializes a socket and starts listening for TLS connections.
        /// This call blocks until a connection is received.
        /// </summary>
        public void InitializeSocket()
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Socket.Bind(new IPEndPoint(IPAddress.Any, m_Port));
            m_Socket.Listen(ListenLength);

            while (true)
            {
                //Set the event to nonsignaled state.
                m_ConnectionResetEvent.Reset();
                m_Socket.BeginAccept(AcceptConnections, new Client());

                //Wait until a connection is made before continuing.
                m_ConnectionResetEvent.WaitOne();
            }
        }

        private void AcceptConnections(IAsyncResult result)
        {
            var resultWrapper = (Client)result.AsyncState;
            try
            {
                resultWrapper.ReplaceSslStream(result, m_Socket);
                resultWrapper.BeginAuthenticateAsServer(EndAuthenticate);

                m_Socket.BeginAccept(AcceptConnections, new Client());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                if (e.InnerException != null)
                    Console.WriteLine(e.InnerException);

                Disconnected?.Invoke(this, resultWrapper);
                resultWrapper.CloseAndDisposeSslStream();
            }
        }

        private void EndAuthenticate(IAsyncResult result)
        {
            var resultWrapper = (Client)result.AsyncState;
            try
            {
                try
                {
                    resultWrapper.EndAuthenticateAsServer(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    if (ex.InnerException != null)
                        Console.WriteLine(ex.InnerException);
                }

                if (resultWrapper.IsAuthenticated())
                {
                    Connected?.Invoke(this, resultWrapper);
                    if (resultWrapper.CanRead())
                    {
                        //resultWrapper.CreateBuffer(5);
                        resultWrapper.CreateBuffer(BUFFER_SIZE);
                        resultWrapper.CreateSlushBuffer(BUFFER_SIZE);
                        resultWrapper.BeginRead(ReceiveData);
                    }
                }
                else
                {
                    Disconnected?.Invoke(this, resultWrapper);
                    resultWrapper.CloseAndDisposeSslStream();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Disconnected?.Invoke(this, resultWrapper);
                resultWrapper.CloseAndDisposeSslStream();
            }
        }

        //Packet type (8), timestamp (8) and payload size (8)
        private static readonly uint HEADER_SIZE = 12;
        private int m_CurrentlyReceived = 0;
        private bool m_PartialPacketReceived = false;

        private void ReceiveData(IAsyncResult result)
        {
            lock (m_LockingObj) //The following code won't execute unless it's able to acquire a lock.
            {
                var resultWrapper = (Client)result.AsyncState;
                try
                {
                    var size = resultWrapper.EndRead(result);
                    m_CurrentlyReceived += size;

                    if (m_CurrentlyReceived >= HEADER_SIZE)
                    {
                        AriesHeader Header;

                        if (!m_PartialPacketReceived)
                            Header = ReadAriesHeader((byte[])resultWrapper.Buffer.Clone());
                        else
                            Header = ReadAriesHeader((byte[])resultWrapper.SlushBuffer.Clone());

                        m_PacketType = Header.PacketType;
                        m_PacketSize = Header.PacketSize;

                        if (m_CurrentlyReceived >= m_PacketSize)
                        {
                            ProcessBuffer(resultWrapper);

                            resultWrapper.BeginRead(ReceiveData);
                        }
                    }
                    else
                        resultWrapper.BeginRead(ReceiveData);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Disconnected?.Invoke(this, resultWrapper);
                    resultWrapper.CloseAndDisposeSslStream();
                }
            }
        }

        private void ProcessBuffer(Client C)
        {
            lock (m_LockingObj) //The following code won't execute unless it's able to acquire a lock.
            {
                while (m_CurrentlyReceived >= m_PacketSize)
                {
                    byte[] PacketBuf = new byte[m_PacketSize];

                    if (!m_PartialPacketReceived)
                    {
                        Array.Copy(C.Buffer, PacketBuf, m_PacketSize);
                    }
                    else
                    {
                        Array.Copy(C.SlushBuffer, PacketBuf, C.SlushBuffer.Length - 1);
                        Array.Copy(C.Buffer, PacketBuf, (m_PacketSize - (C.SlushBuffer.Length - 1)));
                        C.CreateSlushBuffer(BUFFER_SIZE);
                        m_PartialPacketReceived = false;
                    }

                    if (m_PacketType != 0)
                    {
                        lock (C.ReceivedPackets)
                            C.ReceivedPackets.Enqueue(new AriesPacket(PacketBuf, true));

                        m_CurrentlyReceived -= (int)m_PacketSize;

                        ReceivedData?.Invoke(this, C);
                    }
                    else
                    {
                        VoltronHeader Header = ReadVoltronHeader(PacketBuf, 12);

                        if (Header.PacketSize < ((PacketBuf.Length - 1) - 12))
                            ProcessVoltronPackets(C, PacketBuf);
                        else
                        {
                            lock (C.ReceivedPackets)
                                C.ReceivedPackets.Enqueue(new VoltronPacket(PacketBuf, true));

                            ReceivedData?.Invoke(this, C);
                        }

                        m_CurrentlyReceived -= (int)m_PacketSize;
                    }

                    if (m_CurrentlyReceived > 0)
                    {
                        byte[] Remainder = new byte[m_CurrentlyReceived];
                        Array.ConstrainedCopy(C.Buffer, (C.Buffer.Length - m_CurrentlyReceived) + 1,
                            Remainder, 0, m_CurrentlyReceived);

                        C.CreateBuffer(BUFFER_SIZE);
                        Array.Copy(Remainder, C.SlushBuffer, m_CurrentlyReceived);
                        Remainder = null;
                        m_PartialPacketReceived = true;
                    }
                    else
                        C.CreateBuffer(BUFFER_SIZE);
                }
            }
        }

        /// <summary>
        /// Multiple Voltron packets were sent in a Aries frame.
        /// </summary>
        /// <param name="PacketBuf">The packet buffer containing the packets to process.</param>
        private void ProcessVoltronPackets(Client C, byte[] PacketBuf)
        {
            VoltronHeader Header = ReadVoltronHeader(PacketBuf, 12);

            MemoryStream AriesStream = new MemoryStream(PacketBuf);
            EndianBinaryReader Reader = new EndianBinaryReader(new LittleEndianBitConverter(), AriesStream);
            int Remaining = (int)(AriesStream.Length - 1) - 12;

            byte[] AriesHeader = Reader.ReadBytes(12); //Aries header.

            Reader = new EndianBinaryReader(new BigEndianBitConverter(), AriesStream);
            Reader.BaseStream.Position = 12; //We've already read the header.

            while (Header.PacketSize < Remaining)
            {
                byte[] VoltronData = Reader.ReadBytes((int)Header.PacketSize);
                if (Header.PacketType == 0x0044)
                {
                    SplitBufferPDU SplitBufferPacket;

                    SplitBufferPacket = new SplitBufferPDU(VoltronData, false);

                    lock (C.ReceivedSplitBuffers)
                        C.ReceivedSplitBuffers.Enqueue(SplitBufferPacket);

                    if (SplitBufferPacket.EOF == 1)
                        CompileVoltronPackets(C);
                }
                else
                {
                    VoltronPacket Packet = new VoltronPacket(ReconstructVoltronPacket(AriesHeader,
                        VoltronData), true);

                    lock (C.ReceivedPackets)
                        C.ReceivedPackets.Enqueue(Packet);
                }

                Remaining -= (int)Header.PacketSize;

                if (Header.PacketSize < Remaining)
                    Header = ReadVoltronHeader(AriesStream.ToArray(), (int)(AriesStream.Position));
            }

            Reader.Close();

            ReceivedData?.Invoke(this, C);
        }

        private void CompileVoltronPackets(Client C)
        {
            MemoryStream OutputStream = new MemoryStream();
            EndianBinaryReader Reader;
            EndianBinaryWriter Writer;

            AriesHeader AHeader = new AriesHeader();
            AHeader.PacketSize = m_PacketSize;
            AHeader.PacketType = m_PacketType;

            //Reassemble all the Voltron packets.
            for(int i = 0; i < C.ReceivedSplitBuffers.Count; i++)
            {
                SplitBufferPDU SplitBuffer;
                C.ReceivedSplitBuffers.TryDequeue(out SplitBuffer);

                Reader = new EndianBinaryReader(new BigEndianBitConverter(), OutputStream);
                Writer = new EndianBinaryWriter(new BigEndianBitConverter(), OutputStream);

                Writer.Write(Reader.ReadBytes((int)SplitBuffer.FragmentSize));
                Writer.Flush();
            }

            MemoryStream VoltronPackets = new MemoryStream(OutputStream.ToArray());
            uint BufSize = (uint)VoltronPackets.Length;

            for(int i = 0; i < BufSize; i++)
            {
                Reader = new EndianBinaryReader(new BigEndianBitConverter(), VoltronPackets);
                VoltronHeader Header = ReadVoltronHeader(Reader.ReadBytes(12));
                Reader.BaseStream.Position = 0; //Backtrack to beginning of stream.

                VoltronPacket VPacket = new VoltronPacket(
                    ReconstructVoltronPacket(AHeader, Reader.ReadBytes((int)Header.PacketSize)), true);
                C.ReceivedPackets.Enqueue(VPacket);
                BufSize -= Header.PacketSize;

                ReceivedData?.Invoke(this, C);
            }
        }

        /// <summary>
        /// Reconstructs a (full) Voltron packet in memory.
        /// </summary>
        /// <param name="AriesHeader">The header of an aries packet.</param>
        /// <param name="VoltronPacket">The body (and header) of a Voltron packet.</param>
        /// <returns>A Voltron packet with its corresponding Aries header.</returns>
        private byte[] ReconstructVoltronPacket(byte[] AriesHeader, byte[] VoltronPacket)
        {
            MemoryStream OutputStream = new MemoryStream();
            EndianBinaryWriter Writer = new EndianBinaryWriter(new LittleEndianBitConverter(),
                OutputStream);

            Writer.Write(AriesHeader);
            Writer.Flush();

            Writer = new EndianBinaryWriter(new BigEndianBitConverter(), OutputStream);

            Writer.Write(VoltronPacket);
            Writer.Flush();

            return OutputStream.ToArray();
        }

        /// <summary>
        /// Reconstructs a (full) Voltron packet in memory.
        /// </summary>
        /// <param name="AriesHeader">The header of an aries packet.</param>
        /// <param name="VoltronPacket">The body (and header) of a Voltron packet.</param>
        /// <returns>A Voltron packet with its corresponding Aries header.</returns>
        private byte[] ReconstructVoltronPacket(AriesHeader AHeader, byte[] VoltronPacket)
        {
            MemoryStream OutputStream = new MemoryStream();
            EndianBinaryWriter Writer = new EndianBinaryWriter(new LittleEndianBitConverter(),
                OutputStream);

            Writer.Write(AHeader.PacketType);
            Writer.Write(AHeader.Timestamp);
            Writer.Write(AHeader.PacketSize);
            Writer.Flush();

            Writer = new EndianBinaryWriter(new BigEndianBitConverter(), OutputStream);

            Writer.Write(VoltronPacket);
            Writer.Flush();

            return OutputStream.ToArray();
        }

        private AriesHeader ReadAriesHeader(byte[] Buffer)
        {
            AriesHeader Header = new AriesHeader();

            BinaryReader Reader = new BinaryReader(new MemoryStream(Buffer));
            Header.PacketType = Reader.ReadUInt32();
            Header.Timestamp = Reader.ReadUInt32();
            Header.PacketSize = Reader.ReadUInt32() + HEADER_SIZE;
            Reader.Close();

            return Header;
        }

        /// <summary>
        /// Reads a Voltron header from a byte buffer.
        /// </summary>
        /// <param name="Buffer">The buffer to read from.</param>
        /// <param name="Position">The position in the buffer to read from (optional).</param>
        /// <returns>A VoltronHeader instance.</returns>
        private VoltronHeader ReadVoltronHeader(byte[] Buffer, int Position = 0)
        {
            VoltronHeader Header = new VoltronHeader();

            EndianBinaryReader Reader = new EndianBinaryReader(new BigEndianBitConverter(), 
                new MemoryStream(Buffer));

            if (Position != 0)
                Reader.Seek(Position, SeekOrigin.Begin);

            Header.PacketType = Reader.ReadUInt16();
            Header.PacketSize = Reader.ReadUInt32();
            Reader.Close();

            return Header;
        }
    }

    public static class EventHandlerExtensions
    {
        public static void InvokeSafely<T>(this EventHandler<T> eventHandler,
                           object sender, T eventArgs) where T : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(sender, eventArgs);
            }
        }
    }
}
