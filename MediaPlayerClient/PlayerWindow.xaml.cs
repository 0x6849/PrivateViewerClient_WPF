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
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

namespace MediaPlayerClient
{
    /// <summary>
    /// Interaktionslogik für PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
    {
        private List<MediaElement> mediaElements = new List<MediaElement>();
        private bool isPlaying = true;
        private TimeSpan totalTime;
        private DispatcherTimer updateSliderTimer;
        public ServerConnection ServerConnection
        {
            get => serverConnection;
            set
            {
                if (serverConnection != null)
                {
                    serverConnection.MessageReceivedEvent -= ServerConnection_MessageReceivedEvent;
                }
                serverConnection = value;
                serverConnection.MessageReceivedEvent += ServerConnection_MessageReceivedEvent;
            }
        }
        private ServerConnection serverConnection;

        public PlayerWindow(string videoPath)
        {
            InitializeComponent();
            AddVideoInternal(videoPath);
            updateSliderTimer = new DispatcherTimer();
            updateSliderTimer.Interval = TimeSpan.FromSeconds(1);
            updateSliderTimer.Tick += timer_Tick;
            updateSliderTimer.Start();
        }

        private void ServerConnection_MessageReceivedEvent(MediaResult obj)
        {
            if (obj.action != null)
            {
                switch (obj.action)
                {
                    case Action.change:
                        UpdateAllVideos(obj);
                        break;
                }
            }
        }

        public void AddVideo(string videoPath)
        {
            GridSplitter gridSplitter = new GridSplitter();
            VideoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10) });
            gridSplitter.SetValue(Grid.ColumnProperty, VideoGrid.ColumnDefinitions.Count - 1);
            gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            VideoGrid.Children.Add(gridSplitter);
            AddVideoInternal(videoPath);
        }

        private void AddVideoInternal(string videoPath)
        {
            VideoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            MediaElement element = new MediaElement();
            element.Source = new Uri(videoPath);
            element.ScrubbingEnabled = true;
            element.SetValue(Grid.ColumnProperty, VideoGrid.ColumnDefinitions.Count - 1);
            VideoGrid.Children.Add(element);
            mediaElements.Add(element);
            if (mediaElements.Count > 1)
            {
                element.Position = mediaElements[0].Position;
            }
            else
            {
                element.MediaOpened += (sender, e) => SetPrimaryVideo(element);
            }
        }

        private void SetPrimaryVideo(MediaElement element)
        {
            totalTime = element.NaturalDuration.TimeSpan;
            TimeSlider.Maximum = element.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void UpdateAllVideos(MediaResult result)
        {
            mediaElements.ForEach(element =>
            {
                if (result.paused != null)
                {
                    
                    element.LoadedBehavior = result.paused.GetValueOrDefault() ? MediaState.Pause : MediaState.Play;
                }
                if (result.timeStamp != null)
                {
                    element.Position = TimeSpan.FromSeconds(result.timeStamp.GetValueOrDefault());
                }
                if (result.playSpeed != null)
                {
                    element.SpeedRatio = result.playSpeed.GetValueOrDefault();
                }
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        

        private void timer_Tick(object sender, EventArgs e)
        {
            // Check if the movie finished calculate it's total time
            if (mediaElements.Count > 0)
            {
                MediaElement element = mediaElements[0];
                if (element.NaturalDuration.HasTimeSpan && element.NaturalDuration.TimeSpan.TotalSeconds > 0 && !(Mouse.LeftButton == MouseButtonState.Pressed))
                {
                    if (totalTime.TotalSeconds > 0)
                    {
                        // Updating time slider
                        TimeSlider.Value = element.Position.TotalSeconds;
                    }
                }
            }
           
        }

        private void TimeSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
          
            if (totalTime.TotalSeconds > 0)
            {
                mediaElements.ForEach(element => element.Position = TimeSpan.FromSeconds(TimeSlider.Value));
            }
        }
    }
}
