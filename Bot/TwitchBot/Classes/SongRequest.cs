using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Classes
{
    class SongRequest : INotifyPropertyChanged
    {
        private string _username;
        private string _url;
        private DateTime _time;
        public string _summary;

        public string Username
        {
            get
            {
                return _username;
            }
            protected set
            {
                _username = value;
            }
        }
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                if (_url == value)
                {
                    return;
                }
                else
                {
                    _url = value;
                    NotifyPropertyChanged("Url");
                }
            }
        }
        public DateTime Time
        {
            get
            {
                return _time;
            }
            protected set
            {
                _time = value;
            }
        }
        public string Summary
        {
            get
            {
                return _summary;
            }
            protected set
            {
                _summary = value;
            }
        }

        // public string name;
        public SongRequest(string user, string url)
        {
            _username = user;
            _url = url;
            _time = DateTime.Now;
            //name = Youtube.Parse.Name(url);
            GenerateSummary();
        }
        public void GenerateSummary()
        {
            _summary = Url + ", Requested By: " + Username;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
