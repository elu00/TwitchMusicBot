using System;
using System.Collections.Generic;
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
                    list.IndexOf(item);
                    return true;
                }
            }
            return true;
        }
        public string GetList()
        {
            string contents = "The list is";
            foreach (SongRequest song in list)
            {
                contents += (song.name + "requested by" + song.username);
                //implementing later
            }
            return contents;
        }
        public int GetSpot(string username)
        {
			//implementing later
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
