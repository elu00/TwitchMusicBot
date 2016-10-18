using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    class SongRequest
    {
        private string url;
        private string user;
        public SongRequest(string user, string url)
        {
            this.user = user;
            this.url = url;
        }
    }
}
