using System;

namespace TSO_E_Cityserver.Packets
{
    /// <summary>
    /// Sent by clients as part of the ClientOnlinePDU packet.
    /// </summary>
    public struct ClientVersionInfo
    {
        public byte m_majorVersion;
        public byte m_minorVersion;
        public byte m_pointVersion;
        public byte m_artVersion;
    }
}
