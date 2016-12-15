using System;
using TSO_E_Cityserver.Packets.Constants;

namespace TSO_E_Cityserver.Packets.MessageClasses
{
    /// <summary>
    /// Encapsulates the information sent in a DBRequestWrapperPDU packet
    /// using the cTSONetMessageStandard message ID.
    /// </summary>
    public class cTSONetMessageStandard
    {
        public short TransactionID1, TransactionID2;
        public uint SendingAvatarID;
        public byte Flags;
        public cTSONetMessageStandardMsgIDs MessageID;
        public ExtraClsRequestIDs ExtraClsID;
        public byte[] ExtraBody;
        public string Unknown;
        public uint Data1, Data2, Data3, Data4;
    }

    public enum cTSONetMessageStandardHasData
    {
        HasData1 = (1 << 1),
        HasData2 = (1 << 2),
        HasData3 = (1 << 3),
        HasData4 = (1 << 4),
        HasExtra = (1 << 5)
    }
}
