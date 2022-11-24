namespace CountryBot
{
    class Program
    {
        static void Main(string[] args)
        {
           string token  = "5723619088:AAHNrrTUIQ9cmoC70dEWZhnv_l-vhGfQGnA";

            var bot = new BotManager(token);

            bot.StartBot();
        }
    }
}
