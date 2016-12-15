using System;
using System.Text;
using TSO_E_Cityserver.Database;

namespace TSO_E_Cityserver.Packets
{
    public class UpdatePlayerPDU : VoltronPacket
    {
        public UpdatePlayerPDU(string AriesID, Account Acc) : base(0x003d)
        {
            WriteVoltronString("??" + AriesID);
            WriteVoltronString(Acc.Username);
            WriteByte(1); //m_badge - probably founder's badge?
            WriteByte(1); //m_isAlertable - no idea...
        }
    }
}
