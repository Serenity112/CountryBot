using System;

namespace CountryBot
{
    class Program
    {
        static void Main(string[] args)
        {
            string token  = "xxx";

            var bot = new BotManager(token);

            bot.StartBot();
        }
    }
}
