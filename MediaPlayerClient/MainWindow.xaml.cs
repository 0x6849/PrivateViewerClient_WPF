using System;
using System.Collections.Generic;
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
using System.IO;
using System.Xml;
using System.Diagnostics;
using Microsoft.Win32;

namespace MediaPlayerClient
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string configFile = "config.xml";

        private PlayerWindow player = null;
        private ServerConnection serverConnection;

        private string serverUrl = "";
        private string roomName = "";

        public MainWindow()
        {
            InitializeComponent();
            Load();
            RoomTextBox.Text = roomName;
            ServerUrlTextBox.Text = serverUrl;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                serverConnection = new ServerConnection(ServerUrlTextBox.Text, RoomTextBox.Text);
                serverConnection.MessageReceivedEvent += ServerConnection_MessageReceivedEvent;
                await serverConnection.Connect();
                if (player != null)
                {
                    player.ServerConnection = serverConnection;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ServerConnection_MessageReceivedEvent(MediaResult obj)
        {
            //TODO
        }

        private void AddVideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (player == null)
            {
                player = new PlayerWindow(VideoPathTextBox.Text);
                player.ServerConnection = serverConnection;
                player.Closed += (s, e2) => player = null;
                player.Show();
            }
            else
            {
                player.AddVideo(VideoPathTextBox.Text);
            }
        }

        private void Load()
        {
            if (File.Exists(configFile))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(File.ReadAllText(configFile));
                if (doc.DocumentElement.HasAttribute(nameof(serverUrl)))
                {
                    serverUrl = doc.DocumentElement.GetAttribute(nameof(serverUrl));
                }
                if (doc.DocumentElement.HasAttribute(nameof(roomName)))
                {
                    roomName = doc.DocumentElement.GetAttribute(nameof(roomName));
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(configFile, settings))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("root");

                writer.WriteAttributeString(nameof(serverUrl), ServerUrlTextBox.Text);
                writer.WriteAttributeString(nameof(roomName), RoomTextBox.Text);

                writer.WriteEndElement();

                writer.WriteEndDocument();

            }
        }

        private void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "video files (*.mp4)|*.mp4|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                VideoPathTextBox.Text = openFileDialog.FileName;
            }
        }
    }
}
