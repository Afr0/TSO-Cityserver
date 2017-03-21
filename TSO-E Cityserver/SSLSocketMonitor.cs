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
        private uint PacketType, PacketSize;

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

        private void ReceiveData(IAsyncResult result)
        {
            var resultWrapper = (Client)result.AsyncState;
            try
            {
                var size = resultWrapper.EndRead(result);
                m_CurrentlyReceived += size;

                if (m_CurrentlyReceived >= HEADER_SIZE)
                {
                    AriesHeader Header = ReadAriesHeader((byte[])resultWrapper.Buffer.Clone());
                    PacketType = Header.PacketType;
                    PacketSize = Header.PacketSize;

                    if(m_CurrentlyReceived >= PacketSize)
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

        private void ProcessBuffer(Client Client)
        {
            while(m_CurrentlyReceived >= PacketSize)
            {
                byte[] PacketBuf = new byte[PacketSize];
                Array.Copy(Client.Buffer, PacketBuf, PacketSize);

                if (PacketType != 0)
                {
                    lock (Client.ReceivedPackets)
                        Client.ReceivedPackets.Enqueue(new AriesPacket(PacketBuf, true));

                    ReceivedData?.Invoke(this, Client);

                    m_CurrentlyReceived -= (int)PacketSize;
                }
                else
                {
                    VoltronHeader Header = ReadVoltronHeader(PacketBuf, 12);

                    if (Header.PacketSize < (PacketBuf.Length - 12))
                        ProcessVoltronPackets(Client, PacketBuf);
                    else
                    {
                        lock (Client.ReceivedPackets)
                            Client.ReceivedPackets.Enqueue(new VoltronPacket(PacketBuf, true));

                        ReceivedData?.Invoke(this, Client);
                    }

                    m_CurrentlyReceived -= (int)PacketSize;
                }

                if (m_CurrentlyReceived > 0)
                {
                    byte[] Remainder = new byte[m_CurrentlyReceived];
                    Array.ConstrainedCopy(Client.Buffer, (Client.Buffer.Length - m_CurrentlyReceived) + 1,
                        Remainder, 0, m_CurrentlyReceived);

                    //Recreate the packet buffer and copy the remainder back into it.
                    Client.CreateBuffer(BUFFER_SIZE);
                    Array.Copy(Remainder, Client.Buffer, m_CurrentlyReceived);
                    Remainder = null;
                }
                else
                    Client.CreateBuffer(BUFFER_SIZE);
            }
        }

        /// <summary>
        /// Multiple Voltron packets were sent in a Aries frame.
        /// </summary>
        /// <param name="PacketBuf">The packet buffer containing the packets to process.</param>
        private void ProcessVoltronPackets(Client Client, byte[] PacketBuf)
        {
            VoltronHeader Header = ReadVoltronHeader(PacketBuf, 12);

            MemoryStream AriesStream = new MemoryStream(PacketBuf);
            EndianBinaryReader Reader = new EndianBinaryReader(new BigEndianBitConverter(), AriesStream);
            int Remaining = (int)AriesStream.Length - 12;

            byte[] AriesHeader = Reader.ReadBytes(12); //Aries header.

            while (Header.PacketSize < Remaining)
            {
                byte[] VoltronBody = Reader.ReadBytes((int)Header.PacketSize);
                VoltronPacket Packet = new VoltronPacket(ReconstructVoltronPacket(AriesHeader, 
                    VoltronBody), true);

                lock (Client.ReceivedPackets)
                    Client.ReceivedPackets.Enqueue(Packet);

                Remaining -= (int)Header.PacketSize;

                if(Header.PacketSize < Remaining)
                    Header = ReadVoltronHeader(AriesStream.ToArray(), (int)(AriesStream.Position));
            }

            Reader.Close();
            ReceivedData?.Invoke(this, Client);
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
            BinaryWriter Writer = new BinaryWriter(OutputStream);

            Writer.Write(AriesHeader);
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
