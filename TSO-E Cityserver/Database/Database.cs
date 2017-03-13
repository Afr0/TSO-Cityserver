using System;
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
                Cmd.CommandText = "CREATE TABLE IF NOT EXISTS Accounts (Id int PRIMARY KEY, " +
                    "Username nvarchar(256), Password nvarchar(256), AuthTicket int, " + 
                    "PreferedLanguageID int, AvatarID1 int, AvatarID2 int, AvatarID3 int)";
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
                    "SkillLockMechanical int, SkillLockCooking int, SkillLockCharisma int, " + 
                    "SkillLockLogic int, SkillLockBody int, SkillLockCreativity int, MechanicalSkill int, " +
                    "CookingSkill int, CharismaSkill int, LogicSkill int, BodySkill int, CreativitySkill int, " + 
                    "Gender tinyint, SkinColor tinyint, Cash int, HeadOutfitID nvarchar(256), " + 
                    "BodyNormalID nvarchar(256), BodySwimWearID nvarchar(256), BodySleepWearID nvarchar(256), " + 
                    "BodyNudeID nvarchar(256), IsGhost int, SimBonus bigint, PropertyBonus bigint, " + 
                    "VisitorBonus bigint, OnlineJobID mediumint)";
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
                Cmd.CommandText = "CREATE TABLE IF NOT EXISTS CityServers (ID int PRIMARY KEY, " +
                    "Name nvarchar(256), Rank int, " +
                    "Status nvarchar(256), Map int, OnlineAvatars int, MOTDFrom nvarchar(256), " +
                    "MOTDSubject nvharchar(256), MOTDMessage nvarchar(256))";
                Cmd.CommandType = CommandType.Text;
                Cmd.ExecuteNonQuery(CommandBehavior.Default);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to create Cityservers table:");
                Console.WriteLine(e.ToString());

                return false;
            }
        }

        /// <summary>
        /// Inserts an account into the DB if it doesn't exist.
        /// </summary>
        /// <param name="ID">ID of the account.</param>
        /// <param name="Username">Name of the account.</param>
        /// <param name="Password">Password of the account.</param>
        /// <param name="SessionID">SessionID of the account (initially 0).</param>
        /// <param name="PreferedLanguageID">PreferedLanguageID (initially 0).</param>
        /// <param name="AvatarID1">AvatarID1 (initially 0).</param>
        /// <param name="AvatarID2">AvatarID2 (initially 0).</param>
        /// <param name="AvatarID3">AvatarID3 (initially 0).</param>
        public static void CreateAccount(int ID, string Username, string Password, string SessionID, 
            int PreferedLanguageID, string AvatarID1, string AvatarID2, string AvatarID3)
        {
            SQLiteCommand Cmd = new SQLiteCommand(m_DBConnection);
            //TODO: Only INSERT if it doesn't exist...
            Cmd.CommandText = "INSERT OR REPLACE INTO Accounts ('ID', 'Username', " + 
                "'Password', 'AuthTicket', 'PreferedLanguageID', 'AvatarID1', " + 
                "'AvatarID2', 'AvatarID3') VALUES (" + ID + ", '" + Username + "', " +
                "'" + Password + "', '" + SessionID + "', " + PreferedLanguageID + ", '" + 
                AvatarID1 + "', " + "'" + AvatarID2 + "', " + "'" + AvatarID3 + "')";
            Cmd.CommandType = CommandType.Text;
            Cmd.ExecuteNonQuery();
        }

        public static void CreateCityserver(int ID, string Name, int Rank, string Status, int Map,
            int OnlineAvatars, string MOTDFrom, string MOTDSubject, string MOTDMessage)
        {
            SQLiteCommand Cmd = new SQLiteCommand(m_DBConnection);
            //TODO: Only INSERT if it doesn't exist...
            Cmd.CommandText = "INSERT OR REPLACE INTO CityServers ('ID', 'Name', 'Rank', 'Status', 'Map'," +
                "'OnlineAvatars', 'MOTDFrom', 'MOTDSubject', 'MOTDMessage') VALUES (" + ID + ", '" + Name + "', " +
                Rank + ", '" + Status + "', " + Map + ", " + OnlineAvatars + ", '" +
                MOTDFrom + "', '" + MOTDSubject + "', '" + MOTDMessage + "')";
            Cmd.CommandType = CommandType.Text;
            Cmd.ExecuteNonQuery();
        }

        public static void CreateAvatar(uint ID, string Name, string Description, 
            int Simoleans, int SimoleanDelta, int Popularity, int PopularityDelta, string ShardName, 
            int SkillLockMechanical, int SkillLockCooking, int SkillLockCharisma, int SkillLockLogic, 
            int SkillLockBody, int SkillLockCreativity, int MechanicalSkill, int CookingSkill, 
            int CharismaSkill, int LogicSkill, int BodySkill, int CreativitySkill, byte Gender, 
            byte SkinColor, uint Cash, string HeadOutfitID, string BodyNormalID, string BodySwimWearID, 
            string BodySleepWearID, string BodyNudeID, ushort IsGhost, uint SimBonus, uint PropertyBonus, 
            uint VisitorBonus, ushort OnlineJobID)
        {
            SQLiteCommand Cmd = new SQLiteCommand(m_DBConnection);
            //TODO: Only INSERT if it doesn't exist...
            Cmd.CommandText = "INSERT OR REPLACE INTO Avatars ('AvatarID', 'Name', " +
                "'Description', 'Simoleans', 'SimoleanDelta', 'Popularity', 'PopularityDelta', " + 
                "'ShardName', 'SkillLockMechanical', 'SkillLockCooking', 'SkillLockCharisma', " + 
                "'SkillLockLogic', 'SkillLockBody', 'SkillLockCreativity', " + 
                "'MechanicalSkill', 'CookingSkill', 'CharismaSkill', 'LogicSkill', " + 
                "'BodySkill', 'CreativitySkill', 'Gender', 'SkinColor', 'Cash', " + 
                "'HeadOutfitID', 'BodyNormalID', 'BodySwimWearID', 'BodySleepWearID', " + 
                "'BodyNudeID', 'IsGhost', 'SimBonus', 'PropertyBonus', 'VisitorBonus', " + 
                "'OnlineJobID') VALUES (" + ID + ", '" + Name + "', '" + Description + "', " +
                Simoleans.ToString() + ", " + SimoleanDelta.ToString() + ", " + 
                Popularity + ", " + PopularityDelta + ", '" + ShardName + "', " + SkillLockMechanical + 
                ", " +  SkillLockCooking + ", " + SkillLockCharisma + ", " + SkillLockLogic +
                ", " + SkillLockBody + ", " + SkillLockCreativity +", " + MechanicalSkill + 
                ", " + CookingSkill + ", " + CharismaSkill +", " + LogicSkill + ", " + BodySkill + 
                ", " + CreativitySkill +", " + Gender + ", " + SkinColor + ", " + Cash + 
                ", '" + HeadOutfitID +"', '" + BodyNormalID + "', '" + BodySwimWearID + 
                "', '" + BodySleepWearID +"', '" + BodyNudeID + "', " + IsGhost.ToString() + 
                ", " + SimBonus + ", " + PropertyBonus + ", " + VisitorBonus + ", " + OnlineJobID + ")";
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
        /// <returns></returns>
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

        #endregion

        private static Account AccountBuilder(DbDataReader reader)
        {
            return new Account
            {
                Username = reader["Username"].ToString(),
                Password = reader["Password"].ToString(),
                AuthTicket = (uint)reader["AuthTicket"],
                PreferedLanguageID = int.Parse(reader["PreferedLanguageID"].ToString()),
                AvatarID1 = reader["AvatarID1"].ToString(),
                AvatarID2 = reader["AvatarID2"].ToString(),
                AvatarID3 = reader["AvatarID3"].ToString(),
            };
        }

        private static Avatar AvatarBuilder(DbDataReader reader)
        {
            return new Avatar
            {
                AvatarID = uint.Parse(reader["AvatarID"].ToString()),
                Name = reader["Name"].ToString(),
                Description = reader["Description"].ToString(),
                Simoleans = int.Parse(reader["Simoleans"].ToString()),
                SimoleanDelta = int.Parse(reader["SimoleanDelta"].ToString()),
                Popularity = int.Parse(reader["Popularity"].ToString()),
                PopularityDelta = int.Parse(reader["PopularityDelta"].ToString()),
                ShardName = reader["ShardName"].ToString(),

                SkillLockMechanical = short.Parse(reader["SkillLockMechanical"].ToString()),
                SkillLockCooking = short.Parse(reader["SkillLockCooking"].ToString()),
                SkillLockCharisma = short.Parse(reader["SkillLockCharisma"].ToString()),
                SkillLockLogic = short.Parse(reader["SkillLockLogic"].ToString()),
                SkillLockBody = short.Parse(reader["SkillLockBody"].ToString()),
                SkillLockCreativity = short.Parse(reader["SkillLockCreativity"].ToString()),

                MechanicalSkill = short.Parse(reader["MechanicalSkill"].ToString()),
                CookingSkill = short.Parse(reader["CookingSkill"].ToString()),
                CharismaSkill = short.Parse(reader["CharismaSkill"].ToString()),
                LogicSkill = short.Parse(reader["LogicSkill"].ToString()),
                BodySkill = short.Parse(reader["BodySkill"].ToString()),
                CreativitySkill = short.Parse(reader["CreativitySkill"].ToString()),

                Gender = byte.Parse(reader["Gender"].ToString()),
                SkinColor = byte.Parse(reader["SkinColor"].ToString()),
                Cash = uint.Parse(reader["Cash"].ToString()),

                HeadOutfitID = reader["HeadOutfitID"].ToString(),
                BodyNormalID = reader["BodyNormalID"].ToString(),
                BodySwimWearID = reader["BodySwimWearID"].ToString(),
                BodySleepWearID = reader["BodySleepwearID"].ToString(),
                BodyNudeID = reader["BodyNudeID"].ToString(),

                IsGhost = ushort.Parse(reader["IsGhost"].ToString()),

                SimBonus = uint.Parse(reader["SimBonus"].ToString()),
                PropertyBonus = uint.Parse(reader["PropertyBonus"].ToString()),
                VisitorBonus = uint.Parse(reader["VisitorBonus"].ToString()),

                OnlineJobID = ushort.Parse(reader["OnlineJobID"].ToString())
            };
        }
    }
}
