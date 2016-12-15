using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwitchBot.Classes;
using TwitchLib;
using TwitchLib.Models.Client;

namespace TwitchBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Initialize List and Mods
        SongList songs;

        public MainWindow()
        {
            InitializeComponent();
            songs = new SongList();
            //Parse original configuration from config.txt
            if (File.Exists("config.txt"))
            {
                //Initialization
                StreamReader config = new StreamReader("config.txt");
                initUsername.Text = config.ReadLine();
                initOAuth.Text = config.ReadLine();
                initConnection.Text = config.ReadLine();
                songs.log("Configuration loaded", false);
            }
            //Setup Databinding
            SongGrid.DataContext = songs;
            LogBox.DataContext = songs;
        }
        
        private void disableInitTextBoxes()
        {
            initUsername.IsEnabled = false;
            initOAuth.IsEnabled = false;
            initConnection.IsEnabled = false;
            btnInit.IsEnabled = false;
        }
        //Event Handling
        private void Initialize_Click(object sender, RoutedEventArgs e)
        {
            disableInitTextBoxes();
            initText.Text = "Connecting....";
            songs.initialize(initUsername.Text, initOAuth.Text, initConnection.Text);
            initText.Text = "Connected!";
            NextButton.IsEnabled = true;
        }
        private void NextSong(object sender, RoutedEventArgs e)
        {
            songs.log("Owner" + msgs.Ping + songs.Next());
        }
    }
}
