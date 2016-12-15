using System;

namespace TSO_E_Cityserver.Packets
{
    public class TransmitCreateAvatarNotificationPDUResponse : VoltronPacket
    {
        /// <summary>
        /// A response to TransmitCreateAvatarNotificationPDU.
        /// </summary>
        /// <param name="AvatarID">The ID of the avatar that the client created.</param>
        public TransmitCreateAvatarNotificationPDUResponse(uint AvatarID) : base(0x2730)
        {
            WriteUInt32(AvatarID); //AvatarID
        }
    }
}
