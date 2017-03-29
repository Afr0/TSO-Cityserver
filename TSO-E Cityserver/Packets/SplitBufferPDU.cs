using System;

namespace TSO_E_Cityserver.Packets
{
    public class SplitBufferPDU : VoltronPacket
    {
        public byte EOF = 0;
        public uint FragmentSize = 0;
        public const byte SplitBufferPDUID = 0x0044;

        public SplitBufferPDU(byte[] Data, bool ReadAriesHeader) : base(Data, ReadAriesHeader)
        {
            EOF = m_Reader.ReadByte();
            FragmentSize = m_Reader.ReadUInt32();
        }
    }
}
