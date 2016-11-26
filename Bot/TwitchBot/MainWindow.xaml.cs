using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwitchBot.Classes;
using TwitchLib;
using TwitchLib.Models.Client;

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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string logContent = "hai";
        public MainWindow()
        {
            InitializeComponent();
            //Parse original configuration from config.txt
            if (File.Exists("config.txt"))
            {
                //Initialization
                StreamReader config = new StreamReader("config.txt");
                initUsername.Text = config.ReadLine();
                initOAuth.Text = config.ReadLine();
                initConnection.Text = config.ReadLine();
                logContent += "Configuration loaded";
            }
            //Setup Databinding
            
        }
        //Command implementation
        public void initialize(string username, string oauth, string channel)
        {
            //Initialize List and Mods
            SongList songs = new SongList();
            logContent += "List inititialized";
            List<string> mods = new List<string>();

            //Connect to twitch IRC channel
            TwitchClient client = new TwitchClient(new ConnectionCredentials(username, oauth), channel, '!', '!', true);
            logContent += "Client created. Joining channel " + channel + " with username " + username;
            initText.Text = "Connecting....";
            //Event and Command Handlers
            client.OnJoinedChannel += (sender, e) =>
            {
                logContent += "Connected!";
                client.SendMessage("Bot intialized");
            };
            client.OnModeratorsReceived += (sender, e) =>
            {
                logContent+="Mods recieved";
                mods.AddRange(e.Moderators);
            };
            client.OnChatCommandReceived += (sender, e) => chatCommandReceived(sender, e, songs, mods);

            //Connect
            client.Connect();
            client.GetChannelModerators();
        }
        private void chatCommandReceived(object sender, TwitchLib.Events.Client.OnChatCommandReceivedArgs e, SongList songs, List<string> mods)
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
        private void log(string msg, TwitchClient client, bool chat = true)
        {
            if (chat)
            {
                client.SendMessage(msg);
            }
            logContent += msg;
        }

        private void Initialize_Click(object sender, RoutedEventArgs e)
        {
            disableInitTextBoxes();
            initialize(initUsername.Text, initOAuth.Text, initConnection.Text);
        }
        private void disableInitTextBoxes()
        {
            initUsername.IsEnabled = false;
            initOAuth.IsEnabled = false;
            initConnection.IsEnabled = false;
            btnInit.IsEnabled = false;
        }
    }
}
