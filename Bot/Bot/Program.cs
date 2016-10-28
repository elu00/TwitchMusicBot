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
    //Stores all strings used for chat response/interactions for easy modification
    struct msgs
    {
        public const string About = "Open source bot made by Ethan Lu, browse my source and report issues at https://github.com/elu00/TwitchMusicBot";
        public const string EmptyRequest = "Please specify the URL of your song with !request <url>";
        public const string NotOnList = "You are not currently on the list";
        public const string Ping = " -> ";
        public const string CurrentSpot = "Your current spot in the list is ";
        public const string ModsOnly = "Sorry, only the channel moderators can use this command!";
        public const string Rules = "Song/loop must be under 90 seconds.Please try to only use youtube/soundcloud urls";
        public const string Commands = "Available commands are: !request <song>, !spot, !drop, !list, !next, !currentsong, !change <song>";
        public const string UnknownCommand = "Command not recognized";
    }
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
            //Initialization
            StreamReader config = new StreamReader("config.txt");
            string username = config.ReadLine();
            string oauth = config.ReadLine();
            string channel = config.ReadLine();
            Console.WriteLine("Configuration loaded");
                      
            //Initialize List and Mods
            SongList songs = new SongList();
            Console.WriteLine("List inititialized");
            List<string> mods = new List<string>();

            //Connect to twitch IRC channel
            TwitchClient client = new TwitchClient(new ConnectionCredentials(username, oauth), channel, '!', '!', true);
            Console.WriteLine("Client created. Joining channel " + channel + " with username " + username);
            //Event and Command Handlers
            client.OnJoinedChannel += (sender, e) =>
            {
                Console.WriteLine("Connected");
                client.SendMessage("Bot intialized");
            };
            client.OnModeratorsReceived += (sender, e) =>
            {
                Console.WriteLine("Mods recieved");
                mods.AddRange(e.Moderators);
            };
            client.OnChatCommandReceived += (sender, e) => chatCommandReceived(sender, e, songs, mods);
            client.OnChannelStateChanged += onChannelStateChanged;
            client.ChatThrottler = new TwitchLib.Services.MessageThrottler(5, TimeSpan.FromSeconds(60));
            client.ChatThrottler.OnClientThrottled += onClientThrottled;
            client.ChatThrottler.OnThrottledPeriodReset += onThrottlePeriodReset;
            client.WhisperThrottler = new TwitchLib.Services.MessageThrottler(5, TimeSpan.FromSeconds(60));

            //Connect
            client.Connect();
            client.GetChannelModerators();

            //allow custom input
            while (true)
            {
                client.SendMessage(Console.ReadLine());
            }
        }
        //Command implementation
        public static void chatCommandReceived(object sender, TwitchClient.OnChatCommandReceivedArgs e, SongList songs, List<string> mods)
        {
            Console.WriteLine("Command Recieved");
            TwitchClient client = sender as TwitchClient;
            string command = e.Command.Command;
            string username = e.Command.ChatMessage.Username;
            string args = e.Command.ArgumentsAsString;
            switch (command.ToLower())
            {
                case "request":
                    if (args == "")
                    {
                        client.SendMessage(msgs.EmptyRequest);
                        break;
                    }
                    SongRequest request = new SongRequest(username, args);
                        client.SendMessage(username + msgs.Ping + songs.AddSong(request));
                    break;
                case "spot":
                    int spot = songs.GetSpot(username);
                    if(spot == -1)
                    {
                        client.SendMessage(username + msgs.Ping + msgs.NotOnList);
                        break;
                    }
					client.SendMessage(username + msgs.Ping + msgs.CurrentSpot + spot.ToString());
					break;
                case "drop":
                    client.SendMessage(username + msgs.Ping + songs.RemoveSong(username));
                    break;
                case "list":
                    client.SendMessage(username + msgs.Ping + songs.GetList());
                    break;
                case "commands":
                    client.SendMessage(msgs.Commands);
                    break;
                case "rules":
                    client.SendMessage(msgs.Rules);
                    break;
                case "change":
                    client.SendMessage(username + msgs.Ping + songs.ChangeRequest(username, args));
                    break;
                case "currentsong":
                    client.SendMessage(songs.GetCurrentSong());
                    break;
                case "about":
                    client.SendMessage(msgs.About);
                    break;
                // Moderator restricted functions
                case "next":
                    if (mods.Contains(username))
                    {
                        client.SendMessage(songs.Next());
                        break;
                    }
                    else
                    {
                        client.SendMessage(username + msgs.Ping + msgs.ModsOnly);
                        break;
                    }
                case "remove":
                    if(mods.Contains(username))
                    {
                        client.SendMessage("(Mod Removal)" + args + msgs.Ping + songs.RemoveSong(args));
                        break;
                    }
                    else
                    {
                        client.SendMessage(username + msgs.Ping + msgs.ModsOnly);
                        break;
                    }
                default:
					client.SendMessage(username + msgs.Ping+ msgs.UnknownCommand);
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
