using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace TSO_E_Cityserver.Packets
{
    /// <summary>
    /// This is the first packet sent by the server to the client.
    /// </summary>
    public class Type22Packet : AriesPacket
    {
        public Type22Packet() : base(22)
        {
        }
    }
}
