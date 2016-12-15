using System;

namespace TSO_E_Cityserver.Database
{
    /// <summary>
    /// Represents an avatar stored in the database.
    /// </summary>
    public class Avatar
    {
        public uint AvatarID; //Sent by the client in LoadAvatarByID.
        public string Name;
        public string Description;
        public int Simoleans;
        public int SimoleanDelta;
        public int Popularity;
        public int PopularityDelta;
        public string ShardName;

        //No idea what these are or how they work, so let's just hardcode them.
        public short SkillLockMechanical = 0x1011;
        public short SkillLockCooking = 0x1213;
        public short SkillLockCharisma = 0x1415;
        public short SkillLockLogic = 0x1617;
        public short SkillLockBody = 0x1819;
        public short SkillLockCreativity = 0x1a1b;

        public short MechanicalSkill;
        public short CookingSkill;
        public short CharismaSkill;
        public short LogicSkill;
        public short BodySkill;
        public short CreativitySkill;

        public byte Gender, SkinColor;
        public uint Cash;
        public string HeadOutfitID, BodyNormalID, BodySwimWearID, BodySleepWearID, BodyNudeID;
        public ushort IsGhost = 0;
        public uint SimBonus, PropertyBonus, VisitorBonus;
        public ushort OnlineJobID;
    }
}
