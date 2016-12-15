using System;

namespace TSO_E_Cityserver.Packets.MessageClasses
{
    /// <summary>
    /// Encapsulates the information sent in a DBRequestWrapperPDU packet
    /// using the cTSONetMessageStream message ID.
    /// </summary>
    public class cTSONetMessageStream
    {
        public uint Unknown1, Unknown2, Unknown3, Unknown4;
        public byte[] CompressedData;
    }
}
