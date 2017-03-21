namespace TSO_E_Cityserver.Packets.Constants
{
    /// <summary>
    /// Defines possible message IDs for a cTSONetMessageStandard message.
    /// </summary>
    public enum cTSONetMessageStandardMsgIDs : uint
    {
        kDBServiceRequestMessage = 0x3BF82D4E,
        kDBServiceResponseMessage = 0xDBF301A9,
        kCreateAvatarNotificationPDUResponse = 0x7ea33d4d,
        kTopicUpdateSubscriptionRequest = 0x09C83484,
        kTopicUnsubscribeRequest = 0x29C8348F
    }
}
