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
        bool IsPaused
        {
            get => mediaElements.Count > 0 ? mediaElements[0].LoadedBehavior == MediaState.Pause : true;
            set
            {
                mediaElements.ForEach(element => element.LoadedBehavior = value ? MediaState.Pause : MediaState.Play);
                PlayButton.Content = !value ? "\uE769" : "\uE768";
            }
        }
        double TimeStamp
        {
            get => mediaElements.Count > 0 ? mediaElements[0].Position.TotalSeconds : 0;
            set
            {
                if (Math.Abs(TimeStamp - value) > 0.5)
                {
                    Debug.Print((Math.Abs(TimeStamp - value) > 0.5).ToString());
                    mediaElements.ForEach(element => element.Position = TimeSpan.FromSeconds(value));
                }
            }
        }
        double PlaySpeed
        {
            get => mediaElements.Count > 0 ? mediaElements[0].SpeedRatio : 0;
            set
            {
                mediaElements.ForEach(element => element.SpeedRatio = value);
                HackySpeedSlider.Value = value;
            }
        }
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
                if (serverConnection != null)
                {
                    serverConnection.MessageReceivedEvent += ServerConnection_MessageReceivedEvent;
                    serverConnection.SendRequest(MediaCommand.GetUpdate());
                }
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
            HackyTimeSlider.Maximum = element.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void UpdateAllVideos(MediaResult result)
        {
            if (result.paused != null)
            {
                IsPaused = result.paused.GetValueOrDefault();
            }
            if (result.timeStamp != null)
            {
                TimeStamp = result.timeStamp.GetValueOrDefault();
            }
            if (result.playSpeed != null)
            {
                PlaySpeed = result.playSpeed.GetValueOrDefault();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    TogglePause();
                    break;
                case Key.Left:
                    TimeStamp -= 5;
                    ServerConnection?.SendRequest(MediaCommand.SetTimeStamp(TimeStamp));
                    break;
                case Key.Right:
                    TimeStamp += 5;
                    ServerConnection?.SendRequest(MediaCommand.SetTimeStamp(TimeStamp));
                    break;
            }
        }

        

        private void timer_Tick(object sender, EventArgs e)
        {
            if (mediaElements.Count > 0)
            {
                MediaElement element = mediaElements[0];
                if (element.NaturalDuration.HasTimeSpan && element.NaturalDuration.TimeSpan.TotalSeconds > 0 && !(Mouse.LeftButton == MouseButtonState.Pressed))
                {
                    if (totalTime.TotalSeconds > 0)
                    {
                        HackyTimeSlider.Value = element.Position.TotalSeconds;
                    }
                }
            }
           
        }

        private void TimeSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (totalTime.TotalSeconds > 0)
            {
                TimeStamp = HackyTimeSlider.Value;
                ServerConnection?.SendRequest(MediaCommand.SetTimeStamp(TimeStamp));
            }
        }

        private void VideoGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TogglePause();
        }

        private void TogglePause()
        {
            IsPaused = !IsPaused;
            ServerConnection?.SendRequest(MediaCommand.Pause(IsPaused));
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            TimeStamp -= 10;
            ServerConnection?.SendRequest(MediaCommand.SetTimeStamp(TimeStamp));
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            TimeStamp += 30;
            ServerConnection?.SendRequest(MediaCommand.SetTimeStamp(TimeStamp));
        }

       
        private void HackySpeedSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PlaySpeed = HackySpeedSlider.Value;
            ServerConnection?.SendRequest(MediaCommand.SetPlaySpeed(PlaySpeed));
        }
    }
}
