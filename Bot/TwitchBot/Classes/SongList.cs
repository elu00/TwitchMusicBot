using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TwitchLib;
using TwitchLib.Models.Client;

namespace TwitchBot.Classes
{
    //Stores all strings used for chat response/interactions for easy modification
    internal struct Msgs
    {
        public const string About = "Open source bot made by Ethan Lu, browse my source and report issues at https://github.com/elu00/TwitchMusicBot";
        public const string EmptyRequest = "Please specify the URL of your song with !request <song url> (without brackets)";
        public const string NotOnList = "You are not currently on the list";
        public const string Ping = " -> ";
        public const string CurrentSpot = "Your current spot in the list is ";
        public const string ModsOnly = "Sorry, only the channel moderators can use this command!";
        public const string Rules = "Song/loop must be under 90 seconds.Please try to only use youtube/soundcloud urls";
        public const string Commands = "Available commands are: !request <song url>, !spot, !drop, !list, !next, !currentsong, !change <song url>";
        public const string UnknownCommand = "Command not recognized";
    }
    internal class SongList : INotifyPropertyChanged
    {
        private TwitchClient _client;
        private List<string> _mods = new List<string>();
        private ObservableCollection<SongRequest> _list = new ObservableCollection<SongRequest>();
        public ObservableCollection<SongRequest> List
        {
            get
            {
                return _list;
            }
            protected set
            {
                _list = value;
            }
        }
        private string _logContent;

