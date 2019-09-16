namespace GamerBot.EventArgs
{
    public class BotMessageEventArgs : System.EventArgs
    {
        public string Username { get; set; }
        public bool IsSubscriber { get; set; }
        public string Message { get; set; }
        public Bot Bot { get; }

        public BotMessageEventArgs(Bot bot)
        {
            Bot = bot;
        }
    }
}