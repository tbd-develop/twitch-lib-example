using System;
using System.Reflection;
using GamerBot.EventArgs;
using Microsoft.Extensions.Configuration;

namespace GamerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // IConfiguration is useful, we'll load the settings from the
            // appsettings.json file. Here I include UserSecrets (look that up)
            // so that i don't accidentally show my auth token on stream
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();

            // Create an instance of the bot, hand it our configuration
            var bot = new Bot(configuration);

            // The bot offers three events to subscribe to,
            // when a user joins, when a message is received and when a raid is incoming
            // you can add more, but these work for an example now. 
            bot.OnUserJoined += BotOnOnUserJoined;
            bot.OnRaidStarted += BotOnOnRaidStarted;
            bot.OnMessageReceived += BotOnOnMessageReceived;

            // Start up the bot, connect to the irc channel and start listening to the events
            bot.Start();

            // This is how we pause the application, so if you hit a key, the bot will close
            // There are MUCH better ways of doing this.
            Console.ReadLine();
        }

        // These are our bot specific listeners. Here we could do other things
        // In on message received we get the username, message and issubscriber. 
        // There are other properties, you just have to look them up. 
        // We could just send the TwitchLib event args through again, that wouldn't be bad I don't believe
        // But adapting them inside the bot allows us to put our own API specific spin on 
        // the arguments
        private static void BotOnOnMessageReceived(BotMessageEventArgs message)
        {
            Console.WriteLine($"{message.Username} says {message.Message}");

            if (message.IsSubscriber)
            {
                Console.WriteLine("They are a sub");
            }
        }

        // One thing that you should probably decide is on being consistent, 
        // I send the Bot in here and then a message. But in the message I attach the bot to the message
        // This might be your preferred way, but from an API standpoint, it's worth considering being 
        // consistent
        private static void BotOnOnRaidStarted(Bot sender, string message)
        {
            Console.WriteLine($"Raid was started!!!!!! {message}, that's {sender.RaidsSinceStreamBegun} today!");
        }

        private static void BotOnOnUserJoined(Bot sender, string username)
        {
            Console.WriteLine($"User joined {username}");
        }
    }
}
