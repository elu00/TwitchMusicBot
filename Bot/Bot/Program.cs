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

            //Make sure config is not null

            //Initialize List
            SongList songs = new SongList();
            //Connect to twitch IRC channel
            TwitchClient client = new TwitchClient(new ConnectionCredentials(username, oauth), channel);

            //Listen for commands
            client.OnChatCommandReceived += new EventHandler<TwitchClient.OnChatCommandReceivedArgs>(chatCommandReceived);
            
        }
        //Command implementation
        private static void chatCommandReceived(object sender, TwitchClient.OnChatCommandReceivedArgs e)
        {
            string command = e.Command.Command;
            string user = e.Command.ChatMessage.Username;
            string url = e.Command.ArgumentsAsString;
            SongRequest request = new SongRequest(user, url);


        }
    }
}
