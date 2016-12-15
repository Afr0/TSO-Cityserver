using System;
using System.Reflection;
using System.Collections.Generic;
using TSO_E_Cityserver.Database;
using TSO_E_Cityserver.Packets.MessageClasses;
using TSO_E_Cityserver.Packets.Constants;
using TSO_E_Cityserver.Refpack;
using log4net;

namespace TSO_E_Cityserver.Packets
{
    public class PacketParser
    {
        private static readonly ILog m_Logger = 
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async static void OnReceivedPacket21(SslSocketEventArgs Client, AriesPacket Packet)
        {
            string AvatarID = Packet.ReadNullString(112);
            string AriesClientVersion = Packet.ReadNullString(80);
            string Email = Packet.ReadNullString(40);
            string Authserv = Packet.ReadNullString(84);
            ushort Product = Packet.ReadUInt16();
            Packet.ReadByte(); //Unknown.
            string ServiceIdent = Packet.ReadNullString(3);
            Packet.ReadUInt16(); //Unknown.
            string SessionID = Packet.ReadNullString((int)(Packet.PacketSize - 331));
            Packet.ReadBytes(7); //Reserved.

            Console.WriteLine("AvatarID: " + "'" + AvatarID + "'");
            Console.WriteLine("AriesClientVersion: " + "'" + AriesClientVersion + "'");
            Console.WriteLine("Email: " + "'" + Email + "'");
            Console.WriteLine("Authserv: " + "'" + Authserv + "'");
            Console.WriteLine("ServiceIdent: " + "'" + ServiceIdent + "'");
            Console.WriteLine("SessionID: " + "'" + SessionID + "'");

            bool SessionIDAcknowledged = false;
            Account PlayerAccount = new Account();

            IEnumerable<Account> Accs = await DatabaseFacade.GetAccountsAsync();
            foreach(Account Acc in Accs)
            {
                if (string.Equals(Acc.SessionID, SessionID, StringComparison.CurrentCultureIgnoreCase))
                {
                    SessionIDAcknowledged = true;
                    PlayerAccount = Acc;

                    Console.WriteLine("Valid SessionID found!");
                }
            }

            Client.PlayerAccount = PlayerAccount;

            if (SessionIDAcknowledged)
            {
                await Client.SendData(
                    Client.StringPacketsTogether(new VoltronPacket[] { new HostOnlinePDU(),
                        new UpdatePlayerPDU(AvatarID, PlayerAccount) } ));
                //await Client.SendData(new HostOnlinePDU().ToArray());

                m_Logger.Info("Sent HostOnlinePDU: " + new HostOnlinePDU().ToHexString());
                m_Logger.Info("Sent UpdatePlayerPDU: " + new UpdatePlayerPDU(AvatarID, PlayerAccount).ToHexString());
            }
            else
                await Client.SendData(new ServerByePDU().ToArray());
        }

        public static void OnReceivedClientOnlinePDU(SslSocketEventArgs Client, AriesPacket Packet)
        {
            Client.VersionInfo.m_majorVersion = Packet.ReadByte();
            Client.VersionInfo.m_minorVersion = Packet.ReadByte();
            Client.VersionInfo.m_pointVersion = Packet.ReadByte();
            Client.VersionInfo.m_artVersion = Packet.ReadByte();

            Packet.ReadUInt32(); //Unknown
            uint Time = Packet.ReadUInt32(); //A unix timestamp - time required.
            ushort NumberOfAttempts = Packet.ReadUInt16(); //Number of attempts to connect?

            uint LastExitCode = Packet.ReadUInt32();
            byte LastFailureType = Packet.ReadByte();
            byte FailureCount = Packet.ReadByte();
            byte IsRunning = Packet.ReadByte();
            byte IsRelogging = Packet.ReadByte();
            Packet.ReadByte(); //Unknown.

            Console.WriteLine("Client's version: " + Client.VersionInfo.m_minorVersion.ToString() + "." + 
                Client.VersionInfo.m_majorVersion.ToString());
            Console.WriteLine("Client's point version: " + Client.VersionInfo.m_pointVersion);
            Console.WriteLine("Client's art version: " + Client.VersionInfo.m_artVersion);
        }

