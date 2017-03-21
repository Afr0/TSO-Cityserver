using System.Text;
using TSO_E_Cityserver.Packets.Constants;
using TSO_E_Cityserver.Packets.MessageClasses;
using TSO_E_Cityserver.Database;

namespace TSO_E_Cityserver.Packets
{
    /// <summary>
    /// Response to LoadAvatarByID - sent by server.
    /// </summary>
    public class LoadAvatarByIDPDUResponse : VoltronPacket
    {
        public LoadAvatarByIDPDUResponse(uint AvatarID, byte Badge, byte IsAlertable, 
            short TransactionID1, Avatar Sim) : base(0x2712)
        {
            //DataRequestWrapperPDU
            WriteUInt32(0xfeedf00d); //Hardcoded in the client to mean city server...
            WriteVoltronString("");  //m_ariesID
            WriteVoltronString("");  //m_masterAccount
            WriteByte(Badge);
            WriteByte(IsAlertable);
            WriteUInt32(0x00); //Message size.

            int MessageSizePosition = (6 + VoltronStringSize("") +
                VoltronStringSize(""));

            int DataRequestWrapperHeaderSize = (10 + VoltronStringSize("") +
                VoltronStringSize(""));

            //TSONetMessageStandard
            WriteUInt32((uint)MessageClsIDs.cTSONetMessageStandard);
            WriteInt16(TransactionID1);
            WriteInt16(0x00); //TransactionID2.
            WriteUInt32(0xfeedf00d); //Hardcoded in the client to mean city server...

            byte Flags = 0;
            Flags |= (byte)cTSONetMessageStandardHasData.HasExtra;

            WriteByte(Flags); //Flags
            WriteUInt32((uint)cTSONetMessageStandardMsgIDs.kDBServiceResponseMessage);
            WriteUInt32((uint)ExtraClsResponseIDs.LoadAvatarByID);

            //LoadAvatarByID body
            WriteUInt32(AvatarID);
            WriteString(Sim.Name);
            WriteString("B"); //Unknown

            WriteInt16(Sim.SkillLockMechanical);
            WriteInt16(Sim.SkillLockCooking);
            WriteInt16(Sim.SkillLockCharisma);
            WriteInt16(Sim.SkillLockLogic);
            WriteInt16(Sim.SkillLockBody);
            WriteInt16(Sim.SkillLockCreativity);

            WriteInt16(Sim.MechanicalSkill);
            WriteInt16(Sim.CookingSkill);
            WriteInt16(Sim.CharismaSkill);
            WriteInt16(Sim.LogicSkill);
            WriteInt16(Sim.BodySkill);
            WriteInt16(Sim.CreativitySkill);

            //Unknowns.
            WriteUInt32(0x28292a2b);
            WriteUInt32(0x2c2d2e2f);
            WriteUInt32(0x30313233);
            WriteUInt32(0x34353637);
            WriteUInt32(0x38393a3b);
            WriteUInt32(0x3c3d3e3f);
            WriteUInt32(0x40414243);

            WriteByte(Sim.Gender);
            WriteByte(Sim.SkinColor);

            WriteUInt32(Sim.Cash);

            //Unknowns.
            WriteUInt32(0x4a4b4c4d);
            WriteUInt32(0x4e4f5051);
            WriteByte(0x52);

            WriteString(Sim.HeadOutfitID);
            WriteString(Sim.BodyNormalID);
            WriteString(Sim.BodySwimWearID);
            WriteString(Sim.BodySleepWearID);
            WriteString(Sim.BodyNudeID);

            //Unknowns.
            WriteUInt32(0x54555657);
            WriteUInt32(0x58595A5B);
            WriteUInt32(0x5C5D5E5F);
            WriteUInt32(0x60616263);
            WriteUInt32(0x64656667);

            WriteUInt16(0x6869); //Unknown.
            WriteUInt16(Sim.IsGhost);

            //Unknowns.
            WriteUInt32(0x6C6D6E6F);
            WriteUInt32(0x70717273);
            WriteUInt32(0x74757677);

            //Bonus count?
            WriteUInt32(0x00000001);
            //Unknowns.
            WriteUInt32(0x7C7D7E7F);
            WriteUInt32(0x80818283);
            WriteUInt32(Sim.SimBonus);
            WriteUInt32(Sim.PropertyBonus);
            WriteUInt32(Sim.VisitorBonus);

            WriteString("H"); //Date string?

            //Unknowns.
            WriteUInt32(0x00000001);
            WriteUInt32(0x94959697);
            WriteUInt64(0x98999a9b9c9d9e9f);

            //Unknowns.
            WriteUInt32(0x00000001);
            WriteUInt32(0xa5a6a7a8);
            WriteUInt32(0xa9aaabac);
            WriteUInt32(0x00000001);
            WriteUInt16(0xb1b2);
            WriteUInt16(0xb3b4);
            WriteUInt32(0xb5b6b7b8);
            WriteUInt16(0xb9ba);

            WriteUInt16(Sim.OnlineJobID);

            //Unknowns.
            WriteUInt16(0xbdbe);
            WriteUInt32(0xbfc0c1c2);
            WriteUInt32(0xc3c4c5c6);
            WriteUInt32(0x00000001);
            WriteUInt32(0xcbcccdce);
            WriteUInt32(0xcfd0d1d2);
            WriteUInt32(0xd3d4d5d6);
            WriteByte(0xd7);
            WriteByte(0xd8);
            WriteUInt32(0xd9dadbdc);
            WriteUInt32(0xdddedfe0);
            WriteUInt32(0xe1e2e3e4);
            WriteUInt32(0x00000001);
            WriteUInt32(0xe9eaebec);
            WriteUInt32(0xedeeeff0);
            WriteUInt32(0xf1f2f3f4);
            WriteByte(0xf5);
            WriteByte(0xf6);
            WriteUInt32(0xf7f8f9fa);
            WriteUInt32(0xfbfcfdfe);
            WriteUInt32(0xff808182);
            WriteUInt32(0x00000001);
            WriteUInt32(0x8788898a);
            WriteUInt16(0x8b8c);
            WriteUInt32(0x8d8e8f90);
            WriteUInt32(0x91929394);
            WriteUInt32(0x95969798);
            WriteUInt32(0x999a9b9c);
            WriteUInt32(0x00000001);
            WriteUInt16(0x7071);
            WriteUInt64(0x7273747576777879);
            WriteUInt32(0x7a7b7c7d);
            WriteUInt32(0x7e7f6061);
            WriteUInt32(0x62636465);
            WriteUInt32(0x66676869);
            WriteUInt32(0x6a6b6c6d);
            WriteUInt32(0x6e6f5051);
            WriteUInt32(0x52535455);
            WriteUInt32(0x56575859);
            WriteUInt32(0x69f4d5e8);
            WriteUInt32(0x5e5f4041);

            //Aries header = 12 bytes, Voltron header = 6 bytes.
            WriteUInt32At((uint)(StreamLength() - (12 + 6 + DataRequestWrapperHeaderSize)), 
                12 + 6 + MessageSizePosition);
        }
    }
}
