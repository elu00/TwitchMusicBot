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

            //Set command identifiers
            client.AddChatCommandIdentifier('!');
            Console.WriteLine("Client created. Joining channel " + channel + " with username " + username);
            client.Connect();

            //Listen for commands or events
            client.OnJoinedChannel += (sender, e) =>
            {
                Console.WriteLine("Connected");
                client.SendMessage("Bot intialized");
            };
            client.OnChatCommandReceived += (sender, e) => chatCommandReceived(sender, e, songs, client);
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
            string command = e.Command.Command;
            string username = e.Command.ChatMessage.Username;
            string args = e.Command.ArgumentsAsString;
            switch (command)
            {
                case "request":
                    SongRequest request = new SongRequest(username, args);
                    if (songs.AddSong(request))
                    {
                        client.SendMessage(username + "-> Request succesfully added. Your current spot in the list is" + songs.GetSpot(username));
                    }
                    else
                    {
                        client.SendMessage(username + "-> Error: you are already on the list. If you want to change your request, !change it.");
                    }
                    break;
                case "spot":
                    int spot = songs.GetSpot(username);
                    if(spot == -1)
                    {
                        client.SendMessage(username + "-> You are not currently on the list");
                        break;
                    }
					client.SendMessage(username + "-> Your current spot in the list is");
					break;
                case "remove":
                    if(songs.RemoveSong(username))
                    {
                    client.SendMessage(username + "-> Your request has been removed from the list");
                    }
                    else
                    {
                        client.SendMessage(username + "-> Error: you are not currently on the list");
                    }
                    break;
                case "list":
                    client.SendMessage(username + "->" + songs.GetList());
                    break;
                case "next":
                    client.SendMessage(username + "->" + songs.Next());
                    break;
                case "change":
                    client.SendMessage(username + "-> Oops, this hasn't been implemented yet");
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
    }
}
