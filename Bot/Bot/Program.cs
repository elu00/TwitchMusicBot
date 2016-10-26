using Bot.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;

namespace Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            //Parse original configuration from config.txt
            if (!File.Exists("config.txt"))
            {
                Console.WriteLine("No valid configuration text file found, press enter to exit");
                Console.ReadLine();
                Environment.Exit(1);
                //Exit procedure
            }
            StreamReader config = new StreamReader("config.txt");
            string username = config.ReadLine();
            string oauth = config.ReadLine();
            string channel = config.ReadLine();
            Console.WriteLine("Configuration loaded");
            //Make sure parameters are not null
            
            //Initialize List
            SongList songs = new SongList();
            Console.WriteLine("List inititialized");

            //Connect to twitch IRC channel
            TwitchClient client = new TwitchClient(new ConnectionCredentials(username, oauth), channel, '!', '!', true);
            Console.WriteLine("Client created. Joining channel " + channel + " with username " + username);
            client.Connect();

            //Listen for commands or events
            client.OnJoinedChannel += (sender, e) =>
            {
                Console.WriteLine("Connected");
                client.SendMessage("Bot intialized");
            };
            client.OnChatCommandReceived += (sender, e) => chatCommandReceived(sender, e, songs, client);
            client.OnChannelStateChanged += onChannelStateChanged;
            client.ChatThrottler = new TwitchLib.Services.MessageThrottler(5, TimeSpan.FromSeconds(60));
            client.ChatThrottler.OnClientThrottled += onClientThrottled;
            client.ChatThrottler.OnThrottledPeriodReset += onThrottlePeriodReset;
            client.WhisperThrottler = new TwitchLib.Services.MessageThrottler(5, TimeSpan.FromSeconds(60));
            //allow custom input
            while (true)
            {
                client.SendMessage(Console.ReadLine());
            }
        }
        //Command implementation
        public static void chatCommandReceived(object sender, TwitchClient.OnChatCommandReceivedArgs e, SongList songs, TwitchClient client)
        {
            Console.WriteLine("Command Recieved");
            string command = e.Command.Command;
            string username = e.Command.ChatMessage.Username;
            string args = e.Command.ArgumentsAsString;
            switch (command)
            {
                case "request":
                    if (args == "")
                    {
                        client.SendMessage("Please specify the URL of your song with !song <url>");
                        break;
                    }
                    SongRequest request = new SongRequest(username, args);
                        client.SendMessage(username + "-> " + songs.AddSong(request));
                    break;
                case "spot":
                    int spot = songs.GetSpot(username);
                    if(spot == -1)
                    {
                        client.SendMessage(username + "-> You are not currently on the list");
                        break;
                    }
					client.SendMessage(username + "-> Your current spot in the list is " + spot.ToString());
					break;
                case "remove":
                    client.SendMessage(username + "-> " + songs.RemoveSong(username));
                    break;
                case "list":
                    client.SendMessage(username + "-> " + songs.GetList());
                    break;
                case "next":
                    //hot fix - will be open to all moderators in next commit
                    if (username.ToLower() == "rich_brown")
                    {
                        client.SendMessage(username + "-> " + songs.Next());
                        break;
                    }
                    else
                    {
                        client.SendMessage(username + "-> Sorry, only the channel owner can use this command!");
                        break;
                    }
                case "commands":
                    client.SendMessage("Available commands are: !request <song>, !spot, !remove, !list, !next, !currentsong, !change");
                    break;
                //specific rules for this streamer, feel free to change to suit your needs lol
                case "rules":
                    client.SendMessage("Song/loop must be under 90 seconds. Please try to only use youtube/soundcloud urls");
                    break;
                case "change":
                    client.SendMessage(username + "-> " + songs.ChangeRequest(username, args));
                    break;
                case "currentsong":
                    client.SendMessage(username + "-> " + songs.GetCurrentSong());
                    break;
				default:
					client.SendMessage("Command not recognized");
					break;
            }

        }

        public static void onClientThrottled(object sender, TwitchLib.Services.MessageThrottler.OnClientThrottledArgs e)
        {
            Console.WriteLine($"The message '{e.Message}' was blocked by a message throttler. Throttle period duration: {e.PeriodDuration.TotalSeconds}.\n\nMessage violation: {e.ThrottleViolation}");
        }

        public static void onThrottlePeriodReset(object sender, TwitchLib.Services.MessageThrottler.OnThrottlePeriodResetArgs e)
        {
            Console.WriteLine($"The message throttle period was reset.");
        }
        private static void onChannelStateChanged(object sender, TwitchClient.OnChannelStateChangedArgs e)
        {
            Console.WriteLine($"Channel: {e.Channel}\nSub only: {e.ChannelState.SubOnly}\nEmotes only: {e.ChannelState.EmoteOnly}\nSlow mode: {e.ChannelState.SlowMode}\nR9K: {e.ChannelState.R9K}");
        }
    }
}
