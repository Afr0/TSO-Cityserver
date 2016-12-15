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

        public event EventHandler<SslSocketEventArgs> Connected;
        public event EventHandler<SslSocketEventArgs> Disconnected;
        public event EventHandler<SslSocketEventArgs> ReceivedData;

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
                m_Socket.BeginAccept(AcceptConnections, new SslSocketEventArgs());

                //Wait until a connection is made before continuing.
                m_ConnectionResetEvent.WaitOne();
            }
        }

        private void AcceptConnections(IAsyncResult result)
        {
            var resultWrapper = (SslSocketEventArgs)result.AsyncState;
            try
            {
                resultWrapper.ReplaceSslStream(result, m_Socket);
                resultWrapper.BeginAuthenticateAsServer(EndAuthenticate);

                m_Socket.BeginAccept(AcceptConnections, new SslSocketEventArgs());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                if (e.InnerException != null)
                    Console.WriteLine(e.InnerException);

                Disconnected.InvokeSafely(this, resultWrapper);
                resultWrapper.CloseAndDisposeSslStream();
            }
        }

        private void EndAuthenticate(IAsyncResult result)
        {
            var resultWrapper = (SslSocketEventArgs)result.AsyncState;
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
                    Connected.InvokeSafely(this, resultWrapper);
                    if (resultWrapper.CanRead())
                    {
                        //resultWrapper.CreateBuffer(5);
                        resultWrapper.CreateBuffer(BUFFER_SIZE);
                        resultWrapper.BeginRead(ReceiveData);
                    }
                }
                else
                {
                    Disconnected.InvokeSafely(this, resultWrapper);
                    resultWrapper.CloseAndDisposeSslStream();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Disconnected.InvokeSafely(this, resultWrapper);
                resultWrapper.CloseAndDisposeSslStream();
            }
        }

        //Packet type (8), timestamp (8) and payload size (8)
        private static readonly uint HEADER_SIZE = 12;
        private int m_CurrentlyReceived = 0;

        private void ReceiveData(IAsyncResult result)
        {
            var resultWrapper = (SslSocketEventArgs)result.AsyncState;
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
                Disconnected.InvokeSafely(this, resultWrapper);
                resultWrapper.CloseAndDisposeSslStream();
            }
        }

        private void ProcessBuffer(SslSocketEventArgs Client)
        {
            while(m_CurrentlyReceived >= PacketSize)
            {
                byte[] PacketBuf = new byte[PacketSize];
                Array.Copy(Client.Buffer, PacketBuf, PacketSize);

                if (PacketType != 0)
                {
                    lock (Client.ReceivedPackets)
                        Client.ReceivedPackets.Enqueue(new AriesPacket(PacketBuf, true));

                    ReceivedData.InvokeSafely(this, Client);

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

                        ReceivedData.InvokeSafely(this, Client);
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
        private void ProcessVoltronPackets(SslSocketEventArgs Client, byte[] PacketBuf)
        {
            VoltronHeader Header = ReadVoltronHeader(PacketBuf, 12);
            //bool ReadAriesHeader = true;

            MemoryStream AriesStream = new MemoryStream(PacketBuf);
            EndianBinaryReader Reader = new EndianBinaryReader(new BigEndianBitConverter(), AriesStream);
            int Remaining = (int)AriesStream.Length - 12;

            Reader.ReadBytes(12); //Aries header.


            while (Header.PacketSize < Remaining)
            {
                VoltronPacket Packet = new VoltronPacket(Reader.ReadBytes((int)Header.PacketSize), false);

                lock (Client.ReceivedPackets)
                    Client.ReceivedPackets.Enqueue(Packet);

                Remaining -= (int)Header.PacketSize;

                if(Header.PacketSize < Remaining)
                    Header = ReadVoltronHeader(AriesStream.ToArray(), (int)(AriesStream.Position));
            }

            Reader.Close();
            ReceivedData.InvokeSafely(this, Client);
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