        public static void OnReceivedDBServiceWrapperPDU(SslSocketEventArgs Client, VoltronPacket Packet)
        {
            uint SendingAvatarID = Packet.ReadUInt32();
            uint StringID = Packet.ReadUInt32();

            uint BodySize = Packet.ReadUInt32();
            MessageClsIDs MessageClsID = (MessageClsIDs)Packet.ReadUInt32();

            switch (MessageClsID)
            {
                case MessageClsIDs.cTSONetMessageStandard:
                    Console.WriteLine("Received a cTSONetMessageStandard!");
                    cTSONetMessageStandard NetMessageStandard = new cTSONetMessageStandard();
                    NetMessageStandard.TransactionID1 = Packet.ReadInt16();
                    NetMessageStandard.TransactionID2 = Packet.ReadInt16();
                    NetMessageStandard.SendingAvatarID = Packet.ReadUInt32();
                    NetMessageStandard.Flags = Packet.ReadByte();
                    NetMessageStandard.MessageID = (cTSONetMessageStandardMsgIDs)Packet.ReadUInt32();

                    if ((NetMessageStandard.Flags & (1 << 1)) != 0)
                        NetMessageStandard.Data1 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 2)) != 0)
                        NetMessageStandard.Data2 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 3)) != 0)
                        NetMessageStandard.Data3 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 5)) != 0)
                    {
                        NetMessageStandard.ExtraClsID = (ExtraClsRequestIDs)Packet.ReadUInt32();
                        
                        switch(NetMessageStandard.ExtraClsID)
                        {
                            case ExtraClsRequestIDs.UpdatePreferedLanguageByID:
                                uint AvatarID = Packet.ReadUInt32();
                                LanguageCodes LCode = (LanguageCodes)Packet.ReadUInt32();
                                Packet.ReadUInt32(); //Unknown.
                                Packet.ReadBytes(32); //Reserved.
                                Packet.ReadUInt32(); //Unknown.
                                break;
                            case ExtraClsRequestIDs.GetGenericFlash:
                                break;
                            case ExtraClsRequestIDs.Search:
                                break;
                            case ExtraClsRequestIDs.SearchExactMatch:
                                break;
                        }
                    }
                    if ((NetMessageStandard.Flags & (1 << 6)) != 0)
                        NetMessageStandard.Unknown = Packet.ReadString();

                    break;
                case MessageClsIDs.cTSONetMessageStream:
                    Console.WriteLine("Received a cTSONetMessageStream!");
                    cTSONetMessageStream NetMessageStream = new cTSONetMessageStream();
                    NetMessageStream.Unknown1 = Packet.ReadUInt32();
                    NetMessageStream.Unknown2 = Packet.ReadUInt32();
                    NetMessageStream.Unknown3 = Packet.ReadUInt32();
                    NetMessageStream.Unknown4 = Packet.ReadUInt32();
                    NetMessageStream.CompressedData = Packet.ReadBytes((int)(Packet.StreamLength() - 16));

                    //TODO: Decompress data...

                    break;
            }
        }

        public static async void OnReceivedDBRequestWrapperPDU(SslSocketEventArgs Client, VoltronPacket Packet)
        {
            uint SendingAvatarID = Packet.ReadUInt32();
            string m_ariesID = Packet.ReadVoltronString();
            string m_masterAccount = Packet.ReadVoltronString();

            byte m_badge = Packet.ReadByte();
            byte m_isAlertable = Packet.ReadByte();

            uint MsgSize = Packet.ReadUInt32();
            MessageClsIDs MessageClsID = (MessageClsIDs)Packet.ReadUInt32();

            switch(MessageClsID)
            {
                case MessageClsIDs.cTSONetMessageStandard:
                    Console.WriteLine("Received a cTSONetMessageStandard!");
                    cTSONetMessageStandard NetMessageStandard = new cTSONetMessageStandard();
                    NetMessageStandard.TransactionID1 = Packet.ReadInt16();
                    NetMessageStandard.TransactionID2 = Packet.ReadInt16();
                    NetMessageStandard.SendingAvatarID = Packet.ReadUInt32();
                    NetMessageStandard.Flags = Packet.ReadByte();
                    NetMessageStandard.MessageID = (cTSONetMessageStandardMsgIDs)Packet.ReadUInt32();

                    if ((NetMessageStandard.Flags & (1 << 1)) != 0)
                        NetMessageStandard.Data1 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 2)) != 0)
                        NetMessageStandard.Data2 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 3)) != 0)
                        NetMessageStandard.Data3 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 5)) != 0)
                    {
                        NetMessageStandard.ExtraClsID = (ExtraClsRequestIDs)Packet.ReadUInt32();
                        uint AvatarID;

                        switch (NetMessageStandard.ExtraClsID)
                        {
                            case ExtraClsRequestIDs.GetHouseThumbByID:
                                Console.WriteLine("Received a GetHouseThumbByID request!");
                                break;
                            case ExtraClsRequestIDs.GetLotAndObjects:
                                Console.WriteLine("Received a GetLotAndObjects request!");
                                break;
                            case ExtraClsRequestIDs.GetLotList:
                                Console.WriteLine("Received a GetLotList request!");
                                break;
                            case ExtraClsRequestIDs.GetMaxPlayerPerLot:
                                Console.WriteLine("Received a GetMaxPlayerPerLot request!");
                                break;
                            case ExtraClsRequestIDs.GetNeighborhoods:
                                Console.WriteLine("Received a GetNeighborHoods request!");
                                break;
                            case ExtraClsRequestIDs.GetShardVersion:
                                Console.WriteLine("Received a GetShardVersion request!");
                                break;
                            case ExtraClsRequestIDs.GetTopList:
                                Console.WriteLine("Received a GetTopList request!");
                                break;
                            case ExtraClsRequestIDs.GetTopResultSetByID:
                                Console.WriteLine("Received a GetTopResultSetByID request!");
                                break;
                            case ExtraClsRequestIDs.InsertBookmarks:
                                Console.WriteLine("Received a InsertBookmarks request!");
                                break;
                            case ExtraClsRequestIDs.InsertGenericLog:
                                Console.WriteLine("Received a InsertGenericLog request!");
                                break;
                            case ExtraClsRequestIDs.InsertGenericTask:
                                Console.WriteLine("Received a InsertGenericTask request!");
                                break;
                            case ExtraClsRequestIDs.InsertNeighborhoods:
                                Console.WriteLine("Received a InsertNeighborhoods request!");
                                break;
                            case ExtraClsRequestIDs.InsertNewAvatar:
                                Console.WriteLine("Received a InsertNewAvatar request!");
                                break;
                            case ExtraClsRequestIDs.InsertNewFriendshipComment:
                                Console.WriteLine("Received a InsertNewFriendshipComment request!");
                                break;
                            case ExtraClsRequestIDs.InsertPendingRoomateInv:
                                Console.WriteLine("Received a InsertPendingRoomate request!");
                                break;
                            case ExtraClsRequestIDs.InsertSpotlightTextByLotID:
                                Console.WriteLine("Received a InsertSpotlightTextByLotID request!");
                                break;
                            case ExtraClsRequestIDs.MoveOutByAvatarID:
                                Console.WriteLine("Received a MoveOutByAvatarID request!");
                                break;
                            case ExtraClsRequestIDs.LoadAvatarByID:
                                Console.WriteLine("Received a LoadAvatarByID request!");
                                AvatarID = Packet.ReadUInt32();
                                Packet.ReadUInt32(); //Unknown.
                                Packet.ReadBytes(32); //Reserved.
                                Packet.ReadUInt32(); //Unknown.

                                IEnumerable<Avatar> Avatars = await DatabaseFacade.GetAvatarAsync(AvatarID);
                                foreach (Avatar Av in Avatars)
                                {
                                    if (Av.AvatarID == AvatarID)
                                    {
                                        LoadAvatarByIDPDUResponse Response =
                                            new LoadAvatarByIDPDUResponse(AvatarID, m_badge,
                                            m_isAlertable, NetMessageStandard.TransactionID1, Av);
                                        await Client.SendData(Response.ToArray());

                                        m_Logger.Info("Sent LoadAvatarByIDResponsePDU: " +
                                            Response.ToHexString());
                                    }
                                }

                                break;
                            case ExtraClsRequestIDs.MoveLotByID:
                                Console.WriteLine("Received a MoveLotByID request!");
                                break;
                            case ExtraClsRequestIDs.PrtControlToggleByAvatarID:
                                Console.WriteLine("Received a PrtControlToggleByAvatarID request!");
                                break;
                            case ExtraClsRequestIDs.RejectPendingRoomateInv:
                                Console.WriteLine("Received a RejectPendingRoomateInv request!");
                                break;
                            case ExtraClsRequestIDs.ReleaseAvatarLease:
                                Console.WriteLine("Received a ReleaseAvatarLease request!");
                                break;
                            case ExtraClsRequestIDs.SaveAvatarByID:
                                Console.WriteLine("Received a SaveAvatarByID request!");
                                break;
                            case ExtraClsRequestIDs.SaveLotAndObjectBlobByID:
                                Console.WriteLine("Received a SaveLotAndObjectBlobByID request!");
                                break;
                            case ExtraClsRequestIDs.Search:
                                Console.WriteLine("Received a Search request!");
                                break;
                            case ExtraClsRequestIDs.SearchExactMatch:
                                Console.WriteLine("Received a SearchExactMatch request!");
                                break;
                            case ExtraClsRequestIDs.SellObject:
                                Console.WriteLine("Received a SellObject request!");
                                break;
                            case ExtraClsRequestIDs.SetFriendshipComment:
                                Console.WriteLine("Received a SetFriendshipComment request!");
                                break;
                            case ExtraClsRequestIDs.SetHouseByThumbID:
                                Console.WriteLine("Received a SetHouseByThumbID request!");
                                break;
                            case ExtraClsRequestIDs.SetLotDesc:
                                Console.WriteLine("Received a SetLotDesc request!");
                                break;
                            case ExtraClsRequestIDs.SetLotHoursVisitedByID:
                                Console.WriteLine("Received a SetLotHoursVisitedByID request!");
                                break;
                            case ExtraClsRequestIDs.SetLotName:
                                Console.WriteLine("Received a SetLotName request!");
                                break;
                            case ExtraClsRequestIDs.SetMoneyFields:
                                Console.WriteLine("Received a SetMoneyFields request!");
                                break;
                            case ExtraClsRequestIDs.StockDress:
                                Console.WriteLine("Received a StockDress request!");
                                break;
                            case ExtraClsRequestIDs.UpdateBadgeByID:
                                Console.WriteLine("Received a UpdateBadgeByID request!");
                                break;
                            case ExtraClsRequestIDs.UpdateCharDescByID:
                                Console.WriteLine("Received a UpdateCharDescByID request!");
                                break;
                            case ExtraClsRequestIDs.UpdateDataServiceLotAdminInfo_AddAdmittedID:
                                Console.WriteLine("Received a UpdateDataServiceInfo_AddAdmittedID request!");
                                break;
                            case ExtraClsRequestIDs.UpdateDataServiceLotAdminInfo_AddBannedID:
                                Console.WriteLine("Received a UpdateDataServiceInfo_AddBannedID request!");
                                break;
                            case ExtraClsRequestIDs.UpdateDataServiceLotAdminInfo_RemoveAdmittedID:
                                Console.WriteLine("Received a UpdateDataServiceInfo_RemoveAdmittedID request!");
                                break;
                            case ExtraClsRequestIDs.UpdateDataServiceLotAdminInfo_RemoveBannedID:
                                Console.WriteLine("Received a UpdateDataServiceInfo_RemoveBannedID request!");
                                break;
                            case ExtraClsRequestIDs.UpdateDataServiceLotAdminInfo_SetAdmitMode:
                                Console.WriteLine("Received a UpdateDataServiceInfo_SetAdmitMode request!");
                                break;
                            case ExtraClsRequestIDs.UpdateLotValueByID:
                                Console.WriteLine("Received a UpdateLotValueByID request!");
                                break;
                            case ExtraClsRequestIDs.UpdateTaskStatus:
                                Console.WriteLine("Received a UpdateTaskStatus request!");
                                break;
                            case ExtraClsRequestIDs.GetSpotlightLotList:
                                Console.WriteLine("Received a GetSpotlightLotList request!");
                                break;
                            case ExtraClsRequestIDs.GetFinancialDetail:
                                Console.WriteLine("Received a GetFinancialDetail request!");
                                break;
                            case ExtraClsRequestIDs.GetOnlineJobLot:
                                Console.WriteLine("Received a GetOnlineJobLot request!");
                                break;
                            case ExtraClsRequestIDs.GetOnlineJobLotDesactivate:
                                Console.WriteLine("Received a GetOnlineJobLotDesactivate request!");
                                break;
                            case ExtraClsRequestIDs.GetOnlineJobLotRequestDesactivation:
                                Console.WriteLine("Received a GetOnlineJobLotRequestDesactivation request!");
                                break;
                            case ExtraClsRequestIDs.OnlineJobOccupantDesactivation:
                                Console.WriteLine("Received a OnlineJobOccupantDesactivation request!");
                                break;
                            case ExtraClsRequestIDs.UpdatePrivacyModeByID:
                                Console.WriteLine("Received a PrivacyModeByID request!");
                                break;
                            case ExtraClsRequestIDs.GetDataUpdateEventsLastSeqID:
                                Console.WriteLine("Received a GetDataUpdateEventsLastSeqID request!");
                                break;
                            case ExtraClsRequestIDs.GetDataUpdateEvents:
                                Console.WriteLine("Received a GetDataUpdateEvents request!");
                                break;
                            case ExtraClsRequestIDs.GetNeighborhoodInfo:
                                Console.WriteLine("Received a GetNeighborhoodInfo request!");
                                break;
                            case ExtraClsRequestIDs.CallCreateFriends:
                                Console.WriteLine("Received a CallCreateFriends request!");
                                break;
                            case ExtraClsRequestIDs.CallDecayRelationships:
                                Console.WriteLine("Received a CallDecayRelationships request!");
                                break;
                            case ExtraClsRequestIDs.UpdateRelationshipLastContact:
                                Console.WriteLine("Received a UpdateRelationshipLastContact request!");
                                break;
                            case ExtraClsRequestIDs.UpdatePreferedLanguageByID:
                                Console.WriteLine("Received a UpdatePreferedLanguageByID request!");
                                AvatarID = Packet.ReadUInt32();
                                LanguageCodes LCode = (LanguageCodes)Packet.ReadUInt32();
                                Packet.ReadUInt32(); //Unknown.
                                Packet.ReadBytes(32); //Reserved.
                                Packet.ReadUInt32(); //Unknown.

                                Client.PlayerAccount.PreferedLanguageID = (int)LCode;
                                DatabaseFacade.UpdatePreferedLanguageByIDAsync(Client.PlayerAccount.Username, 
                                    LCode);

                                IEnumerable<Account> Accs = await DatabaseFacade.GetAccountsAsync();
                                foreach (Account Acc in Accs)
                                {
                                    Console.WriteLine("Set prefered language to: " + Acc.PreferedLanguageID);
                                }

                                break;
                            case ExtraClsRequestIDs.RenameAvatar:
                                Console.WriteLine("Received a RenameAvatar request!");
                                break;
                            case ExtraClsRequestIDs.GetGenericFlash:
                                break;
                        }
                    }
                    if ((NetMessageStandard.Flags & (1 << 6)) != 0)
                        NetMessageStandard.Unknown = Packet.ReadString();

                    break;
                case MessageClsIDs.cTSONetMessageStream:
                    Console.WriteLine("Received a cTSONetMessageStream!");
                    cTSONetMessageStream NetMessageStream = new cTSONetMessageStream();
                    NetMessageStream.Unknown1 = Packet.ReadUInt32();
                    NetMessageStream.Unknown2 = Packet.ReadUInt32();
                    NetMessageStream.Unknown3 = Packet.ReadUInt32();
                    NetMessageStream.Unknown4 = Packet.ReadUInt32();
                    NetMessageStream.CompressedData = Packet.ReadBytes((int)(Packet.StreamLength() - 16));

                    //TODO: Decompress data...

                    break;
            }
        }

        public static async void OnReceivedRSGZWrapperPDU(SslSocketEventArgs Client, VoltronPacket Packet)
        {
            uint SendingAvatarID = Packet.ReadUInt32();
            Packet.ReadUInt32(); //Unknown.
            uint MsgSize = Packet.ReadUInt32();
            MessageClsIDs MessageClsID = (MessageClsIDs)Packet.ReadUInt32();

            switch (MessageClsID)
            {
                case MessageClsIDs.cTSONetMessageStandard:
                    Console.WriteLine("Received a cTSONetMessageStandard!");
                    cTSONetMessageStandard NetMessageStandard = new cTSONetMessageStandard();
                    NetMessageStandard.TransactionID1 = Packet.ReadInt16();
                    NetMessageStandard.TransactionID2 = Packet.ReadInt16();
                    NetMessageStandard.SendingAvatarID = Packet.ReadUInt32();
                    NetMessageStandard.Flags = Packet.ReadByte();
                    NetMessageStandard.MessageID = (cTSONetMessageStandardMsgIDs)Packet.ReadUInt32();

                    if ((NetMessageStandard.Flags & (1 << 1)) != 0)
                        NetMessageStandard.Data1 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 2)) != 0)
                        NetMessageStandard.Data2 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 3)) != 0)
                        NetMessageStandard.Data3 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 5)) != 0)
                    {
                        uint ExtraClsID = Packet.ReadUInt32();
                        NetMessageStandard.ExtraClsID = (ExtraClsRequestIDs)ExtraClsID;
                        uint AvatarID;

                        switch (NetMessageStandard.ExtraClsID)
                        {
                            case ExtraClsRequestIDs.TransmitCreateAvatarNotificationPDU:
                                Console.WriteLine("Received TransmitCreateAvatarNotificationPDU!");
                                Packet.ReadUInt32(); //SendingAvatarID

                                Avatar Av = new Avatar();
                                Av.Name = Packet.ReadString();
                                Av.Description = Packet.ReadString();

                                Console.WriteLine("Name:" + Av.Name);
                                Console.WriteLine("Description: " + Av.Description);
                                Console.WriteLine(Packet.ReadString());
                                
                                Av.Gender = Packet.ReadByte();
                                Av.SkinColor = Packet.ReadByte();

                                Console.WriteLine("Gender: " + ((Av.Gender == 0) ? "male" : "female"));

                                Av.HeadOutfitID = Packet.ReadUInt64().ToString("X8");
                                Av.BodyNormalID = Packet.ReadUInt64().ToString("X8");
                                Av.BodySwimWearID = Packet.ReadUInt64().ToString("X8");
                                Av.BodySleepWearID = Packet.ReadUInt64().ToString("X8");

                                Packet.ReadUInt32(); //ClsID?

                                RefpackStream RefStream = new RefpackStream(Packet.Decompress(), true);

                                cTSONeighbor Neighbor = new cTSONeighbor(RefStream.Decompress());

                                await Client.SendData(new TransmitCreateAvatarNotificationPDUResponse(Neighbor.AvatarID).ToArray());

                                break;
                        }
                    }

                    break;
            }
        }
    }
}
