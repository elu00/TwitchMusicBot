using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    class SongRequest
    {
        public string username;
        public string url;
        public string name;
        public string time;
        public string summary;

        public SongRequest(string user, string url)
        {
            this.username = user;
            this.url = url;
            time = DateTime.Now.ToString();
            name = Youtube.Parse.Name(url);
            summary = username;
        }
    }
}
