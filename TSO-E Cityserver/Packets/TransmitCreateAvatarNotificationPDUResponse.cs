using System;
using TSO_E_Cityserver.Packets.Constants;
using TSO_E_Cityserver.Packets.MessageClasses;

namespace TSO_E_Cityserver.Packets
{
    public class TransmitCreateAvatarNotificationPDUResponse : VoltronPacket
    {
        /// <summary>
        /// A response to TransmitCreateAvatarNotificationPDU.
        /// </summary>
        public TransmitCreateAvatarNotificationPDUResponse(byte Badge, byte IsAlertable, 
            short TransactionID1, uint AvatarID) : base(0x2712)
        {
            WriteUInt32(0xfeedf00d); //Hardcoded in the client to mean city server...
            WriteVoltronString("");  //m_ariesID
            WriteVoltronString("");  //m_masterAccount
            WriteByte(Badge);
            WriteByte(IsAlertable);
            WriteUInt32(0x00); //Message size.

            int MessageSizePosition = (6 + VoltronStringSize("") +
                VoltronStringSize(""));

            int DataRequestWrapperHeaderSize = (10 + VoltronStringSize("") + VoltronStringSize(""));

            //TSONetMessageStandard
            WriteUInt32((uint)MessageClsIDs.cTSONetMessageStandard);
            WriteInt16(TransactionID1);
            WriteInt16(0x00); //TransactionID2.
            WriteUInt32(0xfeedf00d); //Hardcoded in the client to mean city server...

            byte Flags = 0;
            Flags |= (byte)cTSONetMessageStandardHasData.HasData2;

            WriteByte(Flags); //Flags
            WriteUInt32((uint)cTSONetMessageStandardMsgIDs.kCreateAvatarNotificationPDUResponse);

            WriteUInt32(AvatarID); //The new avatar's ID is stored in Data2

            WriteUInt32At((uint)(StreamLength() - (12 + 6 + DataRequestWrapperHeaderSize)),
                12 + 6 + MessageSizePosition);
        }
    }
}
