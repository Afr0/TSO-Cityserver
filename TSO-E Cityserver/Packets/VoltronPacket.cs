﻿using System;
using System.Text;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;
using TSO_E_Cityserver.Refpack;
using TSO_E_Cityserver.Packets.MessageClasses;
using TSO_E_Cityserver.Packets.Constants;

namespace TSO_E_Cityserver.Packets
{
    public class VoltronPacket : AriesPacket
    {
        public ushort VoltronPacketType;
        public uint VoltronPacketSize;

        /// <summary>
        /// Constructs a new VoltronPacket for writing data.
        /// </summary>
        /// <param name="PacketType">The type of packet that this is.</param>
        public VoltronPacket(ushort PacketType) : base(0)
        {
            m_BigEndian = true;
            m_Writer = new EndianBinaryWriter(new BigEndianBitConverter(), m_PacketStream);

            VoltronPacketType = PacketType;
            WriteUInt16(VoltronPacketType);
            WriteUInt32(0x00); //Temporary packet size.
        }

        /// <summary>
        /// Constructs a new VoltronPacket for reading data.
        /// </summary>
        /// <param name="Data">The buffer to read from.</param>
        /// <param name="ReadAriesHeader">Whether or not to read the Aries header.</param>
        public VoltronPacket(byte[] Data, bool ReadAriesHeader) : base(Data, ReadAriesHeader)
        {
            m_BigEndian = true;
            m_Reader = new EndianBinaryReader(new BigEndianBitConverter(), m_PacketStream);
            VoltronPacketType = ReadUInt16();
            VoltronPacketSize = ReadUInt32();
        }

        /// <summary>
        /// Reads a Stream header and returns a decompressed Refpack body.
        /// </summary>
        public Stream Decompress()
        {
            try
            {
                byte BodyType = ReadByte();
                uint DecompressedSize = ReadUInt32();

                if (BodyType == 0)
                    return new MemoryStream(ReadBytes((int)DecompressedSize));
                else
                {
                    uint CompressedSize = ReadUInt32();
                    //This is *always* in little endian, regardless of the rest
                    //of the stream, probably to avoid switching, because its
                    //value is the same as the above field.
                    SwitchEndianNess(new LittleEndianBitConverter());
                    uint StreamSize = ReadUInt32();

                    //For some reason, reading the RefPak header inside the decompresser causes it to crash.
                    //So read it here instead.
                    ReadBytes(5);

                    Decompresser Dec = new Decompresser();
                    Dec.DecompressedSize = DecompressedSize;
                    //CompressedSize includes the size of itself (4 bytes) and the RefPak header (5 bytes).
                    byte[] DecompressedData = Dec.Decompress(ReadBytes((int)CompressedSize - 9));

                    SwitchEndianNess(new BigEndianBitConverter());

                    return new MemoryStream(DecompressedData);
                }
            }
            catch (Exception E)
            {
                throw new Exception(E.ToString());
            }
        }

        /// <summary>
        /// Reads and returns a cTSONetMessageStandard header.
        /// </summary>
        /// <returns>A cTSONetMessageStandard instance.</returns>
        public cTSONetMessageStandard ReadcTSONetMessageStandard()
        {
            cTSONetMessageStandard NetMessageStandard = new cTSONetMessageStandard();
            NetMessageStandard.TransactionID1 = ReadInt16();
            NetMessageStandard.TransactionID2 = ReadInt16();
            NetMessageStandard.SendingAvatarID = ReadUInt32();
            NetMessageStandard.Flags = ReadByte();
            NetMessageStandard.MessageID = (cTSONetMessageStandardMsgIDs)ReadUInt32();
            return NetMessageStandard;
        }

        private void SwitchEndianNess(EndianBitConverter Endianness)
        {
            long CurrentPos = m_Reader.BaseStream.Position;
            Stream CopyOfStream = m_Reader.BaseStream;
            m_Reader = new EndianBinaryReader(Endianness, CopyOfStream);
            m_Reader.Seek((int)CurrentPos, SeekOrigin.Begin);
        }

        /// <summary>
        /// Gets the size in bytes of a string encoded as a Voltron string.
        /// </summary>
        /// <param name="Str">The string.</param>
        protected int VoltronStringSize(string Str)
        {
            return 4 + Encoding.UTF8.GetBytes(Str).Length;
        }

        public override byte[] ToArray()
        {
            base.ToArray();

            //Write the length of the packet before returning the stream.
            m_Writer = new EndianBinaryWriter(new BigEndianBitConverter(), m_PacketStream);
            m_Writer.Seek(14, SeekOrigin.Begin);
            WriteUInt32((uint)(m_PacketStream.Length - 12));
            m_Writer.Flush();
            m_Writer.Seek(0, SeekOrigin.End);

            return m_PacketStream.ToArray();
        }
    }
}
