using System;

namespace TSO_E_Cityserver.Database
{
    /// <summary>
    /// Represents an account stored in the database.
    /// </summary>
    public class Account
    {
        public int PlayerID;
        public string Username;
        public string Password; //A hashed version of the password.
        public string SessionID;
        public int PreferedLanguageID;
        public uint AvatarID1 = 0, AvatarID2 = 0, AvatarID3 = 0;
        public Avatar[] Avatars;
    }
}
