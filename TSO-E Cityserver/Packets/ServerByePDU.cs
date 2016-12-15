using System;

namespace TSO_E_Cityserver.Packets
{
    public class ServerByePDU : VoltronPacket
    {
        public ServerByePDU(string ReasonCode = "You have been logged out due to inactivity.\n Please log in again.") : base(0x0007)
        {
            WriteInt32(-1); //m_reasonCode.
            WriteVoltronString(ReasonCode);
            WriteVoltronString("Wrong ticket!"); //m_RequestTicket - no idea what this is supposed to be.
        }
    }
}
