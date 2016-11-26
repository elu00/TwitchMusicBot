using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchBot.Classes;

namespace TwitchBot
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
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            /*
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

            //Connect
            client.Connect();
            client.GetChannelModerators();
            */
        }
        //Command implementation
        private static void chatCommandReceived(object sender, TwitchLib.Events.Client.OnChatCommandReceivedArgs e, SongList songs, List<string> mods)
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
                        log(msgs.EmptyRequest, client);
                        break;
                    }
                    SongRequest request = new SongRequest(username, args);
                    log(username + msgs.Ping + songs.AddSong(request), client);
                    break;
                case "spot":
                    int spot = songs.GetSpot(username);
                    if (spot == -1)
                    {
                        log(username + msgs.Ping + msgs.NotOnList, client);
                        break;
                    }
                    log(username + msgs.Ping + msgs.CurrentSpot + spot.ToString(), client);
                    break;
                case "drop":
                    log(username + msgs.Ping + songs.RemoveSong(username), client);
                    break;
                case "list":
                    log(username + msgs.Ping + songs.GetList(), client);
                    break;
                case "commands":
                    log(msgs.Commands, client);
                    break;
                case "rules":
                    log(msgs.Rules, client);
                    break;
                case "change":
                    log(username + msgs.Ping + songs.ChangeRequest(username, args), client);
                    break;
                case "currentsong":
                    log(songs.GetCurrentSong(), client);
                    break;
                case "about":
                    log(msgs.About, client);
                    break;
                // Moderator restricted functions
                case "next":
                    if (mods.Contains(username))
                    {
                        log(songs.Next(), client);
                        break;
                    }
                    else
                    {
                        log(username + msgs.Ping + msgs.ModsOnly, client);
                        break;
                    }
                case "remove":
                    if (mods.Contains(username))
                    {
                        log("(Mod Removal)" + args + msgs.Ping + songs.RemoveSong(args), client);
                        break;
                    }
                    else
                    {
                        log(username + msgs.Ping + msgs.ModsOnly, client);
                        break;
                    }
                default:
                    log(username + msgs.Ping + msgs.UnknownCommand, client);
                    break;
            }

        }
        private static void log(string msg, TwitchClient client)
        {
            client.SendMessage(msg);
            // Do stuff with the GUI
        }
    }
}
