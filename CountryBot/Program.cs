namespace CountryBot
{
    class Program
    {
        static void Main(string[] args)
        {
           string token  = "your_token";

            var bot = new BotManager(token);

            bot.StartBot();
        }
    }
}
