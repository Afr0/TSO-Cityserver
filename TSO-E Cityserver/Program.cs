using System;
using System.Configuration;
using System.Collections.Concurrent;
using TSO_E_Cityserver.Database;
using TSO_E_Cityserver.Packets;

namespace TSO_E_Cityserver
{
    class Program
    {
        private static SslSocketMonitor m_SSLSocket;
        private static int m_ConnectionIDs = 0;
        private static ConcurrentDictionary<int, Client> m_Sockets = 
            new ConcurrentDictionary<int, Client>();

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection Settings = Config.AppSettings.Settings;

            if (Settings["Certificate"] == null)
                //Change this to include full address depending on platform/environment.
                Settings.Add("Certificate", "selfsigned.cer");
            else
                Settings["Certificate"].Value = "selfsigned.cer";

            if (Settings["AccountDatasource"] == null)
                Settings.Add("AccountsDatasource", "C:\\Accounts.db");
            else
                Settings["AccountDatasource"].Value = "C:\\Accounts.db";

            Config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(Config.AppSettings.SectionInformation.Name);

            Console.WriteLine("Donald Trump has Tiny Hands, a TSO Server Emulator, v. 1.0");
            Console.WriteLine("Creating SQLite Database...");

            if (!DatabaseFacade.Initialize())
            {
                Console.ReadLine();
                Environment.Exit(0);
            }

            Console.WriteLine("Success!\n Creating tables...");

            if(!DatabaseFacade.CreateTables())
            {
                Console.ReadLine();
                Environment.Exit(0); 
            }

            DatabaseFacade.CreateServer(1, "Alphaville", 49, 1, 6, "Up", 2, 0, "Afr0", "Happy hacking!",
                "Welcome to Alphaville, hosted by Donald Trump Has Tiny Hands!");

            Console.WriteLine("Success!\n Creating master account...");
            DatabaseFacade.CreateAccount(1, "asdf", "hjkl", 10, 1337, 0, 0);

            //Test...
            DatabaseFacade.CreateAvatar(1337, "Donald Trump", "", 5000, 0, 10, 10, "AlphaVille", 0,
                0x1011, 0x1213, 0x1415, 0x1617, 0x1819, 0x1a1b, 0x1c1d, 0x1e1f, 0x2021, 0x2223, 0x2425,
                0x2627, 0x00, 0x00, 5000, /*"0x000003A10000000D"*/"0x0000018D0000000D", /*"0x0000024A0000000D"*/"0x0000030000000D", "0x000002B70000000D", 
                "0x000002B60000000D","0x0000024E0000000D", 0, 0x84858687, 0x88898A8B, 0x8C8D8E8F, 0xbbbc, 
                new byte[] { 0, 0, 0 });

            Console.WriteLine("Success!");

            m_SSLSocket = new SslSocketMonitor(49100); //This is TSO Cityserver's port.
            m_SSLSocket.Connected += M_SSLSocket_Connected;
            m_SSLSocket.ReceivedData += M_SSLSocket_ReceivedData;
            m_SSLSocket.InitializeSocket();
        }

        private static async void M_SSLSocket_Connected(object sender, Client Client)
        {
            if (Client.IsAuthenticated())
            {
                Console.WriteLine("Received new authenticated connection!");
                m_Sockets.TryAdd(m_ConnectionIDs, Client);

                //This is static, so can only be accessed by one thread at a time.
                m_ConnectionIDs++;

                await Client.SendData(new Type22Packet().ToArray());
            }
        }

        private static void M_SSLSocket_ReceivedData(object sender, Client Client)
        {
            AriesPacket ReceivedPacket;
            VoltronPacket VPacket;

            while (Client.ReceivedPackets.TryDequeue(out ReceivedPacket))
            {
                switch (ReceivedPacket.PacketType)
                {
                    case 0:
                        VPacket = (VoltronPacket)ReceivedPacket;

                        switch (VPacket.VoltronPacketType)
                        {
                            case 0x000a:
                                Console.WriteLine("Received ClientOnlinePDU!");
                                PacketParser.OnReceivedClientOnlinePDU(Client, VPacket);
                                break;
                            case 0x0032:
                                Console.WriteLine("Received SetAcceptAlertsPDU!");
                                break;
                            case 0x0034:
                                Console.WriteLine("Received SetIgnoreListPDU!");
                                break;
                            case 0x0036:
                                Console.WriteLine("Received SetInvinciblePDU!");
                                break;
                            case 0x0038:
                                Console.WriteLine("Received SetInvisiblePDU!");
                                break;
                            case 0x0042:
                                Console.WriteLine("Received SetAcceptFlashesPDU!");
                                break;
                            case 0x0051:
                                Console.WriteLine("Received GetMPSMessagesPDU!");
                                break;
                            case 0x0071:
                                Console.WriteLine("Received ResetWatchdogPDU!");
                                break;
                            case 0x2712:
                                Console.WriteLine("Received DBRequestWrapperPDU!");
                                PacketParser.OnReceivedDBRequestWrapperPDU(Client, VPacket);
                                break;
                            case 0x2729:
                                Console.WriteLine("Received ComponentVersionRequestPDU!");
                                break;
                            case 0x2730:
                                Console.WriteLine("Received RSGZWrapperPDU!");
                                PacketParser.OnReceivedRSGZWrapperPDU(Client, VPacket);
                                break;
                            case 0x2734:
                                Console.WriteLine("Received DataServiceWrapperPDU!");
                                PacketParser.OnReceivedDBServiceWrapperPDU(Client, VPacket);
                                break;
                            case 0x271e:
                                Console.WriteLine("Received GenericFlashRequestPDU!");
                                break;
                            default:
                                Console.WriteLine("Voltron packet type: " + VPacket.VoltronPacketType.ToString("X2"));
                                break;
                        }
                        break;
                    case 1:
                        Console.WriteLine("Received a type 1 packet - client disconnected!");
                        break;
                    case 3:
                        Console.WriteLine("Received a type 3 packet - client transfered to another server!");
                        break;
                    case 21:
                        Console.WriteLine("Received a type 21 packet!");
                        PacketParser.OnReceivedPacket21(Client, ReceivedPacket);
                        break;
                    case 26:
                        Console.WriteLine("Received a type 26 packet - client experienced an error!");
                        break;
                    case 28:
                        Console.WriteLine("Received a type 28 packet - client sent a ping!");
                        break;
                    case 29:
                        Console.WriteLine("Received a type 29 packet!");
                        break;
                }
            }
        }
    }
}
