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
                Console.WriteLine("No valid configuration text file found");
                //Exit procedure
            }
            StreamReader config = new StreamReader("config.txt");
            string username = config.ReadLine();
            string oauth = config.ReadLine();
            string channel = config.ReadLine();

            //Make sure parameters are not null

            //Initialize List
            SongList songs = new SongList();
            //Connect to twitch IRC channel
            TwitchClient client = new TwitchClient(new ConnectionCredentials(username, oauth), channel, '!', '!');

            //Listen for commands
            client.OnChatCommandReceived += (sender, e) => chatCommandReceived(sender, e, songs, client);

        }
        //Command implementation
        private static void chatCommandReceived(object sender, TwitchClient.OnChatCommandReceivedArgs e, SongList songs, TwitchClient client)
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
    }
}
