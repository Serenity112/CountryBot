using System;
using System.Collections.Generic;

namespace CountryBot
{
    static class BotUsers
    {
        public class UserData
        {
            public BotState state { get; set; }

            public WorldSideKey sideKey { get; set; }

            public WorldSideKey tempSideKey { get; set; }

            public int guessIndex { get; set; }

            public Dictionary<WorldSideKey, List<int>> remainingIndexes { get; set; }

            public UserData()
            {
                state = BotState.WorldSideChoise;
                sideKey = WorldSideKey.World;
                tempSideKey = WorldSideKey.World;
                guessIndex = 0;
                remainingIndexes = new Dictionary<WorldSideKey, List<int>>();
            }
        }
        static BotUsers()
        {
            Users = new Dictionary<long, UserData>();
        }
          
        // Users futture DB
        public static Dictionary<long, UserData> Users { get; set; }

        // Access methods
        public static void InitUser(long id)
        {
            if (!Users.ContainsKey(id))
            {
                Users.Add(id, new UserData());
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id already exists");
            }
            
        }
        public static void UpdateUserState(long id, BotState state)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].state = state;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }
        }
        public static void UpdateUserTempSideKey(long id, WorldSideKey key)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].tempSideKey = key;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }
        }
        public static void UpdateUserWorldSideKey(long id, WorldSideKey key)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].sideKey = key;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }
        }
        public static void UpdateUserIndexDictionary(long id, Dictionary<WorldSideKey, List<int>> dictioanry)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].remainingIndexes = dictioanry;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }
        }
        public static void ClearUserRemainingDictionary(long id)
        {
            if (Users.ContainsKey(id))
            {
                var newDictionary = new Dictionary<WorldSideKey, List<int>>();

                UpdateUserIndexDictionary(id, newDictionary);

                Users[id].guessIndex = 0;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }
           
        }
        public static void InitUserRemainingDictionary(long id, WorldSideKey key, Dictionary<WorldSideKey, Dictionary<string, CountryData>> World)
        {
            if (Users.ContainsKey(id))
            {
                var newDictionary = new Dictionary<WorldSideKey, List<int>>();

                if (key == WorldSideKey.World)
                {
                    foreach (WorldSideKey sidekey in Enum.GetValues(typeof(WorldSideKey)))
                    {
                        if (sidekey != WorldSideKey.World)
                        {
                            List<int> uniqueIndexes = new List<int>();

                            for (int i = 0; i < World[sidekey].Count; i++)
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
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }  
        }
        public static void UpdateGuessIndex(long id, int index)
        {
            if (Users.ContainsKey(id))
            {
                Users[id].guessIndex = index;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }
        }

        public static BotState getBotState(long id)
        {
            if (Users.ContainsKey(id))
            {
                return Users[id].state;
            } else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }
  
        }
        public static WorldSideKey getWorldSideKey(long id)
        {
            if (Users.ContainsKey(id))
            {
                return Users[id].sideKey;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }    
        }
        public static WorldSideKey getTempWorldSideKey(long id)
        {
            if (Users.ContainsKey(id))
            {
                return Users[id].tempSideKey;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }    
        }
        public static int getguessIndex(long id)
        {
            if (Users.ContainsKey(id))
            {
                return Users[id].guessIndex;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }   
        }
        public static Dictionary<WorldSideKey, List<int>> getremainingIndexes(long id)
        {
            if (Users.ContainsKey(id))
            {
                return Users[id].remainingIndexes;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Id not found");
            }  
        }
        public static bool UserContainsKey(long id)
        {
            return Users.ContainsKey(id);
        }
    }
}