        public string LogContent
        {
            get { return _logContent; }
            protected set { _logContent = value; }
        }
        public SongList()
        {
            _logContent = "List initialized \n";
        }
        /// <summary>
        /// Initializes the Connection to Twitch chat servers and invokes event handlers
        /// </summary>
        /// <param name="username">The username used to connect to chat as</param>
        /// <param name="oauth">OAuth token for the corresponding user. "oauth:" prefix is optional.</param>
        /// <param name="channel">The chat channel to connect to</param>
        public void Initialize(string username, string oauth, string channel)
        {
            //Connect to twitch IRC channel
            _client = new TwitchClient(new ConnectionCredentials(username, oauth), channel, '!', '!', true);
            Log("Client created. Joining channel " + channel + " with username " + username, false);
            //Event and Command Handlers
            _client.OnJoinedChannel += (sender, e) =>
            {
                Log("Connected!", false);
                _client.SendMessage("Bot intialized");
            };
            _client.OnModeratorsReceived += (sender, e) =>
            {
                Log("Mods recieved", false);
                _mods.AddRange(e.Moderators);
            };
            _client.OnChatCommandReceived += ChatCommandReceived;

            //Connect
            _client.Connect();
            _client.GetChannelModerators();
        }
        private void ChatCommandReceived(object sender, TwitchLib.Events.Client.OnChatCommandReceivedArgs e)
        {
            Console.WriteLine(@"Command Recieved");
            string command = e.Command.Command;
            string username = e.Command.ChatMessage.Username;
            string args = e.Command.ArgumentsAsString;
            switch (command.ToLower())
            {
                case "request":
                    if (args == "")
                    {
                        Log(Msgs.EmptyRequest);
                        break;
                    }
                    SongRequest request = new SongRequest(username, args);
                    Log(username + Msgs.Ping + AddSong(request));
                    break;
                case "spot":
                    int spot = GetSpot(username);
                    if (spot == -1)
                    {
                        Log(username + Msgs.Ping + Msgs.NotOnList);
                        break;
                    }
                    Log(username + Msgs.Ping + Msgs.CurrentSpot + spot.ToString());
                    break;
                case "drop":
                    Log(username + Msgs.Ping + RemoveSong(username));
                    break;
                case "list":
                    Log(username + Msgs.Ping + GetList());
                    break;
                case "commands":
                    Log(Msgs.Commands);
                    break;
                case "rules":
                    Log(Msgs.Rules);
                    break;
                case "change":
                    Log(username + Msgs.Ping + ChangeRequest(username, args));
                    break;
                case "currentsong":
                    Log(GetCurrentSong());
                    break;
                case "about":
                    Log(Msgs.About);
                    break;
                // Moderator restricted functions
                case "next":
                    if (_mods.Contains(username))
                    {
                        Log(Next());
                        break;
                    }
                    else
                    {
                        Log(username + Msgs.Ping + Msgs.ModsOnly);
                        break;
                    }
                case "remove":
                    if (_mods.Contains(username))
                    {
                        Log("(Mod Removal)" + args + Msgs.Ping + RemoveSong(args));
                        break;
                    }
                    else
                    {
                        Log(username + Msgs.Ping + Msgs.ModsOnly);
                        break;
                    }
                default:
                    Log(username + Msgs.Ping + Msgs.UnknownCommand);
                    break;
            }

        }
        /// <summary>
        /// Adds a SongRequest object to the lsit
        /// </summary>
        /// <param name="song"></param>
        /// <returns>String denoting success/failure</returns>
        public string AddSong(SongRequest song)
        {
            if (_list.Any(item => item.Username==song.Username))
            {
                return "Error: you are already on the list. If you want to change your request, !change it.";
            }

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                _list.Add(song);
            }));

            return "Request successfully added. Your current spot in the list is " + (_list.Count-1);
        }
        /// <summary>
        /// Removes a song from the list
        /// </summary>
        /// <param name="username">The username corresponding to the song to be removed</param>
        /// <returns></returns>
        public string RemoveSong(string username)
        {
            foreach (SongRequest item in _list)
            {
                if (item.Username == username)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        _list.RemoveAt(_list.IndexOf(item));
                    }));
                    return "Your request has been removed from the list";
                }
            }
            return "Error: you are not currently on the list";
        }
        /// <summary>
        /// Simply advances to the next song on the list
        /// </summary>
        /// <returns>String coressponding to the success/failure of the operation.</returns>
        public string Next()
        {
            /* Will be used to implement historical data/analysis to a corresponding text file
            StreamReader rec = new StreamReader("history.txt");
            rec.WriteLine(_list[0].url)
            */
            if (_list[0] == null)
            {
                return "The list is currently empty";
            }
            else
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    _list.RemoveAt(0);
                }));
                if(_list.Count == 0)
                {
                    return "That's the end of the list!";
                }
                return "Next song: " + _list[0].Summary;
            }
        }
        /// <summary>
        /// Get the current lsit
        /// </summary>
        /// <returns>A string containing the first 5 people on the list.</returns>
        public string GetList()
        {
            if(_list.Count == 0)
            {
                return "The list is currently empty";
            }
            string contents = "The first 5 people in the list are: (Currently Up) ";
            for (int i = 0; i < 4; i++)
            {
                if(_list.Count == i)
                {
                    break;
                }
                contents += i.ToString() + ". " + _list[i].Username + ", ";

            }
            return contents;
        }
        /// <summary>
        /// Backups all current song requests to archive.txt
        /// </summary>
        /// <returns></returns>
        public void ArchiveList()
        {
            using (StreamWriter file = new StreamWriter(@"archive.txt", true))
            {
                foreach (SongRequest song in _list)
                {
                    file.WriteLine(song.Username + song.Url);
                }
            }
        }
        /// <summary>
        /// Finds the spot of the user specified
        /// </summary>
        /// <param name="username">Username to lookup</param>
        /// <returns> -1 if user is not in list, else returns their spot in the list</returns>
        public int GetSpot(string username)
        {
			foreach (SongRequest user in _list)
			{
                //iterate through _list and return index is user is inside
                if(user.Username == username)
                {
                    return _list.IndexOf(user);
                }
			}
            return -1;
        }
        /// <summary>
        /// Gets current song
        /// </summary>
        /// <returns>Current song name</returns>
        public string GetCurrentSong()
        {
            if (_list[0] == null)
            {
                return "The list is currently empty";
            }
            else
            {
                return "The current song is "+ _list[0].Summary;
            }
        }
        /// <summary>
        /// Allows user to change the URL of their request without being removed from the list
        /// </summary>
        /// <param name="username">Username to change the URl of</param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string ChangeRequest(string username, string url)
        {
            if (url == "")
            {
                return "Please specify the new URL of your request";
            }
            int index = -1;
            foreach (SongRequest user in _list)
            {
                //iterate through _list and return index is user is inside
                if (user.Username == username)
                {
                    index = _list.IndexOf(user);
                }
            }
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
            _list[index].Url = url;
            _list[index].GenerateSummary();
            }));
            return "Your request has been sucessfully updated";
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Logs messages/operations to the internal log string.
        /// </summary>
        /// <param name="msg">The message to add to the list</param>
        /// <param name="chat">An optional bool that specifies whether or not the message should be sent to the Twitch Chat. True by default</param>
        public void Log(string msg, bool chat = true)
        {
            if (chat)
            {
                _client.SendMessage(msg);
            }
            _logContent += (msg + "\n");
            NotifyPropertyChanged("LogContent");
        }
    }
}
