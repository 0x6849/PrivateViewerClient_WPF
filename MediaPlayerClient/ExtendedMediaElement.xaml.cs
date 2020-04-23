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

namespace MediaPlayerClient
{
    /// <summary>
    /// Interaktionslogik für ExtendedMediaElement.xaml
    /// </summary>
    public partial class ExtendedMediaElement : UserControl
    {
        public double TimeStamp
        {
            get => mediaElement.Position.TotalSeconds;
            set
            {
                mediaElement.Position = TimeSpan.FromSeconds(value);
            }
        }

        public bool paused


        public ExtendedMediaElement()
        {
            InitializeComponent();
        }

        public void SetMedia(string path, float timeStamp)
        {
            mediaElement.Source = new Uri(path);
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
