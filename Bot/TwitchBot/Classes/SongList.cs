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
    class SongList : INotifyPropertyChanged
    {
        private TwitchClient client;
        private List<string> mods = new List<string>();
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
        public void initialize(string username, string oauth, string channel)
        {
            //Connect to twitch IRC channel
            client = new TwitchClient(new ConnectionCredentials(username, oauth), channel, '!', '!', true);
            log("Client created. Joining channel " + channel + " with username " + username, false);
            //Event and Command Handlers
            client.OnJoinedChannel += (sender, e) =>
            {
                log("Connected!", false);
                client.SendMessage("Bot intialized");
            };
            client.OnModeratorsReceived += (sender, e) =>
            {
                log("Mods recieved", false);
                mods.AddRange(e.Moderators);
            };
            client.OnChatCommandReceived += chatCommandReceived;

            //Connect
            client.Connect();
            client.GetChannelModerators();
        }
        private void chatCommandReceived(object sender, TwitchLib.Events.Client.OnChatCommandReceivedArgs e)
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
                        log(msgs.EmptyRequest);
                        break;
                    }
                    SongRequest request = new SongRequest(username, args);
                    log(username + msgs.Ping + AddSong(request));
                    break;
                case "spot":
                    int spot = GetSpot(username);
                    if (spot == -1)
                    {
                        log(username + msgs.Ping + msgs.NotOnList);
                        break;
                    }
                    log(username + msgs.Ping + msgs.CurrentSpot + spot.ToString());
                    break;
                case "drop":
                    log(username + msgs.Ping + RemoveSong(username));
                    break;
                case "list":
                    log(username + msgs.Ping + GetList());
                    break;
                case "commands":
                    log(msgs.Commands);
                    break;
                case "rules":
                    log(msgs.Rules);
                    break;
                case "change":
                    log(username + msgs.Ping + ChangeRequest(username, args));
                    break;
                case "currentsong":
                    log(GetCurrentSong());
                    break;
                case "about":
                    log(msgs.About);
                    break;
                // Moderator restricted functions
                case "next":
                    if (mods.Contains(username))
                    {
                        log(Next());
                        break;
                    }
                    else
                    {
                        log(username + msgs.Ping + msgs.ModsOnly);
                        break;
                    }
                case "remove":
                    if (mods.Contains(username))
                    {
                        log("(Mod Removal)" + args + msgs.Ping + RemoveSong(args));
                        break;
                    }
                    else
                    {
                        log(username + msgs.Ping + msgs.ModsOnly);
                        break;
                    }
                default:
                    log(username + msgs.Ping + msgs.UnknownCommand);
                    break;
            }

        }
        //returns true if song is succesfully added, returns false when user is already in queue
        public string AddSong(SongRequest song)
        {
            foreach (SongRequest item in _list)
            {
                if(item.Username==song.Username)
                {
                    return "Error: you are already on the list. If you want to change your request, !change it.";
                }
            }

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                _list.Add(song);
            }));

            return "Request successfully added. Your current spot in the list is " + (_list.Count-1);
        }
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

            /* Retired due to Twitch's 500 character limit per character. Likely will be used in web implementation though, so it's being kept around
            int count = -1;
            string contents = "The _list is ";
            foreach (SongRequest song in _list)
            {
                count++;
                if(count==0)
                {
                    contents += "Currently up: " + song.summary + " ";
                }
                else
                {
                    contents += count + ". " + song.summary + " ";
                }
            }
            return contents;
            */
        }
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

        public string ChangeRequest(string username, string args)
        {
            if (args == "")
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
            _list[index].Url = args;
            _list[index].GenerateSummary();
            }));
            return "Your request has been sucessfully updated";
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void log(string msg, bool chat = true)
        {
            if (chat)
            {
                client.SendMessage(msg);
            }
            _logContent += (msg + "\n");
            NotifyPropertyChanged("LogContent");
        }
    }
}
