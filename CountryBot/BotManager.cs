using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using System.Linq;

namespace CountryBot
{
    class BotManager
    {
        private const string hostDataPath = @"D:\countries";
        // Client
        private static ITelegramBotClient client { get; set; }

        // WorldSide
        private struct WorldSideData
        {
            public WorldSideData(WorldSideKey key, string name)
            {
                this.key = key;
                this.rus_name = name;
            }

            public WorldSideKey key;
            public string rus_name;
        }
        public enum WorldSideKey
        {
            World,
            Europe,
            Asia,
            Africa,
            NorthAmerica,
            SouthAmerica,
            Oceania,
        }

        //State
        public enum BotState
        {
            Greetings,
            WorldSideChoise,
            CountryQuestion,
            CountryGuessing,
        }

        // Lists

        private static List<WorldSideData> WorldSideNamesForKeys;

        public static Dictionary<WorldSideKey, Dictionary<string, CountryData>> World;
        public BotManager(string telegramToken)
        {
            client = new TelegramBotClient(telegramToken);

            WorldSideNamesForKeys = new List<WorldSideData>();
            WorldSideNamesForKeys.Add(new WorldSideData(WorldSideKey.World, "Мир"));
            WorldSideNamesForKeys.Add(new WorldSideData(WorldSideKey.Europe, "Европа"));
            WorldSideNamesForKeys.Add(new WorldSideData(WorldSideKey.Asia, "Азия"));
            WorldSideNamesForKeys.Add(new WorldSideData(WorldSideKey.Africa, "Африка"));
            WorldSideNamesForKeys.Add(new WorldSideData(WorldSideKey.NorthAmerica, "Северная Америка"));
            WorldSideNamesForKeys.Add(new WorldSideData(WorldSideKey.SouthAmerica, "Южная Америка"));
            WorldSideNamesForKeys.Add(new WorldSideData(WorldSideKey.Oceania, "Океания"));


            World = new Dictionary<WorldSideKey, Dictionary<string, CountryData>>();

            foreach (WorldSideKey key in Enum.GetValues(typeof(WorldSideKey)))
            {
                if (key != WorldSideKey.World)
                {
                    string path = $"{hostDataPath}/{key}.txt";
                    var dict = CsvParser.GetCountriesFromFile(path);
                    World.Add(key, dict);
                }
            }
        }

        public void StartBot()
        {
            client.StartReceiving(HandleUpdateAsync, Error);

            Console.ReadLine();
        }
        private static IReplyMarkup GetReplyMarkup(List<string> options)
        {
            var reply = new List<List<KeyboardButton>>();

            foreach (var option in options)
            {
                reply.Add(new List<KeyboardButton> { option });
            }

            return new ReplyKeyboardMarkup(reply);
        }

