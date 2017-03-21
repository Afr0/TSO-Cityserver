using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using TSO_E_Cityserver.Database;
using TSO_E_Cityserver.Packets.Constants;
using TSO_E_Cityserver.Packets.MessageClasses;
using TSO_E_Cityserver.Refpack;
using log4net;

namespace TSO_E_Cityserver.Packets
{
    public class PacketParser
    {
        private static readonly ILog m_Logger = 
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async static void OnReceivedPacket21(Client Client, AriesPacket Packet)
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

            if (Client.TemporaryAvatarID != 0)
            {
                DatabaseFacade.UpdateAvatarID(Client.TemporaryAvatarID, uint.Parse(AvatarID));
                Client.NewAvatarID = uint.Parse(AvatarID);
            }

            if (SessionIDAcknowledged)
            {
                await Client.SendData(
                    Client.StringPacketsTogether(new VoltronPacket[] { new HostOnlinePDU(),
                    new UpdatePlayerPDU(AvatarID, PlayerAccount) }));

                m_Logger.Info("Sent HostOnlinePDU: " + new HostOnlinePDU().ToHexString());
                m_Logger.Info("Sent UpdatePlayerPDU: " + new UpdatePlayerPDU(AvatarID, PlayerAccount).ToHexString());
            }
            else
                await Client.SendData(new ServerByePDU().ToArray());
        }

        public static void OnReceivedClientOnlinePDU(Client Client, AriesPacket Packet)
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

        public static async void OnReceivedDBServiceWrapperPDU(Client Client, VoltronPacket Packet)
        {
            uint SendingAvatarID = Packet.ReadUInt32();
            uint StringID = Packet.ReadUInt32();

            uint BodySize = Packet.ReadUInt32();
            MessageClsIDs MessageClsID = (MessageClsIDs)Packet.ReadUInt32();

            switch (MessageClsID)
            {
                case MessageClsIDs.cTSONetMessageStandard:
                    Console.WriteLine("Received a cTSONetMessageStandard!");
                    cTSONetMessageStandard NetMessageStandard = Packet.ReadcTSONetMessageStandard();

                    switch(NetMessageStandard.MessageID)
                    {
                        case cTSONetMessageStandardMsgIDs.kTopicUpdateSubscriptionRequest:
                            Console.WriteLine("Received a topic subscription request!");
                            uint TopicID = Packet.ReadUInt32();

                            IEnumerable<Avatar> Avatars = await DatabaseFacade.GetAvatarAsync(SendingAvatarID);
                            string Name = "", Description = "";

                            foreach (Avatar A in Avatars)
                            {
                                if (A.AvatarID == SendingAvatarID)
                                {
                                    Name = A.Name;
                                    Description = A.Description;
                                }
                            }

                            if (!Avatars.GetEnumerator().MoveNext())
                            {
                                Name = Client.NewAvatar.Name;
                                Description = Client.NewAvatar.Description;
                            }

                            switch (TopicID)
                            {
                                case 0xA46E47DC: //My avatar request
                                    await Client.SendData(new MyAvatarResponse(Name, Description).ToArray());
                                    break;
                                case 0x486B3F7E: //Property page request
                                    break;
                                case 0xD042E9D6: //Sim page request
                                    break;
                                case 0xE4E9B25D: //Current city request
                                    break;
                                
                            }
                            break;
                        case cTSONetMessageStandardMsgIDs.kTopicUnsubscribeRequest:
                            Console.WriteLine("Received a topic unsubscription request!");
                            break;
                    }

                    break;
            }
        }

        public static async void OnReceivedDBRequestWrapperPDU(Client Client, VoltronPacket Packet)
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
                    cTSONetMessageStandard NetMessageStandard = Packet.ReadcTSONetMessageStandard();

