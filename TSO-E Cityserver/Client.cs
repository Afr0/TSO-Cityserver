using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TSO_E_Cityserver.Database;
using TSO_E_Cityserver.Packets;

namespace TSO_E_Cityserver
{
    /// <summary>
    /// A client connected to this server communicating via the Voltron protocol.
    /// </summary>
    public class Client
    {
        private static Configuration Config = 
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private static readonly X509Certificate Certificate =
            X509Certificate.CreateFromCertFile(Config.AppSettings.Settings["Certificate"].Value);

        private SslStream m_sslStream;

        public ConcurrentQueue<AriesPacket> ReceivedPackets = new ConcurrentQueue<AriesPacket>();
        public byte[] Buffer { get; private set; }

        public ClientVersionInfo VersionInfo = new ClientVersionInfo();
        public Account PlayerAccount;

        /// <summary>
        /// Client just created a new avatar and is transfering to the neighborhood screen.
        /// </summary>
        public bool HasCreatedNewAvatar = false;

        public uint TemporaryAvatarID = 0;
        //ID received after client created an avatar.
        public uint NewAvatarID = 0;
        //Newly created avatar (in CAS).
        public Avatar NewAvatar;

        public void ReplaceSslStream(IAsyncResult result, Socket socket)
        {
            if (m_sslStream != null)
            {
                CloseAndDisposeSslStream();
            }

            m_sslStream = new SslStream(new NetworkStream(socket.EndAccept(result), true));
        }

        public void CloseAndDisposeSslStream()
        {
            if (m_sslStream == null)
            {
                return;
            }

            m_sslStream.Close();
            m_sslStream.Dispose();
        }

        public void BeginAuthenticateAsServer(AsyncCallback endAuthenticate)
        {
            m_sslStream.BeginAuthenticateAsServer(Certificate, false, SslProtocols.Default, false,
                endAuthenticate, this);
        }

        public void EndAuthenticateAsServer(IAsyncResult result)
        {
            m_sslStream.EndAuthenticateAsServer(result);
        }

        public void CreateBuffer(int size)
        {
            Buffer = new byte[size];
        }

        public void BeginRead(AsyncCallback receiveData)
        {
            if (Buffer == null)
            {
                throw new ApplicationException("Buffer has not been set.");
            }

            m_sslStream.BeginRead(Buffer, 0, Buffer.Length, receiveData, this);
        }

        public int EndRead(IAsyncResult result)
        {
            return m_sslStream.EndRead(result);
        }

        public bool IsAuthenticated()
        {
            return m_sslStream.IsAuthenticated;
        }

        public bool CanRead()
        {
            return m_sslStream.CanRead;
        }

        /// <summary>
        /// Begins sending data through this SSLSocketEventArgs' SslStream instance.
        /// </summary>
        /// <param name="Buffer">The buffer holding the data to send.</param>
        /// <param name="PacketName">Name of the packet being sent, for logging purposes.</param>
        public async Task SendData(byte[] Buffer)
        {
            await m_sslStream.WriteAsync(Buffer, 0, Buffer.Length);
            await m_sslStream.FlushAsync();
        }

        /// <summary>
        /// Strings several packets together.
        /// </summary>
        /// <param name="Packets">An array of Voltron packets to string together.</param>
        /// <returns>A byte array consisting of the packets string together.</returns>
        public byte[] StringPacketsTogether(VoltronPacket[] Packets)
        {
            MemoryStream MemStream = new MemoryStream();
            BinaryWriter Writer = new BinaryWriter(MemStream);

            foreach (VoltronPacket Pack in Packets)
                Writer.Write(Pack.ToArray());

            return MemStream.ToArray();
        }
    }
}
