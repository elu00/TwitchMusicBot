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
        public bool AddSong(SongRequest song)
        {
            foreach (SongRequest item in list)
            {
                if(item.username==song.username)
                {
                    return false;
                }
            }
            list.Add(song);
            return true;
        }
        public bool RemoveSong(string username)
        {
            foreach (SongRequest item in list)
            {
                if (item.username == username)
                {
                    list.RemoveAt(list.IndexOf(item));
                    return true;
                }
            }
            return false;
        }
        public string Next()
        {
            /* Will be used to implement historical data/analysis to a corresponding text file
            StreamReader rec = new StreamReader("history.txt");
            rec.WriteLine(list[0].url)
            */
            list.RemoveAt(0);
            return "Next song:" + list[0].summary;
        }
        public string GetList()
        {
            int count = 1;
            string contents = "The list is";
            foreach (SongRequest song in list)
            {
                count++;
                contents += (count + "." + song.summary);
            }
            return contents;
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

    }
}