        // Process methods
        private async static Task ProcessGreetings(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            switch (message.Text)
            {
                case "/start":
                    long id = message.Chat.Id;

                    BotUsers.InitUser(id);

                    await botClient.SendTextMessageAsync(id, "Привет, я бот, помогаю тебе учить страны, и веду статистику!", replyMarkup: new ReplyKeyboardRemove());

                    await SendWorldSideChoise(botClient, update, cancellationToken);

                    BotUsers.UpdateUserState(id, BotState.WorldSideChoise);
                    break;

                default:
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Для запуска напиши /start", replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }
        private async static Task SendWorldSideChoise(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            List<string> options = new List<string>();

            foreach (var side in WorldSideNamesForKeys)
            {
                options.Add(side.rus_name);
            }

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Ну чо, что угадываем?", replyMarkup: GetReplyMarkup(
               options));
        }
        private async static Task ProcessWorldSideChoose(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            Console.WriteLine("Вы ввели: " + message.Text);
            long id = message.Chat.Id;

            foreach (var side in WorldSideNamesForKeys)
            {
                Console.WriteLine("side.rus_name: " + side.rus_name);
                if (side.rus_name == message.Text)
                {
                    Console.WriteLine("Correct:");

                    BotUsers.UpdateUserWorldSideKey(id, side.key);

                    BotUsers.UpdateUserState(id, BotState.CountryQuestion);

                    BotUsers.InitUserRemainingDictionary(id, side.key);

                    await botClient.SendTextMessageAsync(id, "Я говорю страну, ты столицу", replyMarkup: new ReplyKeyboardRemove());

                    await ProcessCountryQuestion(botClient, id);
                    return;
                }
            }

            await botClient.SendTextMessageAsync(id, "Не понял)");
            ConsoleErrorOutput(message);
        }
        private static WorldSideKey getWeightedRandomKey(long id)
        {
            WorldSideKey result = WorldSideKey.Europe; // Просто начальное значение

            var chanceparams = new List<WeightedChanceParam>();

            var userRemainingIndexes = BotUsers.getremainingIndexes(id);

            foreach (WorldSideKey key in Enum.GetValues(typeof(WorldSideKey)))
            {
                if (key != WorldSideKey.World)
                {
                    int weight = userRemainingIndexes[key].Count();

                    if (weight != 0)
                    {
                        chanceparams.Add(new WeightedChanceParam(() => result = key, weight));
                    } 
                }
            }

            WeightedChanceExecutor weightedChanceExecutor = new WeightedChanceExecutor(chanceparams);
     
            weightedChanceExecutor.Execute();

            return result;
        }
        private async static Task ProcessCountryQuestion(ITelegramBotClient botClient, long id)
        {
            WorldSideKey currKey = BotUsers.getWorldSideKey(id);

            int seed = Guid.NewGuid().GetHashCode();

            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            if(currKey == WorldSideKey.World)
            {
                currKey = getWeightedRandomKey(id);
                BotUsers.UpdateUserTempSideKey(id, currKey);
            }

            var userRemainingIndexes = BotUsers.getremainingIndexes(id);

            int randomRemainingIndex = rnd.Next(0, userRemainingIndexes[currKey].Count());

            int origElementPos = userRemainingIndexes[currKey][randomRemainingIndex];

            string country = World[currKey].ElementAt(origElementPos).Key;

            BotUsers.UpdateGuessIndex(id, origElementPos);

            userRemainingIndexes[currKey].RemoveAt(randomRemainingIndex);

            BotUsers.UpdateUserIndexDictionary(id, userRemainingIndexes);

            BotUsers.UpdateUserState(id, BotState.CountryGuessing);

            // Загадывание страны в чате
            await botClient.SendTextMessageAsync(id, country, replyMarkup: new ReplyKeyboardRemove());
        }
        private async static Task ProcessCountryGuessing(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            long id = message.Chat.Id;

            WorldSideKey currKey = BotUsers.getWorldSideKey(id);

            if(currKey == WorldSideKey.World)
            {
                currKey = BotUsers.getTempWorldSideKey(id);
            }

            int userExpectedIndex = BotUsers.getguessIndex(id);

            string userExpectedAnswer = World[currKey].ElementAt(userExpectedIndex).Value.capital;

            if(message.Text != userExpectedAnswer)
            {
                await botClient.SendTextMessageAsync(id, $"ОШИБКА!\nОтвет - {userExpectedAnswer}", replyMarkup: new ReplyKeyboardRemove());
            } 

            var dict = BotUsers.getremainingIndexes(id);

            foreach(var item in dict)
            {
                int indexLeft = item.Value.Count();

                if (indexLeft > 0)
                {
                    BotUsers.UpdateUserState(id, BotState.CountryQuestion); // не имеет смысла из-за прямого вызова метода далее

                    await ProcessCountryQuestion(botClient, id);

                    return;
                }  
            }

            await botClient.SendTextMessageAsync(id, "Категория пуста. Го некст", replyMarkup: new ReplyKeyboardRemove());

            await SendWorldSideChoise(botClient, update, cancellationToken);

            BotUsers.UpdateUserState(id, BotState.WorldSideChoise);
        }

        // Main handler
        private async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message.Text != null)
            {
                long id = update.Message.Chat.Id;

                if (!BotUsers.UserContainsKey(id))
                {
                    //update.Message.Text = "/start";
                    await ProcessGreetings(botClient, update, cancellationToken);
                }
                else
                {
                    switch (BotUsers.getBotState(id))
                    {
                        case BotState.WorldSideChoise:
                            await ProcessWorldSideChoose(botClient, update, cancellationToken);
                            break;
                        case BotState.CountryGuessing:
                            await ProcessCountryGuessing(botClient, update, cancellationToken);
                            break;
                        case BotState.CountryQuestion:
                            await ProcessCountryQuestion(botClient, id);
                            break;
                    }
                }
            }
        }

        // Work with errors
        private static void ConsoleErrorOutput(Message message)
        {
            long id = message.Chat.Id;
            Console.WriteLine($"Unsupported message: {message.Text} | Chat id: {id} | Step: {BotUsers.getBotState(id)}");
        }
        private static async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(exception));
        }
    }
}