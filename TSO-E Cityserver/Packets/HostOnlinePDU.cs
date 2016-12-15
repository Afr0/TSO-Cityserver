using System;
using System.Collections.Generic;

namespace TSO_E_Cityserver.Packets
{
    public class HostOnlinePDU : VoltronPacket
    {
        /// <summary>
        /// This buffer length ensures that clients won't send Voltron packets split
        /// across several smaller Aries packets.
        /// </summary>
        private readonly ushort MAXBUFFERSIZE = 32767;

        public HostOnlinePDU() : base(0x001e)
        {
            WriteUInt16(0); //A 2-byte unsigned integer specifying the number of reserved words that follow.
            WriteUInt16(0); //m_hostVersion - A 2-byte unsigned integer; this field is ignored by the client.
            //A 2-byte unsigned integer specifying the maximum size Aries packet that the server can accept.
            WriteUInt16(MAXBUFFERSIZE);
        }
    }
}
