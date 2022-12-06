using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CountryBot
{
    static class Database
    {
        static SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-9690BV5;Initial Catalog=CountryBotDB;Integrated security=True;MultipleActiveResultSets=true");

        private const string hostDataPath = @"..\..\..\countries";
        static Database()
        {
            sqlConnection.Open();

            //InitCountriesToDB();
        }

        // Set    
        public static void InitUser(long id)
        {
            new SqlCommand($"insert into UserData(userID) values({id})", sqlConnection).ExecuteNonQuery();
        }
        public static void UpdateUserState(long id, BotState state)
        {
            new SqlCommand($"UPDATE UserData SET state = '{state}' WHERE userID = {id};", sqlConnection).ExecuteNonQuery();
        }
        public static void UpdateUserWorldSideKey(long id, WorldSideKey key)
        {
            new SqlCommand($"UPDATE UserData SET sideKey = '{key}' WHERE userID = {id};", sqlConnection).ExecuteNonQuery();
        }
        public static void InitUserRemainingDictionary(long id, WorldSideKey key)
        {
            SqlDataReader reader = new SqlCommand($"exec sp_spaceused '{key}'", sqlConnection).ExecuteReader();

            int sideCount = 0;

            while (reader.Read())
            {
                sideCount = Int32.Parse(reader["rows"].ToString());
            }

            reader.Close();

            for (int i = 1; i < sideCount+1; i++)
            {
                new SqlCommand($"insert into RemainingIndex(index_id, userID, remaining_index) values({i}, {id}, {i})", sqlConnection).ExecuteNonQuery();
            }
        }
        public static void UpdateGuessIndex(long id, int index)
        {
            new SqlCommand($"UPDATE UserData SET guessIndex = '{index}' WHERE userID = {id};", sqlConnection).ExecuteNonQuery();
        }
        public static void shiftRemainingIndex(long id, int index)
        {
            new SqlCommand($"UPDATE RemainingIndex SET index_id = index_id-1 where (userID = {id} AND index_id > {index})", sqlConnection).ExecuteNonQuery();
        }
        public static void deleteRemainingIndex(long id, int index)
        {
            new SqlCommand($"DELETE from RemainingIndex where (userID = {id} AND index_id = {index})", sqlConnection).ExecuteNonQuery();
        }
        public static void ClearRemainingIndexes(long id)
        {
            new SqlCommand($"DELETE from RemainingIndex where (userID = {id})", sqlConnection).ExecuteNonQuery();
        }
        public static void ClearTempValues(long id)
        {
            new SqlCommand($"UPDATE UserData SET sideKey = NULL where (userID = {id})", sqlConnection).ExecuteNonQuery();

            new SqlCommand($"UPDATE UserData SET guessIndex = NULL where (userID = {id})", sqlConnection).ExecuteNonQuery();
        }

        // Get
        public static BotState getBotState(long id)
        {
            SqlDataReader reader = new SqlCommand($"select state from UserData where userID = {id}", sqlConnection).ExecuteReader();

            string state = null;

            while (reader.Read())
            {
                state = reader["state"].ToString();
            }

            Enum.TryParse(state, out BotState enumstate);

            reader.Close();

            return enumstate;
        }
        public static WorldSideKey getWorldSideKey(long id)
        {
            SqlDataReader reader = new SqlCommand($"select sideKey from UserData where userID = {id}", sqlConnection).ExecuteReader();

            string key = null;

            while (reader.Read())
            {
                key = reader["sideKey"].ToString();
            }

            Enum.TryParse(key, out WorldSideKey enumstate);

            reader.Close();

            return enumstate;
        }   
        public static int getGuessIndex(long id)
        {
            SqlDataReader reader = new SqlCommand($"select guessIndex from UserData where userID = {id}", sqlConnection).ExecuteReader();

            string index = null;

            while (reader.Read())
            {
                index = reader["guessIndex"].ToString();
            }

            reader.Close();

            return Int32.Parse(index);
        }
        public static bool UserContainsKey(long id)
        {
            SqlDataReader reader = new SqlCommand($"select COUNT(*) as userID from UserData where (userID = {id})", sqlConnection).ExecuteReader();

            long count = 0;

            while (reader.Read())
            {
                count = Int64.Parse(reader["userID"].ToString());
            }

            reader.Close();

            return count != 0;     
        }
        public static string getCountryByIndex(WorldSideKey key, int index)
        {
            SqlDataReader reader = new SqlCommand($"select country from {key} where country_id = {index}", sqlConnection).ExecuteReader();

            string result = null;

            while (reader.Read())
            {
                result = reader["country"].ToString();
            }

            reader.Close();

            return result;
        }
        public static string getCapitalByIndex(WorldSideKey key, int index)
        {
            SqlDataReader reader = new SqlCommand($"select capital from {key} where country_id = {index}", sqlConnection).ExecuteReader();

            string result = null;

            while (reader.Read())
            {
                result = reader["capital"].ToString();
            }

            reader.Close();

            return result;
        }
        public static int getCountryNumByIndex(long id, int index)
        {
            SqlDataReader reader = new SqlCommand($"select remaining_index from RemainingIndex where (index_id = {index} AND userID = {id})", sqlConnection).ExecuteReader();

            int result = 0;

            while (reader.Read())
            {
                result = Int32.Parse(reader["remaining_index"].ToString());
            }

            reader.Close();

            return result;
        }
        public static int getRemainingIndexSize(long id)
        {
            SqlDataReader reader = new SqlCommand($"select COUNT(*) as country_id from RemainingIndex where (userID = {id})", sqlConnection).ExecuteReader();

            int result = 0;

            while (reader.Read())
            {
                result = Int32.Parse(reader["country_id"].ToString());
            }

            reader.Close();

            return result;
        }


        // Statistic

        public static void updateCountryInfo(long id, string capital, bool correct)
        {
            SqlDataReader reader = new SqlCommand($"select country_id from World where (capital = '{capital}')", sqlConnection).ExecuteReader();

            int country_id = 0;

            while (reader.Read())
            {
                country_id = Int32.Parse(reader["country_id"].ToString());
            }

            reader.Close();

            int count = (int)(new SqlCommand($"select COUNT(*) from Statistic where (userID = {id} AND country_id = {country_id})", sqlConnection).ExecuteScalar());


            if (count == 0)
            {           
                new SqlCommand($"insert into Statistic (userID, country_id, total, correct) values({id}, {country_id}, 1, {Convert.ToInt32(correct)})", sqlConnection).ExecuteNonQuery();
            } else
            {
                new SqlCommand($"UPDATE Statistic SET total = total+1 where (userID = {id} AND country_id = {country_id})", sqlConnection).ExecuteNonQuery();

                new SqlCommand($"UPDATE Statistic SET correct = correct + {Convert.ToInt32(correct)} where (userID = {id} AND country_id = {country_id})", sqlConnection).ExecuteNonQuery();
            }
        }


        // Before bot using
        public static void InitCountriesToDB()
        {
            new SqlCommand($"truncate table World", sqlConnection).ExecuteNonQuery();

            foreach (WorldSideKey key in Enum.GetValues(typeof(WorldSideKey)))
            {
                if (key != WorldSideKey.World)
                {
                    new SqlCommand($"truncate table {key}", sqlConnection).ExecuteNonQuery();

                    string path = @$"{hostDataPath}\{key}.txt";

                    var dict = CsvParser.GetCountriesFromFile(path);

                    foreach(var item in dict)
                    {
                        string str = $"insert into {key} (country, capital) values('{item.Key}', '{item.Value.capital}')";

                        new SqlCommand(str, sqlConnection).ExecuteNonQuery();
                    }

                    new SqlCommand($"insert into World select country, capital from {key}", sqlConnection).ExecuteNonQuery();
                }
            }
        }

    }
}
