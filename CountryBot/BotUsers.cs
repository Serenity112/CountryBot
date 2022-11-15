using System;
using System.Collections.Generic;
using System.Text;
using static CountryBot.BotManager;

namespace CountryBot
{
    static class BotUsers
    {
        private class UserData
        {
            public BotState state;

            public WorldSideKey sideKey;

            public WorldSideKey tempSideKey;

            public int guessIndex = 0;

            public Dictionary<WorldSideKey, List<int>> remainingIndexes = new Dictionary<WorldSideKey, List<int>>();
        }

        private static Dictionary<long, UserData> Users = new Dictionary<long, UserData>();

        public static void InitUser(long id)
        {
            Users.Add(id, new UserData());
        }
        public static void UpdateUserState(long id, BotState state)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].state = state;
            }
        }
        public static void UpdateUserTempSideKey(long id, WorldSideKey key)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].tempSideKey = key;
            }
        }
        public static void UpdateUserWorldSideKey(long id, WorldSideKey key)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].sideKey = key;
            }
        }
        public static void UpdateUserIndexDictionary(long id, Dictionary<WorldSideKey, List<int>> dictioanry)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].remainingIndexes = dictioanry;
            }
        }
        public static void ClearUserRemainingDictionary(long id)
        {
            var newDictionary = new Dictionary<WorldSideKey, List<int>>();

            UpdateUserIndexDictionary(id, newDictionary);

            Users[id].guessIndex = 0;
        }
        public static void InitUserRemainingDictionary(long id, WorldSideKey key)
        {
            var newDictionary = new Dictionary<WorldSideKey, List<int>>();

            if (key == WorldSideKey.World)
            {
                foreach (WorldSideKey sidekey in Enum.GetValues(typeof(WorldSideKey)))
                {
                    if(sidekey != WorldSideKey.World)
                    {
                        List<int> uniqueIndexes = new List<int>();

                        for (int i = 0; i < BotManager.World[sidekey].Count; i++)
                        {
                            uniqueIndexes.Add(i);
                        }

                        newDictionary.Add(sidekey, uniqueIndexes);
                    }
                
                }
            }
            else
            {
                List<int> uniqueIndexes = new List<int>();

                for (int i = 0; i < World[key].Count; i++)
                {
                    uniqueIndexes.Add(i);
                }

                newDictionary.Add(key, uniqueIndexes);
            }

            Users[id].guessIndex = 0;

            UpdateUserIndexDictionary(id, newDictionary);
        }
        public static void UpdateGuessIndex(long id, int index)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].guessIndex = index;
            }
        }

        public static BotState getBotState(long id)
        {
            return Users[id].state;
        }
        public static WorldSideKey getWorldSideKey(long id)
        {
            return Users[id].sideKey;
        }
        public static WorldSideKey getTempWorldSideKey(long id)
        {
            return Users[id].tempSideKey;
        }
        public static int getguessIndex(long id)
        {
            return Users[id].guessIndex;
        }
        public static Dictionary<WorldSideKey, List<int>> getremainingIndexes(long id)
        {
            return Users[id].remainingIndexes;
        }
        public static bool UserContainsKey(long id)
        {
            return Users.ContainsKey(id);
        }
    }
}