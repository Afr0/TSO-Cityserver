using System;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Data.Common;
using System.Threading.Tasks;

namespace TSO_E_Cityserver.Database
{
    public class DatabaseFacade
    {
        private static SQLiteConnection m_DBConnection;

        /// <summary>
        /// Initializes the connection to the database.
        /// </summary>
        /// <returns>True if successful. If something failed, 
        /// the exception is logged to the console and this method returns false.</returns>
        public static bool Initialize()
        {
            Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection Settings = Config.AppSettings.Settings;

            try
            {
                m_DBConnection = new SQLiteConnection("Data Source=" + Settings["AccountsDatasource"].Value);
                m_DBConnection.Open();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to create database:");
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Creates the tables in the DB if they do not already exist.
        /// </summary>
        /// <returns>True if successful. If something failed, 
        /// the exception is logged to the console and this method returns false.</returns>
        public static bool CreateTables()
        {
            SQLiteCommand Cmd;

            try
            {
                Cmd = new SQLiteCommand(m_DBConnection);
                Cmd.CommandText = "CREATE TABLE IF NOT EXISTS CityServers (Id int PRIMARY KEY, " +
                    "Name nvarchar(256), Port int, Rank int, GMTRange int, " +
                    "Status nvarchar(256), Map int, OnlineAvatars int, MOTDFrom nvarchar(256), " + 
                    "MOTDSubject nvarchar(256)," + "MOTDMessage nvarchar(256))";
                Cmd.CommandType = CommandType.Text;
                Cmd.ExecuteNonQuery(CommandBehavior.Default);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to create CityServers table:");
                Console.WriteLine(e.ToString());

                return false;
            }

            try
            {
                Cmd = new SQLiteCommand(m_DBConnection);
                Cmd.CommandText = "CREATE TABLE IF NOT EXISTS Accounts (ID int PRIMARY KEY, " +
                    "Username nvarchar(256), Password nvarchar(256), " + 
                    "AuthTicket nvarchar256, " + "PreferedLanguageID int, AvatarID1 int, " + 
                    "AvatarID2 int, AvatarID3 int)";
                Cmd.CommandType = CommandType.Text;
                Cmd.ExecuteNonQuery(CommandBehavior.Default);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to create Accounts table:");
                Console.WriteLine(e.ToString());

                return false;
            }

            try
            {
                Cmd = new SQLiteCommand(m_DBConnection);
                Cmd.CommandText = "CREATE TABLE IF NOT EXISTS Avatars (AvatarID int PRIMARY KEY, " +
                    "Name nvarchar(256), Description nvarchar(256), Simoleans int, SimoleanDelta int, " +
                    "Popularity int, " + "PopularityDelta int, ShardName nvarchar(256), " + 
                    "PropertyID int, SkillLockMechanical int, SkillLockCooking int, SkillLockCharisma int, " + 
                    "SkillLockLogic int, SkillLockBody int, SkillLockCreativity int, MechanicalSkill int, " +
                    "CookingSkill int, CharismaSkill int, LogicSkill int, BodySkill int, CreativitySkill int, " + 
                    "Gender tinyint, SkinColor tinyint, Cash int, HeadOutfitID nvarchar(256), " + 
                    "BodyNormalID nvarchar(256), BodySwimWearID nvarchar(256), BodySleepWearID nvarchar(256), " + 
                    "BodyNudeID nvarchar(256), IsGhost int, SimBonus bigint, PropertyBonus bigint, " + 
                    "VisitorBonus bigint, OnlineJobID mediumint, cTSONeighborBlob blob)";
                Cmd.CommandType = CommandType.Text;
                Cmd.ExecuteNonQuery(CommandBehavior.Default);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to create Avatars table:");
                Console.WriteLine(e.ToString());

                return false;
            }

            try
            {
                Cmd = new SQLiteCommand(m_DBConnection);
                Cmd.CommandText = "CREATE TABLE IF NOT EXISTS Properties (PropertyID int PRIMARY KEY, " +
                    "NumOccupants byte, IsOnline byte, LeaderID byte, Name nvarchar(256), " +
                    "Description nvarchar(256), LotPrice int, Category byte)";
                Cmd.CommandType = CommandType.Text;
                Cmd.ExecuteNonQuery(CommandBehavior.Default);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to create Properties table:");
                Console.WriteLine(e.ToString());

                return false;
            }
        }

        public static void CreateServer(int ID, string Name, int Port, int Rank, int GMTRange, string Status,
            int Map, int OnlineAvatars = 0, string MOTDFrom = "", string MOTDSubject = "", 
            string MOTDMessage = "")
        {
            SQLiteCommand Cmd = new SQLiteCommand(m_DBConnection);
            //TODO: Only INSERT if it doesn't exist...
            Cmd.CommandText = "INSERT OR REPLACE INTO CityServers ('ID', 'Name', " +
                "'Port', 'Rank', 'GMTRange', 'Status', 'Map', 'OnlineAvatars', " +
                "'MOTDFrom', 'MOTDSubject', 'MOTDMessage') VALUES (" + ID + ", '" + 
                Name + "', " + Port + ", " + Rank + ", " + GMTRange + ", '" + Status + "', " + Map + 
                ", " + OnlineAvatars + ", '" + MOTDFrom + "', '" + MOTDSubject + 
                "', '"  + MOTDMessage + "')";
            Cmd.CommandType = CommandType.Text;
            Cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserts an account into the DB if it doesn't exist.
        /// </summary>
        /// <param name="ID">ID of the account.</param>
        /// <param name="Username">Name of the account.</param>
        /// <param name="Password">Password of the account.</param>
        /// <param name="PreferedLanguageID">PreferedLanguageID (initially 0).</param>
        /// <param name="AvatarID1">AvatarID1 (initially 0).</param>
        /// <param name="AvatarID2">AvatarID2 (initially 0).</param>
        /// <param name="AvatarID3">AvatarID3 (initially 0).</param>
        public static void CreateAccount(int ID, string Username, string Password, 
            int PreferedLanguageID, int AvatarID1, int AvatarID2, int AvatarID3)
        {
            SQLiteCommand Cmd = new SQLiteCommand(m_DBConnection);
            //TODO: Only INSERT if it doesn't exist...
            Cmd.CommandText = "INSERT OR REPLACE INTO Accounts ('ID', 'Username', " + 
                "'Password', 'AuthTicket', 'PreferedLanguageID', 'AvatarID1', " + 
                "'AvatarID2', 'AvatarID3') VALUES (" + ID + ", '" + Username + "', " +
                "'" + Password + "', '" + "', " + PreferedLanguageID + ", '" + 
                AvatarID1 + "', " + "'" + AvatarID2 + "', " + "'" + AvatarID3 + "')";
            Cmd.CommandType = CommandType.Text;
            Cmd.ExecuteNonQuery();
        }

        public static void CreateAvatar(uint ID, string Name, string Description, 
            int Simoleans, int SimoleanDelta, int Popularity, int PopularityDelta, string ShardName, 
            uint PropertyID, int SkillLockMechanical, int SkillLockCooking, int SkillLockCharisma, 
            int SkillLockLogic, int SkillLockBody, int SkillLockCreativity, int MechanicalSkill, 
            int CookingSkill, int CharismaSkill, int LogicSkill, int BodySkill, int CreativitySkill, 
            byte Gender, byte SkinColor, uint Cash, string HeadOutfitID, string BodyNormalID, 
            string BodySwimWearID, string BodySleepWearID, string BodyNudeID, ushort IsGhost, 
            uint SimBonus, uint PropertyBonus, uint VisitorBonus, ushort OnlineJobID, byte[] cTSONeighborBlob)
        {
            SQLiteCommand Cmd = new SQLiteCommand(m_DBConnection);
            //TODO: Only INSERT if it doesn't exist...
            Cmd.CommandText = "INSERT OR REPLACE INTO Avatars ('AvatarID', 'Name', " +
                "'Description', 'Simoleans', 'SimoleanDelta', 'Popularity', 'PopularityDelta', " + 
                "'ShardName', 'PropertyID', 'SkillLockMechanical', 'SkillLockCooking', 'SkillLockCharisma', " + 
                "'SkillLockLogic', 'SkillLockBody', 'SkillLockCreativity', " + 
                "'MechanicalSkill', 'CookingSkill', 'CharismaSkill', 'LogicSkill', " + 
                "'BodySkill', 'CreativitySkill', 'Gender', 'SkinColor', 'Cash', " + 
                "'HeadOutfitID', 'BodyNormalID', 'BodySwimWearID', 'BodySleepWearID', " + 
                "'BodyNudeID', 'IsGhost', 'SimBonus', 'PropertyBonus', 'VisitorBonus', " + 
                "'OnlineJobID', 'cTSONeighborBlob')" + 
                " VALUES (" + ID + ", '" + Name + "', '" + Description + "', " +
                Simoleans.ToString() + ", " + SimoleanDelta.ToString() + ", " + 
                Popularity + ", " + PopularityDelta + ", '" + ShardName + "', " + PropertyID + 
                ", " + SkillLockMechanical + ", " +  SkillLockCooking + ", " + SkillLockCharisma + 
                ", " + SkillLockLogic + ", " + SkillLockBody + ", " + SkillLockCreativity + 
                ", " + MechanicalSkill + ", " + CookingSkill + ", " + CharismaSkill + 
                ", " + LogicSkill + ", " + BodySkill + ", " + CreativitySkill + 
                ", " + Gender + ", " + SkinColor + ", " + Cash + ", '" + HeadOutfitID + 
                "', '" + BodyNormalID + "', '" + BodySwimWearID + "', '" + BodySleepWearID + 
                "', '" + BodyNudeID + "', " + IsGhost.ToString() + ", " + SimBonus + 
                ", " + PropertyBonus + ", " + VisitorBonus + ", " + OnlineJobID + ", " + "@blob" + ")";
            Cmd.Parameters.Add("@blob", DbType.Binary, cTSONeighborBlob.Length).Value = cTSONeighborBlob;
            Cmd.CommandType = CommandType.Text;
            Cmd.ExecuteNonQuery();
        }

        public static async Task<IEnumerable<Account>> GetAccountsAsync()
        {
            using (var Cmd = new SQLiteCommand())
            {
                Cmd.Connection = m_DBConnection;
                Cmd.CommandText = "SELECT * FROM Accounts";
                Cmd.CommandType = CommandType.Text;

                using (DbDataReader Reader = await Cmd.ExecuteReaderAsync())
                {
                    return Reader.Select(r => AccountBuilder(r)).ToList();
                }
            }
        }

        /// <summary>
        /// Asynchronously gets an avatar from the DB.
        /// </summary>
        /// <param name="AvatarID">The ID of the avatar to get.</param>
        /// <returns>An Avatar instance.</returns>
        public static async Task<IEnumerable<Avatar>> GetAvatarAsync(uint AvatarID)
        {
            using (var Cmd = new SQLiteCommand())
            {
                Cmd.Connection = m_DBConnection;
                Cmd.CommandText = "SELECT * FROM Avatars WHERE AvatarID=" + AvatarID.ToString();
                Cmd.CommandType = CommandType.Text;

                using (DbDataReader Reader = await Cmd.ExecuteReaderAsync())
                {
                    return Reader.Select(r => AvatarBuilder(r)).ToList();
                }
            }
        }

        /// <summary>
        /// Asynchronously gets a property from the DB.
        /// </summary>
        /// <param name="PropertyID">The ID of the property to get.</param>
        /// <returns>An Property instance.</returns>
        public static async Task<IEnumerable<SimProperty>> GetPropertyAsync(uint PropertyID)
        {
            using (var Cmd = new SQLiteCommand())
            {
                Cmd.Connection = m_DBConnection;
                Cmd.CommandText = "SELECT * FROM Properties WHERE PropertyID=" + PropertyID.ToString();
                Cmd.CommandType = CommandType.Text;

                using (DbDataReader Reader = await Cmd.ExecuteReaderAsync())
                {
                    return Reader.Select(r => PropertyBuilder(r)).ToList();
                }
            }
        }

        public static IEnumerable<Avatar> GetAvatar(uint AvatarID)
        {
            using (var Cmd = new SQLiteCommand())
            {
                Cmd.Connection = m_DBConnection;
                Cmd.CommandText = "SELECT * FROM Avatars WHERE AvatarID=" + AvatarID.ToString();
                Cmd.CommandType = CommandType.Text;

                using (DbDataReader Reader = Cmd.ExecuteReader())
                {
                    return Reader.Select(r => AvatarBuilder(r)).ToList();
                }
            }
        }

        #region Updates

        public static async void UpdatePreferedLanguageByIDAsync(string Accountname,
            LanguageCodes ID)
        {
            using (var Cmd = new SQLiteCommand())
            {
                Cmd.Connection = m_DBConnection;
                Cmd.CommandText = "UPDATE Accounts SET PreferedLanguageID = " + (int)ID +
                    " WHERE Username = '" + Accountname + "'";
                Cmd.CommandType = CommandType.Text;

                await Cmd.ExecuteReaderAsync();
            }
        }

        /// <summary>
        /// Updates the AvatarID for an avatar.
        /// </summary>
        /// <param name="OldAvatarID">The old AvatarID.</param>
        /// <param name="AvatarID">The AvatarID with which to replace the old one.</param>
        public static async void UpdateAvatarID(uint OldAvatarID, uint AvatarID)
        {
            using (var Cmd = new SQLiteCommand())
            {
                Cmd.Connection = m_DBConnection;
                Cmd.CommandText = "UPDATE Avatars SET AvatarID = " + (int)AvatarID +
                    " WHERE AvatarID = " + OldAvatarID;
                Cmd.CommandType = CommandType.Text;

                await Cmd.ExecuteReaderAsync();
            }
        }

        #endregion

        private static Account AccountBuilder(DbDataReader Reader)
        {
            return new Account
            {
                PlayerID = int.Parse(Reader["ID"].ToString()),
                Username = Reader["Username"].ToString(),
                Password = Reader["Password"].ToString(),
                SessionID = Reader["AuthTicket"].ToString(),
                PreferedLanguageID = int.Parse(Reader["PreferedLanguageID"].ToString()),
                AvatarID1 = uint.Parse(Reader["AvatarID1"].ToString()),
                AvatarID2 = uint.Parse(Reader["AvatarID2"].ToString()),
                AvatarID3 = uint.Parse(Reader["AvatarID3"].ToString()),
            };
        }

        private static Avatar AvatarBuilder(DbDataReader Reader)
        {
            return new Avatar
            {
                AvatarID = uint.Parse(Reader["AvatarID"].ToString()),
                Name = Reader["Name"].ToString(),
                Description = Reader["Description"].ToString(),
                Simoleans = int.Parse(Reader["Simoleans"].ToString()),
                SimoleanDelta = int.Parse(Reader["SimoleanDelta"].ToString()),
                Popularity = int.Parse(Reader["Popularity"].ToString()),
                PopularityDelta = int.Parse(Reader["PopularityDelta"].ToString()),
                ShardName = Reader["ShardName"].ToString(),
                PropertyID = uint.Parse(Reader["PropertyID"].ToString()),

                SkillLockMechanical = short.Parse(Reader["SkillLockMechanical"].ToString()),
                SkillLockCooking = short.Parse(Reader["SkillLockCooking"].ToString()),
                SkillLockCharisma = short.Parse(Reader["SkillLockCharisma"].ToString()),
                SkillLockLogic = short.Parse(Reader["SkillLockLogic"].ToString()),
                SkillLockBody = short.Parse(Reader["SkillLockBody"].ToString()),
                SkillLockCreativity = short.Parse(Reader["SkillLockCreativity"].ToString()),

                MechanicalSkill = short.Parse(Reader["MechanicalSkill"].ToString()),
                CookingSkill = short.Parse(Reader["CookingSkill"].ToString()),
                CharismaSkill = short.Parse(Reader["CharismaSkill"].ToString()),
                LogicSkill = short.Parse(Reader["LogicSkill"].ToString()),
                BodySkill = short.Parse(Reader["BodySkill"].ToString()),
                CreativitySkill = short.Parse(Reader["CreativitySkill"].ToString()),

                Gender = byte.Parse(Reader["Gender"].ToString()),
                SkinColor = byte.Parse(Reader["SkinColor"].ToString()),
                Cash = uint.Parse(Reader["Cash"].ToString()),

                HeadOutfitID = Reader["HeadOutfitID"].ToString(),
                BodyNormalID = Reader["BodyNormalID"].ToString(),
                BodySwimWearID = Reader["BodySwimWearID"].ToString(),
                BodySleepWearID = Reader["BodySleepwearID"].ToString(),
                BodyNudeID = Reader["BodyNudeID"].ToString(),

                IsGhost = ushort.Parse(Reader["IsGhost"].ToString()),

                SimBonus = uint.Parse(Reader["SimBonus"].ToString()),
                PropertyBonus = uint.Parse(Reader["PropertyBonus"].ToString()),
                VisitorBonus = uint.Parse(Reader["VisitorBonus"].ToString()),

                OnlineJobID = ushort.Parse(Reader["OnlineJobID"].ToString()),

                cTSONeighborBlob = (byte[])Reader["cTSONeighborBlob"]
            };
        }

        private static SimProperty PropertyBuilder(DbDataReader Reader)
        {
            return new SimProperty
            {
                ID = uint.Parse(Reader["ID"].ToString()),
                NumOccupants = byte.Parse(Reader["NumOccupants"].ToString()),
                IsOnline = byte.Parse(Reader["IsOnline"].ToString()),
                LeaderID = byte.Parse(Reader["LeaderID"].ToString()),
                Name = Reader["Name"].ToString(),
                Description = Reader["Description"].ToString(),
                LotPrice = uint.Parse(Reader["LotPrice"].ToString()),
                Category = byte.Parse(Reader["Category"].ToString()),
            };
        }
    }
}
