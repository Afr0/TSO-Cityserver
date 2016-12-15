using System.IO;
using System.Collections.Generic;

namespace TSO_E_Cityserver.Database
{
    public class RelationshipMatrix
    {
        public byte Version;
        public List<List<RelationshipMatrixRow>> Rows = new List<List<RelationshipMatrixRow>>();
    }

    public class RelationshipMatrixRow
    {
        public uint RowValue;
        public List<List<uint>> Columns = new List<List<uint>>();
    }
    
    public class cTSONeighbor
    {
        public uint AvatarID;
        public short[] Globals;
        public RelationshipMatrix RelMatrix = new RelationshipMatrix();

        public cTSONeighbor(Stream DecompressedStream)
        {
            BinaryReader Reader = new BinaryReader(DecompressedStream);

            AvatarID = Reader.ReadUInt32();

            uint UnknownCount1 = Reader.ReadUInt32();

            for(uint i = 0; i < UnknownCount1; i++)
            {
                Reader.ReadUInt32(); //AvatarID
                Reader.ReadUInt32(); //Unknown

                short GlobalsCount = Reader.ReadInt16();
                Globals = new short[GlobalsCount];

                for(short j = 0; j < GlobalsCount; j++)
                    Globals[j] = Reader.ReadInt16();

                uint UnknownCount2 = Reader.ReadUInt32();

                for (uint k = 0; k < UnknownCount2; k++)
                {
                    Reader.ReadSingle();
                    Reader.ReadSingle();
                }

                short Version = Reader.ReadInt16();
                Reader.ReadUInt32(); //Unknown.

                RelMatrix.Version = Reader.ReadByte();
                uint Rows = Reader.ReadUInt32();

                for(uint RowCount = 0; RowCount < Rows; RowCount++)
                {
                    RelationshipMatrixRow Row = new RelationshipMatrixRow();
                    Row.RowValue = Reader.ReadUInt32();
                    uint Columns = Reader.ReadUInt32();

                    for (uint ColumnCount = 0; ColumnCount < Columns; ColumnCount++)
                    {
                        List<uint> Column = new List<uint>();
                        Column.Add(Reader.ReadUInt32());
                        Column.Add(Reader.ReadUInt32());

                        Row.Columns.Add(Column);
                    }
                }
            }
        }
    }
}
