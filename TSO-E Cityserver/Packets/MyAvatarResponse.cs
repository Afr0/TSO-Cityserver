using System;
using TSO_E_Cityserver.Packets.Constants;

namespace TSO_E_Cityserver.Packets
{
    public class MyAvatarResponse : VoltronPacket
    {
        public MyAvatarResponse(string Name, string Description) : base(0x2734)
        {
            /* MyAvatar.Avatar_Name packet */
            /*27 34 - Voltron PDU Type: 0x2734(DataServiceWrapperPDU)
              00 00 00 40 - Voltron PDU Size: 0x00000040 bytes
              FE ED F0 0D - DataServiceWrapperPDU.SendingAvatarID: 0xfeedf00d
              A4 6E 47 DC - DataServiceWrapperPDU.StringID: MyAvatar(0xA46E47DC)
              00 00 00 2E - DataServiceWrapperPDU.BodySize: 0x0000002e bytes
              09 73 60 27 - DataServiceWrapperPDU.BodyClsid: GZCLSID_cTSOTopicUpdateMessage(0x09736027)
              0F E8 80 3B - DataServiceWrapperPDU.Body.UpdateCounter: (0x0fe8, 0x803b)
              A9 73 60 C5 - DataServiceWrapperPDU.Body.MessageID: GZMSGID_cITSOApprovedTopicUpdate(0xA97360C5)
              00 00 00 00 - DataServiceWrapperPDU.Body.m_statusCode: 0x00000000
              00 00 00 03 - DataServiceWrapperPDU.Body.VectorSize: 0x00000003
              05 60 03 32 - DataServiceWrapperPDU.Body.Vector[0]: Avatar(0x05600332)
              00 00 05 39 - DataServiceWrapperPDU.Body.Vector[1]: 0x00000539
              4F FE 27 0C - DataServiceWrapperPDU.Body.Vector[2]: Avatar_Name(0x4FFE270C)
              89 6D 16 88 - DataServiceWrapperPDU.Body.cTSOValueClsid: 0x896D1688(cTSOValue <class cRZAutoRefCount<class cIGZString> >)
              08 4A 6F 6C 6C 79 53 69 6D - DataServiceWrapperPDU.Body.cTSOValue.Value: "JollySim"
              00          - DataServiceWrapperPDU.Body.m_reasonText: ""*/

            WriteUInt32(0xfeedf00d); //Hardcoded in the client to mean city server...
            WriteVoltronString("MyAvatar"); //String ID
            WriteUInt32(00); //Temporary body size.
            WriteUInt32((uint)MessageClsIDs.cTSOTopicUpdateMessage);
            WriteUInt16(0x0f93); //Update counter, no idea.
            WriteUInt16(0x6027); //Update counter, no idea.
            WriteUInt32((uint)PreAlphaConstants.GZMSGID_cITSOApprovedTopicUpdate); //MessageID
            WriteUInt32(0x00000000); //m_statusCode

            WriteUInt32(0x00000003); //VectorSize
            //DataServiceWrapperPDU.Body.Vector[0]
            WriteUInt32(0x05600332); //Avatar
            //DataServiceWrapperPDU.Body.Vector[1]
            WriteUInt32(0x00000539); //Unknown
            //DataServiceWrapperPDU.Body.Vector[2]
            WriteUInt32(0x4FFE270C); //Avatar_Name
            //DataServiceWrapperPDU.Body.cTSOValueClsid
            WriteUInt32(0x896D1688); //0x896D1688(cTSOValue <class cRZAutoRefCount<class cIGZString> >)
            WriteString(Name);
            WriteByte(0x00); //DataServiceWrapperPDU.Body.m_reasonText

            /* MyAvatar.Avatar_Description packet */
            WriteBytes(new byte[] { 0x27,0x34,
                0x00,0x00,0x00,0x50,
                0xFE,0xED,0xF0,0x0D,
                0xA4,0x6E,0x47,0xDC,
                0x00,0x00,0x00,0x3E,
                0x09,0x73,0x60,0x27,
                0x0F,0xE8,0x80,0x3B,
                0xA9,0x73,0x60,0xC5,
                0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x03,
                0x05,0x60,0x03,0x32,
                0x00,0x00,0x05,0x39,
                0x54,0xB9,0x9C,0x9A,
                0x89,0x6D,0x16,0x88 });

            WriteString(Description);
            WriteByte(0x00); //DataServiceWrapperPDU.Body.m_reasonText

            /* MyAvatar.Avatar_Appearance packet */
            WriteBytes(new byte[] { 0x27,0x34,
                0x00,0x00,0x00,0x6C,
                0xFE,0xED,0xF0,0x0D,
                0xA4,0x6E,0x47,0xDC,
                0x00,0x00,0x00,0x5A,
                0x09,0x73,0x60,0x27,
                0x0F,0xE8,0x80,0x3B,
                0xA9,0x73,0x60,0xC5,
                0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x03,
                0x05,0x60,0x03,0x32,
                0x00,0x00,0x05,0x39,
                0xF1,0xB9,0x2C,0x99,
                0xA9,0x6E,0x7E,0x5B,
                0x89,0x73,0x9A,0x79,
                0x3B,0x04,0x30,0xBF,
                0x00,0x00,0x00,0x03,
                0x1D,0x53,0x02,0x75,
                0x69,0xD3,0xE3,0xDB,
                0x00,0x00,0x01,0x60,0x00,0x00,0x00,0x0D, /* body outfit */
                0x25,0x11,0xB2,0x9C,
                0xC9,0x76,0x08,0x7C,
                0x00,
                0x68,0x74,0x75,0xBF,
                0x69,0xD3,0xE3,0xDB,
                0x00,0x00,0x04,0x0F,0x00,0x00,0x00,0x0D, /* head outfit */ 0x00 });
        }
    }
}
