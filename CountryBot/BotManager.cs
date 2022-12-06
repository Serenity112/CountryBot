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
    // Keys
    public enum WorldSideKey
    {
        World,
        Europe,
        Asia,
        Africa,
        America,
        Oceania,
    }   
    //State
    public enum BotState
    {
       // Greetings,
        WorldSideChoise,
        CountryQuestion,
        CountryGuessing,
    }
    // Main class
    class BotManager
    {
        private const string hostDataPath = @"..\..\..\countries";
        // Client
        private static ITelegramBotClient client { get; set; }

        // WorldSide
        private struct KeyLocalization
        {
            public KeyLocalization(WorldSideKey key, string name)
            {
                this.key = key;
                this.rus_name = name;
            }

            public WorldSideKey key { get; set; }
            public string rus_name { get; set; }
        }
       
        // Lists

        private static List<KeyLocalization> keyLocalizations { get; set; }

        public BotManager(string telegramToken)
        {
            client = new TelegramBotClient(telegramToken);

            keyLocalizations = new List<KeyLocalization>();
            keyLocalizations.Add(new KeyLocalization(WorldSideKey.World, "Мир"));
            keyLocalizations.Add(new KeyLocalization(WorldSideKey.Europe, "Европа"));
            keyLocalizations.Add(new KeyLocalization(WorldSideKey.Asia, "Азия"));
            keyLocalizations.Add(new KeyLocalization(WorldSideKey.Africa, "Африка"));
            keyLocalizations.Add(new KeyLocalization(WorldSideKey.America, "Америка"));
            keyLocalizations.Add(new KeyLocalization(WorldSideKey.Oceania, "Океания"));
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

                    Database.InitUser(id);

                    await botClient.SendTextMessageAsync(id, "Привет, я бот, помогаю тебе учить страны, и веду статистику!", replyMarkup: new ReplyKeyboardRemove());

                    await SendWorldSideChoise(botClient, update, cancellationToken);

                    Database.UpdateUserState(id, BotState.WorldSideChoise);
                    break;

                default:
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Для запуска напиши /start", replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }
        private async static Task SendWorldSideChoise(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            List<string> options = new List<string>();

            foreach (var side in keyLocalizations)
            {
                options.Add(side.rus_name);
            }

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Ну чо, что угадываем?", replyMarkup: GetReplyMarkup(
               options));
        }
        private async static Task ProcessWorldSideChoose(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            long id = message.Chat.Id;

            foreach (var side in keyLocalizations)
            {
                if (side.rus_name == message.Text)
                {
                    Database.UpdateUserWorldSideKey(id, side.key);

                    Database.UpdateUserState(id, BotState.CountryQuestion);

                    Database.InitUserRemainingDictionary(id, side.key);

                    await botClient.SendTextMessageAsync(id, "Я говорю страну, ты столицу", replyMarkup: new ReplyKeyboardRemove());

                    await ProcessCountryQuestion(botClient, id);
                    return;
                }
            }

            await botClient.SendTextMessageAsync(id, "Не понял)");
            ConsoleErrorOutput(message);
        }   
        private async static Task ProcessCountryQuestion(ITelegramBotClient botClient, long id)
        {
            WorldSideKey currKey = Database.getWorldSideKey(id);

            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            int randomRemainingIndex = rnd.Next(1, Database.getRemainingIndexSize(id) + 1);
             
            int countryNumIndex = Database.getCountryNumByIndex(id, randomRemainingIndex);

            string country = Database.getCountryByIndex(currKey, countryNumIndex);

            Database.UpdateGuessIndex(id, countryNumIndex);

            Database.deleteRemainingIndex(id, randomRemainingIndex);

            Database.shiftRemainingIndex(id, randomRemainingIndex);

            Database.UpdateUserState(id, BotState.CountryGuessing);

            // Загадывание страны в чате
            await botClient.SendTextMessageAsync(id, country, replyMarkup: new ReplyKeyboardRemove());
        }
        private async static Task ProcessCountryGuessing(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            long id = message.Chat.Id;

            switch (message.Text)
            {
                case "/stop":
                    await botClient.SendTextMessageAsync(id, "Ок(", replyMarkup: new ReplyKeyboardRemove());

                    await SendWorldSideChoise(botClient, update, cancellationToken);

                    Database.UpdateUserState(id, BotState.WorldSideChoise);

                    Database.ClearRemainingIndexes(id);

                    Database.ClearTempValues(id);

                    return;
                default:
                    WorldSideKey currKey = Database.getWorldSideKey(id);

                    int userExpectedIndex = Database.getGuessIndex(id);

                    string userExpectedAnswer = Database.getCapitalByIndex(currKey, userExpectedIndex);

                    if (message.Text == userExpectedAnswer)
                    {
                        Database.updateCountryInfo(id, userExpectedAnswer, true);

                    } else
                    {
                        Database.updateCountryInfo(id, userExpectedAnswer, false);

                        await botClient.SendTextMessageAsync(id, $"ОШИБКА!\nОтвет - {userExpectedAnswer}", replyMarkup: new ReplyKeyboardRemove());
                    }

                    int remainingSize = Database.getRemainingIndexSize(id);

                    if(remainingSize > 0)
                    {
                        Database.UpdateUserState(id, BotState.CountryQuestion); // не имеет смысла из-за прямого вызова метода далее

                        await ProcessCountryQuestion(botClient, id);
                    } else
                    {
                        await botClient.SendTextMessageAsync(id, "Категория пуста. Го некст", replyMarkup: new ReplyKeyboardRemove());

                        await SendWorldSideChoise(botClient, update, cancellationToken);

                        Database.UpdateUserState(id, BotState.WorldSideChoise);

                        Database.ClearTempValues(id);
                    }

                    break;
            }
        }

        // Main handler
        private async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message.Text != null)
                {
                    long id = update.Message.Chat.Id;

                    if (!Database.UserContainsKey(id))
                    {
                        await ProcessGreetings(botClient, update, cancellationToken);
                    }
                    else
                    {
                        switch (Database.getBotState(id))
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
            } catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message} | Chat id: {update.Message.Chat.Id}");
            }
            
        }

        // Work with errors
        private static void ConsoleErrorOutput(Message message)
        {
            long id = message.Chat.Id;
            Console.WriteLine($"Unsupported message: {message.Text} | Chat id: {id} | Step: {Database.getBotState(id)}");
        }
        private static async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(exception));
        }
    }
}