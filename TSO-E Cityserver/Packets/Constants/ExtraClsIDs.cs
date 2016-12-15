

namespace TSO_E_Cityserver.Packets.Constants
{
    /// <summary>
    /// These values can be found in the ExtraClsID field of a cTSONetMessageStandard request.
    /// </summary>
    public enum ExtraClsRequestIDs : uint
    {
        GetHouseThumbByID =          0x9BF18F10,
        GetLotAndObjects =           0x0BFD89E3,
        GetLotList =                 0x5BEEB701,
        GetMaxPlayerPerLot =         0x5F0A0561,
        GetNeighborhoods =           0x8AE0FD8C,
        GetShardVersion =            0x5E209378,
        GetTopList =                 0x3D8787DA,
        GetTopResultSetByID =        0xBCD038AC,
        InsertBookmarks =            0x09CBE333,
        InsertGenericLog =           0x3D03D5F7,
        InsertGenericTask =          0xC98B6799,
        InsertNeighborhoods =        0xAAE1247E,
        InsertNewAvatar =            0x8A3BE831,
        InsertNewFriendshipComment = 0x6AE8E1ED,
        InsertPendingRoomateInv =    0x3CE98067,
        InsertSpotlightTextByLotID = 0x8B8AE566,
        MoveOutByAvatarID =          0x4CEEB62C,
        LoadAvatarByID =             0x2ADF7EED,
        MoveLotByID =                0xEB42651A,
        PrtControlToggleByAvatarID = 0x8A53F433,
        RejectPendingRoomateInv =    0xDCE98959,
        ReleaseAvatarLease =         0x6B9EAECD,
        RenewAvatarLease =           0xCB9EAF2F,
        SaveAvatarByID =             0x2ADDE378,
        SaveLotAndObjectBlobByID =   0xCBFD89A7,
        Search =                     0x89483786,
        SearchExactMatch =           0xA952742D,
        SellObject =                 0x8BFD896A,
        SetFriendshipComment =       0x0AE0AEB8,
        SetHouseByThumbID =          0xFBF6E364,
        SetLotDesc =                 0x8A70B952,
        SetLotHoursVisitedByID =     0x7C02938C,
        SetLotName =                 0x6A70B931,
        SetMoneyFields =             0x5CF147E8,
        StockDress =                 0x2B4510AC,
        UpdateBadgeByID =            0xCAFB30AA,
        UpdateCharDescByID =         0xAA3FEDA1,
        UpdateDataServiceLotAdminInfo_AddAdmittedID = 0xCA26E9CF,
        UpdateDataServiceLotAdminInfo_AddBannedID = 0xEA26E9F8,
        UpdateDataServiceLotAdminInfo_RemoveAdmittedID = 0xEA26E9E4,
        UpdateDataServiceLotAdminInfo_RemoveBannedID = 0x0A26EA0C,
        UpdateDataServiceLotAdminInfo_SetAdmitMode = 0xCA26E9BD,
        UpdateLotValueByID         = 0xDC17FB0E,
        UpdateTaskStatus           = 0xA92AF562,
        GetSpotlightLotList        = 0xEBD4DDAC,
        GetFinancialDetail         = 0x0C23D673,
        GetOnlineJobLot            = 0x8C3BBA00,
        GetOnlineJobLotDesactivate = 0xAC50944B,
        GetOnlineJobLotRequestDesactivation = 0x0C9D233E,
        OnlineJobOccupantDesactivation = 0xAC96E1AE,
        UpdatePrivacyModeByID      = 0xA0C6106C,
        GetDataUpdateEventsLastSeqID = 0xCBB127FD,
        GetDataUpdateEvents        = 0x0B5E2124,
        GetNeighborhoodInfo        = 0xCB7AD7EE,
        CallCreateFriends          = 0x21586E78,
        CallDecayRelationships     = 0xE1586F32,
        UpdateRelationshipLastContact = 0x2D33ABF3,
        UpdatePreferedLanguageByID = 0x2D98FAF3,
        RenameAvatar               = 0x1060F3A1,

        GetGenericFlash =            0x9FE8B670,
        GetAvatarIDByName =          0xA95BB3D7,
        TransmitCreateAvatarNotificationPDU = 0x3EA44787
    }

    /// <summary>
    /// These values can be found in the ExtraClsID field of a cTSONetMessageStandard response.
    /// </summary>
    public enum ExtraClsResponseIDs : uint
    {
        SaveAvatarByID = 0x2ADDE27E,
        LoadAvatarByID = 0x2ADF8FF5,
        GetGenericFlash = 0x3FE8B5A4,
        GetAvatarIDByName = 0xE95BB42F,
        Search = 0xC94837CC,
        SearchExactMatch = 0x89527401,
        SellObject = 0x8BFD895F,
        TransmitCreateAvatarNotificationPDU = 0x3EA44787
    }
}
