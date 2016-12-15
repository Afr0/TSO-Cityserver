using System;
using System.Text;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.Text.RegularExpressions;

namespace TSO_E_Cityserver.Packets
{
    public struct AriesHeader
    {
        public uint PacketType;
        public uint Timestamp;
        public uint PacketSize;
    }

    public struct VoltronHeader
    {
        public ushort PacketType;
        public uint PacketSize;
    }

    public class AriesPacket : IDisposable
    {
        /// <summary>
        /// The type of Aries packet that this is.
        /// </summary>
        protected uint m_PacketType = 0;

        /// <summary>
        /// The size of a packet, in bytes. Does not include header(s) for Aries packets.
        /// </summary>
        protected uint m_PacketSize = 0;

        public uint PacketType { get { return m_PacketType; } }
        public uint PacketSize { get { return m_PacketSize; } }

        protected EndianBinaryReader m_Reader;
        protected EndianBinaryWriter m_Writer;
        protected MemoryStream m_PacketStream = new MemoryStream();

        protected bool m_BigEndian = false;

        /// <summary>
        /// Constructs a new AriesPacket instance for writing data.
        /// </summary>
        public AriesPacket(uint PType)
        {
            m_PacketType = PType;

            m_Writer = new EndianBinaryWriter(new LittleEndianBitConverter(), m_PacketStream);
            WriteUInt32(PType);
            WriteUInt32(0x00); //Timestamp, ignored by client.
            WriteUInt32(0x00); //Temporary packet size.
        }

        /// <summary>
        /// Constructs a new AriesPacket instance for reading data.
        /// </summary>
        /// <param name="Data">The buffer to read from.</param>
        /// <param name="ReadHeader">Whether or not to read the Aries header.</param>
        public AriesPacket(byte[] Data, bool ReadHeader)
        {
            m_PacketStream = new MemoryStream(Data);
            m_Reader = new EndianBinaryReader(new LittleEndianBitConverter(), m_PacketStream);

            if (ReadHeader)
            {
                m_PacketType = ReadUInt32();
                ReadUInt32(); //Timestamp
                m_PacketSize = ReadUInt32();
            }
        }

        /// <summary>
        /// Gets the length of this packet's underlying stream.
        /// </summary>
        /// <returns>A uint representing the length of the stream in bytes.</returns>
        public uint StreamLength()
        {
            return (uint)m_PacketStream.Length;
        }

        #region Writing

        public void WriteByte(byte Value)
        {
            m_Writer.Write(Value);
            m_Writer.Flush();
        }

        /// <summary>
        /// Writes a number of bytes to this packet's underlying stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteBytes(byte[] Data)
        {
            m_Writer.Write(Data, 0, Data.Length);
            m_Writer.Flush();
        }

        /// <summary>
        /// Writes a uint to this packet's underlying stream.
        /// </summary>
        /// <param name="Value">The uint to write.</param>
        public void WriteUInt32(uint Value)
        {
            m_Writer.Write(Value);
            m_Writer.Flush();
        }

        /// <summary>
        /// Writes a ulong to this packet's underlying stream.
        /// </summary>
        /// <param name="Value">The ulong to write.</param>
        public void WriteUInt64(ulong Value)
        {
            m_Writer.Write(Value);
            m_Writer.Flush();
        }

        /// <summary>
        /// Writes a ushort to this packet's underlying stream.
        /// </summary>
        /// <param name="Value">The ushort to write.</param>
        public void WriteUInt16(ushort Value)
        {
            m_Writer.Write(Value);
            m_Writer.Flush();
        }

        /// <summary>
        /// Writes a short to this packet's underlying stream.
        /// </summary>
        /// <param name="Value">The short to write.</param>
        public void WriteInt16(short Value)
        {
            m_Writer.Write(Value);
            m_Writer.Flush();
        }

        /// <summary>
        /// Writes a int to this packet's underlying stream.
        /// </summary>
        /// <param name="Value">The int to write.</param>
        public void WriteInt32(int Value)
        {
            m_Writer.Write(Value);
            m_Writer.Flush();
        }

        /// <summary>
        /// Writes a uint to this packet's underlying stream at a specified position.
        /// </summary>
        /// <param name="Value">The uint to write.</param>
        /// <param name="Position">The position at which to write the value.
        /// Assumes a seek origin from the beginning of the packet's underlying stream.</param>
        public void WriteUInt32At(uint Value, int Position)
        {
            long CurrentPos = m_Writer.BaseStream.Position;
            m_Writer.Seek(Position, SeekOrigin.Begin);
            m_Writer.Write(Value);
            m_Writer.Flush();
            m_Writer.Seek((int)CurrentPos, SeekOrigin.Begin);
        }

        /// <summary>
        /// Writes a Voltron string to this packet's underlying stream.
        /// A Voltron string is a string preceeded by a 4-byte integer 
        /// with the most significant bit flipped specifying the length 
        /// of the string.
        /// </summary>
        /// <param name="Value">The string to write.</param>
        public void WriteVoltronString(string Value)
        {
            m_Writer.Write((uint)(Encoding.UTF8.GetBytes(Value).Length ^ 0x80000000));
            m_Writer.Write(Encoding.UTF8.GetBytes(Value));
            m_Writer.Flush();
        }

        public void WriteString(string Value)
        {
            m_Writer.Write(Value);
            m_Writer.Flush();
        }

