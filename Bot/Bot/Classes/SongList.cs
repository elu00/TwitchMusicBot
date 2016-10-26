using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Classes
{
    class SongList
    {
        private List<SongRequest> list = new List<SongRequest>();
        //returns true if song is succesfully added, returns false when user is already in queue
        public string AddSong(SongRequest song)
        {
            foreach (SongRequest item in list)
            {
                if(item.username==song.username)
                {
                    return "Error: you are already on the list. If you want to change your request, !change it.";
                }
            }
            list.Add(song);
            return "Request successfully added. Your current spot in the list is " + (list.Count-1);
        }
        public string RemoveSong(string username)
        {
            foreach (SongRequest item in list)
            {
                if (item.username == username)
                {
                    list.RemoveAt(list.IndexOf(item));
                    return "Your request has been removed from the list";
                }
            }
            return "Error: you are not currently on the list";
        }
        public string Next()
        {
            /* Will be used to implement historical data/analysis to a corresponding text file
            StreamReader rec = new StreamReader("history.txt");
            rec.WriteLine(list[0].url)
            */
            if (list[0] == null)
            {
                return "The list is currently empty";
            }
            else
            {
                list.RemoveAt(0);
                if(list[0] == null)
                {
                    return "That's the end of the list!";
                }
                return "Next song: " + list[0].summary;
            }
        }
        public string GetList()
        {
            if(list.Count == 0)
            {
                return "The list is currently empty";
            }
            string contents = "The first 5 people in the list are: (Currently Up) ";
            for (int i = 0; i < 4; i++)
            {
                contents += list[i].username + ", ";

            }
            return contents;

            /* Retired due to Twitch's 500 character limit per character. Likely will be used in web implementation though, so it's being kept around
            int count = -1;
            string contents = "The list is ";
            foreach (SongRequest song in list)
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
			foreach (SongRequest user in list)
			{
                //iterate through list and return index is user is inside
                if(user.username == username)
                {
                    return list.IndexOf(user);
                }
			}
            return -1;
        }
        public string GetCurrentSong()
        {
            if (list[0] == null)
            {
                return "The list is currently empty";
            }
            else
            {
                return "The current song is "+ list[0].summary;
            }
        }

        public string ChangeRequest(string username, string args)
        {
            if (args == "")
            {
                return "Please specify the new URL of your request";
            }
            int index = -1;
            foreach (SongRequest user in list)
            {
                //iterate through list and return index is user is inside
                if (user.username == username)
                {
                    index = list.IndexOf(user);
                }
            }
            list[index].url = args;
            list[index].GenerateSummary();
            return "Your request has been sucessfully updated";
        }

    }
}
