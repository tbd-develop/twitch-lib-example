using System;
using GamerBot.EventArgs;
using Microsoft.Extensions.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace GamerBot
{
    public class Bot
    {
        // These delegates are for the message signatures of our events, 
        // they could be defined anywhere in the library to be fair, 
        // I just prefer putting them at the top
        public delegate void UserJoinedDelegate(Bot sender, string username);
        public delegate void RaidDelegate(Bot sender, string message);
        public delegate void MessageReceivedDelegate(BotMessageEventArgs message);

        // These are the corresponding events to which our delegates are wired
        // These are public because they are subscribed to from outside the 
        public event UserJoinedDelegate OnUserJoined;
        public event RaidDelegate OnRaidStarted;
        public event MessageReceivedDelegate OnMessageReceived;

        // Just an object to do something with chatters, not necessarily useful 
        // but represents state that you could hold in the bot as the bot knows more about what's going through it
        public Chatters Chatters { get; }
        // Another variable to track, again representing state that this Bot is tracking
        public int RaidsSinceStreamBegun { get; private set; }

        // The twitch client from TwitchLib itself, it's here because we create it in the constructor
        // and then connect it in the start method
        private readonly TwitchClient _client;

        // Here comes our configuration in to the constructor
        public Bot(IConfiguration configuration)
        {
            // Set up our state 
            Chatters = new Chatters();
            RaidsSinceStreamBegun = 0;

            // Twitch Client can be constructed without arguments
            _client = new TwitchClient();

            // We can initialize the client with values from the configuration
            _client.Initialize(new
                ConnectionCredentials(
                    configuration["twitch:bot:name"], // This is in appsettings.json
                    configuration["twitch:bot:oauth"] // This could be in there too, but it's better as a usersecret
                ), configuration["twitch:bot:channel"]); // Store this in appsettings.json as well

            // Subscribe to the events of the TwitchClient, 
            // these are not the events of the bot, these are the events of the Client. 
            _client.OnUserJoined += OnClientUserJoined;
            _client.OnJoinedChannel += OnClientJoinedChannel;
            _client.OnMessageReceived += OnClientMessageReceived;
            _client.OnRaidNotification += OnClientRaidNotification;
        }

        /// <summary>
        /// Good practice says that if you subscribe to events, you should be able to unsubscribe to them
        /// Enough messing with events, you'll get some fun memory leaks. 
        /// </summary>
        ~Bot()
        {
            // If this is getting called again, no point crashing
            if (_client == null) return;

            // Unsubscribe from each event with the -=
            _client.OnUserJoined -= OnClientUserJoined;
            _client.OnJoinedChannel -= OnClientJoinedChannel;
            _client.OnMessageReceived -= OnClientMessageReceived;
            _client.OnRaidNotification -= OnClientRaidNotification;
        }

        /// <summary>
        ///  Start is literally just connect now
        /// </summary>
        public void Start()
        {
            _client.Connect();
        }

        // This is the client event, we're subscribing to it, and it's private because nobody need know about this
        // On ClientJoined might not be an event we care very much about, so we don't have a public bot event 
        // exposed representing this. 
        private void OnClientJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine($"Client Joined Channel ({e.BotUsername},{e.Channel})");
        }

        // Again, the client event, in this case we can log something and then we invoke the event if anybody 
        // is subscribed to it. Try removing the subscription from Program.cs, you'll see that it doesn't break this
        private void OnClientUserJoined(object sender, OnUserJoinedArgs e)
        {
            Console.WriteLine($"User {e.Username} joined {e.Channel}");

            OnUserJoined?.Invoke(this, e.Username);
        }

        // And again, we have a client event. This time we're going to put a bit more meat on the bones.
        // Carrots in the basket or a suitably less carnivorous way to refer to a more substantial piece of code
        // We use our chatters, we register out "chatter", they are talking this is someone who is active
        // We then invoke the event, pulling what we need from incoming message. 
        private void OnClientMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            Chatters.Register(e.ChatMessage.Username);

            OnMessageReceived?.Invoke(new BotMessageEventArgs(this)
            {
                Username = e.ChatMessage.Username,
                IsSubscriber = e.ChatMessage.IsSubscriber,
                Message = e.ChatMessage.Message
            });
        }

        // Last example, raid notification. Log it out, increment the count and then call the subscription. 
        private void OnClientRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            Console.WriteLine($"{e.Channel} is raiding {e.RaidNotificaiton.DisplayName}");

            RaidsSinceStreamBegun++;

            // I hate when APIs have typos in them!
            OnRaidStarted?.Invoke(this, e.RaidNotificaiton.DisplayName);
        }
    }
}