        #endregion

        #region Reading

        /// <summary>
        /// Reads an unsigned 64bit integer from the underlying packet stream.
        /// </summary>
        /// <returns>A uint.</returns>
        public ulong ReadUInt64()
        {
            if (!m_BigEndian)
                return m_Reader.ReadUInt64();
            else
                return m_Reader.ReadUInt64();
        }

        /// <summary>
        /// Reads an unsigned 32bit integer from the underlying packet stream.
        /// </summary>
        /// <returns>A uint.</returns>
        public uint ReadUInt32()
        {
            return m_Reader.ReadUInt32();
        }

        /// <summary>
        /// Reads an unsigned 16bit integer from the underlying packet stream.
        /// </summary>
        /// <returns>A ushort.</returns>
        public ushort ReadUInt16()
        {
            if (!m_BigEndian)
                return m_Reader.ReadUInt16();
            else
                return m_Reader.ReadUInt16();
        }

        /// <summary>
        /// Reads a 16bit integer from the underlying packet stream.
        /// </summary>
        /// <returns>A short.</returns>
        public short ReadInt16()
        {
            if (!m_BigEndian)
                return m_Reader.ReadInt16();
            else
                return m_Reader.ReadInt16();
        }

        /// <summary>
        /// Reads a null terminated string from the underlying packet stream.
        /// </summary>
        /// <returns>A string.</returns>
        public string ReadNullString(int Length = 0)
        {
            StringBuilder SBuilder = new StringBuilder();

            int StringLength = 0;

            while(true)
            {
                char Current = (char)m_Reader.ReadByte();
                SBuilder.Append(Current);

                if (Length != 0)
                {
                    StringLength++;

                    if (StringLength == Length)
                        break;
                }
                else
                {
                    if (Current == '\0')
                        break;
                }
            }

            return Regex.Replace(SBuilder.ToString(), @"\s", "").Replace("\0", "");
        }

        /// <summary>
        /// Reads a Voltron string from this packet's underlying stream.
        /// A Voltron string is a string preceeded by a 4-byte integer 
        /// with the most significant bit flipped specifying the length 
        /// of the string.
        /// </summary>
        /// <returns>A string.</returns>
        public string ReadVoltronString()
        {
            uint Length = 0x80000000 ^ m_Reader.ReadUInt32();
            return BitConverter.ToString(m_Reader.ReadBytes((int)Length));
        }

        /// <summary>
        /// Reads a string from the underlying packet stream.
        /// </summary>
        /// <returns>A string.</returns>
        public string ReadString()
        {
            return m_Reader.ReadString();
        }

        /// <summary>
        /// Reads a byte from the underlying packet stream.
        /// </summary>
        /// <returns>A byte.</returns>
        public byte ReadByte()
        {
            return m_Reader.ReadByte();
        }

        /// <summary>
        /// Reads a number of bytes from the underlying packet stream.
        /// </summary>
        /// <param name="Count">The number of bytes to read.</param>
        /// <returns>An array of bytes.</returns>
        public byte[] ReadBytes(int Count)
        {
            return m_Reader.ReadBytes(Count);
        }

        /// <summary>
        /// Reads a number of bytes from the underlying packet stream.
        /// </summary>
        /// <param name="Count">The number of bytes to read.</param>
        /// <param name="LittleEndian">Use little endian encoding when reading?</param>
        /// <returns>An array of bytes.</returns>
        public byte[] ReadBytes(int Count, bool LittleEndian)
        {
            if (!m_BigEndian && LittleEndian == false)
                SwitchEndianNess(new BigEndianBitConverter());

            byte[] Data = m_Reader.ReadBytes(Count);

            if (!m_BigEndian && LittleEndian == false)
                SwitchEndianNess(new LittleEndianBitConverter());

            return Data;
        }

        private void SwitchEndianNess(EndianBitConverter Endianness)
        {
            long CurrentPos = m_Reader.BaseStream.Position;
            Stream CopyOfStream = m_Reader.BaseStream;
            m_Reader = new EndianBinaryReader(Endianness, CopyOfStream);
            m_Reader.Seek((int)CurrentPos, SeekOrigin.Begin);
        }

        #endregion

        /// <summary>
        /// Returns this AriesPacket as an array of bytes.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public virtual byte[] ToArray()
        {
            //If m_Writer is null, it means this packet isn't outgoing.
            if (m_Writer != null)
            {
                //Write the length of the packet before returning the stream.
                if (!m_BigEndian)
                {
                    m_Writer.Seek(8, SeekOrigin.Begin);
                    WriteUInt32((uint)(m_PacketStream.Length) - 12);
                }
                else
                {
                    m_Writer = new EndianBinaryWriter(new LittleEndianBitConverter(), m_PacketStream);
                    m_Writer.Seek(8, SeekOrigin.Begin);
                    WriteUInt32((uint)(m_PacketStream.Length) - 12);
                }

                m_Writer.Seek(0, SeekOrigin.End);
            }

            return m_PacketStream.ToArray();
        }

        public string ToHexString()
        {
            StringBuilder SBuilder = new StringBuilder();
            SoapHexBinary Hex = new SoapHexBinary(ToArray());
            return Hex.ToString();
        }

        public void Dispose()
        {
            if (m_Writer != null)
                m_Writer.Dispose();
            if (m_Reader != null)
                m_Reader.Dispose();
        }
    }
}
