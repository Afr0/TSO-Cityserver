using System;

namespace TSO_E_Cityserver.Database
{
    public class SimProperty
    {
        public uint ID;
        public byte NumOccupants = 0;
        public byte IsOnline = 0;
        public uint LeaderID; //ID of the sim leading the scoreboard for this lot?
        public string Name = "", Description = "";
        public uint LotPrice = 0;
        public byte Category = 0;
    }
}