                    if ((NetMessageStandard.Flags & (1 << 1)) != 0)
                        NetMessageStandard.Data1 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 2)) != 0)
                        NetMessageStandard.Data2 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 3)) != 0)
                        NetMessageStandard.Data3 = Packet.ReadUInt32();
                    if ((NetMessageStandard.Flags & (1 << 4)) != 0)
                        NetMessageStandard.Data4 = Packet.ReadUInt32();
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
                                bool SentLoadAvatarByIDResponse = false;

                                foreach (Avatar Av in Avatars)
                                {
                                    if (Av.AvatarID == AvatarID)
                                    {
                                        LoadAvatarByIDPDUResponse Response =
                                            new LoadAvatarByIDPDUResponse(AvatarID, m_badge,
                                            m_isAlertable, NetMessageStandard.TransactionID1, Av);
                                        await Client.SendData(Response.ToArray());

                                        SentLoadAvatarByIDResponse = true;
                                        m_Logger.Info("Sent LoadAvatarByIDResponsePDU: " +
                                            Response.ToHexString());
                                    }
                                }

                                if(!SentLoadAvatarByIDResponse)
                                {
                                    LoadAvatarByIDPDUResponse Response =
                                        new LoadAvatarByIDPDUResponse(Client.NewAvatarID, m_badge, 
                                        m_isAlertable, NetMessageStandard.TransactionID1, Client.NewAvatar);
                                    await Client.SendData(Response.ToArray());

                                    SentLoadAvatarByIDResponse = true;
                                    m_Logger.Info("Sent LoadAvatarByIDResponsePDU: " +
                                        Response.ToHexString());
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

        public static async void OnReceivedRSGZWrapperPDU(Client Client, VoltronPacket Packet)
        {
            try
            {
                uint SendingAvatarID = Packet.ReadUInt32();
                Packet.ReadUInt32(); //MessageID.
                uint MsgSize = Packet.ReadUInt32();
                MessageClsIDs MessageClsID = (MessageClsIDs)Packet.ReadUInt32();

                switch (MessageClsID)
                {
                    case MessageClsIDs.cTSONetMessageStandard:
                        Console.WriteLine("Received a cTSONetMessageStandard!");
                        cTSONetMessageStandard NetMessageStandard = Packet.ReadcTSONetMessageStandard();

                        if ((NetMessageStandard.Flags & (1 << 1)) != 0)
                            NetMessageStandard.Data1 = Packet.ReadUInt32();
                        if ((NetMessageStandard.Flags & (1 << 2)) != 0)
                            NetMessageStandard.Data2 = Packet.ReadUInt32();
                        if ((NetMessageStandard.Flags & (1 << 3)) != 0)
                            NetMessageStandard.Data3 = Packet.ReadUInt32();
                        if ((NetMessageStandard.Flags & (1 << 4)) != 0)
                            NetMessageStandard.Data4 = Packet.ReadUInt32();
                        if ((NetMessageStandard.Flags & (1 << 5)) != 0)
                        {
                            uint ExtraClsID = Packet.ReadUInt32();
                            NetMessageStandard.ExtraClsID = (ExtraClsRequestIDs)ExtraClsID;

                            switch (NetMessageStandard.ExtraClsID)
                            {
                                case ExtraClsRequestIDs.TSOAvatarCreationRequest:
                                    Console.WriteLine("Received TransmitCreateAvatarNotificationPDU!");
                                    Packet.ReadUInt32(); //Version

                                    Avatar Av = new Avatar();
                                    Av.Name = Packet.ReadString();
                                    Av.Description = Packet.ReadString();

                                    Console.WriteLine("Name:" + Av.Name);
                                    Console.WriteLine("Description: " + Av.Description);

                                    Av.Gender = Packet.ReadByte();
                                    Av.SkinColor = Packet.ReadByte();

                                    Console.WriteLine("Gender: " + ((Av.Gender == 0) ? "male" : "female"));

                                    Av.HeadOutfitID = Packet.ReadUInt64().ToString("X8");
                                    Av.BodyNormalID = Packet.ReadUInt64().ToString("X8");
                                    Av.BodySwimWearID = Packet.ReadUInt64().ToString("X8");
                                    Av.BodySleepWearID = Packet.ReadUInt64().ToString("X8");
                                    Av.BodyNudeID = Packet.ReadUInt64().ToString("X8");

                                    Packet.ReadUInt32(); //ClsID

                                    RefpackStream RefStream = new RefpackStream(Packet.Decompress(), true);
                                    MemoryStream cTSONeighborBlob = (MemoryStream)RefStream.Decompress();
                                    Av.cTSONeighborBlob = cTSONeighborBlob.ToArray();
                                    cTSONeighborBlob.Dispose();

                                    //cTSONeighbor Neighbor = new cTSONeighbor(RefStream.Decompress(), true);

                                    Av.AvatarID = (uint)Guid.NewGuid().ToString().GetHashCode();
                                    Client.TemporaryAvatarID = Av.AvatarID;

                                    Av.PropertyID = 0;
                                    DatabaseFacade.CreateAvatar(Av.AvatarID, Av.Name, Av.Description, 5000, 0, 10, 10,
                                        "AlphaVille", Av.PropertyID, 0x1011, 0x1213, 0x1415, 0x1617, 0x1819, 0x1a1b, 0x1c1d,
                                        0x1e1f, 0x2021, 0x2223, 0x2425, 0x2627, 0x00, 0x00, 5000, Av.HeadOutfitID,
                                        Av.BodyNormalID, Av.BodySwimWearID, Av.BodySleepWearID, Av.BodyNudeID, 0,
                                        0x84858687, 0x88898A8B, 0x8C8D8E8F, 0xbbbc, Av.cTSONeighborBlob);
                                    Client.NewAvatar = Av;

                                    //TODO: Is this thread-safe? Probably not :(
                                    Client.HasCreatedNewAvatar = true;

                                    TransmitCreateAvatarNotificationPDUResponse Response = new TransmitCreateAvatarNotificationPDUResponse(1, 1, 
                                        NetMessageStandard.TransactionID1, Av.AvatarID);
                                    await Client.SendData(Response.ToArray());
                                    m_Logger.Info("Sent TransmitCreateAvatarNotificationPDUResponse: " + Response.ToHexString());

                                    //Send a type 22 packet to retrieve the client's updated account
                                    //with AccountID from the DB (see OnReceivedPacket22).
                                    await Client.SendData(new Type22Packet().ToArray());
                                    m_Logger.Info("Sent Type22 packet!");

                                    break;
                            }
                        }

                        break;
                    }
                }
                catch(Exception E)
                {
                    m_Logger.Info("PacketParser.cs: Failed to parse packet: " + E.ToString());
                }
        }
    }
}
