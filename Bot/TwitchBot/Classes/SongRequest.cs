using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Classes
{
    class SongRequest
    {
        public string username;
        public string url;
       // public string name;
        public DateTime time;
        public string summary;

        public SongRequest(string user, string url)
        {
            username = user;
            this.url = url;
            time = DateTime.Now;
            //name = Youtube.Parse.Name(url);
            GenerateSummary();
        }
        public void GenerateSummary()
        {
            summary = url + ", Requested By: " + username;
        }
    }
}
