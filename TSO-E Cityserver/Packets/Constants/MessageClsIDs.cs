

namespace TSO_E_Cityserver.Packets.Constants
{
    public enum MessageClsIDs : uint
    {
        cTSONetMessageStandard = 0x125194E5,
        cTSONetMessageStream = 0x125194F5,
        cTSOAvatarCreationRequest = 0x3EA44787,
        cTSOInterDictor = 0xAA3ECCB3,
        cTSOInterdictionPass = 0xAA5FA4D8,
        cTSOInterdictionPassAndLog = 0xCA5FA4E0,
        cTSOInterdictionDrop = 0xCA5FA4E3,
        cTSOInterdictionDropAndLog = 0xCA5FA4EB,
        cTSONetMessageEnvelope = 0xAA7B191E,
        cTSOChannelMessageEnvelope = 0x2A7B4E6A,
        cTSODeadStream = 0x0A9D7E3A,
        cTSOTopicUpdateMessage = 0x09736027,
        cTSODataTransportBuffer = 0x0A2C6585,
        cTSOTopicUpdateErrorMessage = 0x2A404946,
    }
}
