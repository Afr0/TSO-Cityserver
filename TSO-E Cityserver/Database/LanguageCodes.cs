using System;


namespace TSO_E_Cityserver.Database
{
    /// <summary>
    /// Defines a player's prefered language, as stored in the DB.
    /// </summary>
    public enum LanguageCodes : byte
    {
        unused = 0,
        EngUS = 1,
        EngInternational = 2,
        French = 3,
        German = 4,
        Italian = 5,
        Spanish = 6,
        Dutch = 7,
        Danish = 8,
        Swedish = 9,
        Norwegian = 10,
        Finnish = 11,
        Hebrew = 12,
        Russian = 13,
        Portugese = 14,
        Japanese = 15,
        Polish = 16,
        ChineseSimple = 17,
        ChineseTrad = 18,
        Thai = 19,
        Korean = 20
    }
}
