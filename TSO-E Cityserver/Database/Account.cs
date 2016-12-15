using System;

namespace TSO_E_Cityserver.Database
{
    /// <summary>
    /// Represents an account stored in the database.
    /// </summary>
    public class Account
    {
        public string Username;
        public string Password; //A hashed version of the password.
        public string SessionID;
        public int PreferedLanguageID;
        public string AvatarID1, AvatarID2, AvatarID3;
        public Avatar[] Avatars;
    }
}
