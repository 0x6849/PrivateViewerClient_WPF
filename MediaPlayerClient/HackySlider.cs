using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaPlayerClient
{ 
    public class HackySlider : Panel
    {
        private LinearGradientBrush brush;
        private GradientStop startStop, endStop;

        public double Maximum
        {
            get => maximum;
            set
            {
                maximum = value;
                Value = Value;
            }
        }
        private double maximum = 1;

        public double Value
        {
            get => val;
            set
            {
                val = Math.Min(Math.Max(value, 0), Maximum);
                startStop.Offset = value / maximum;
                endStop.Offset = value / maximum;
            }
        }
        private double val;

        private double InternalValue
        {
            get => Value;
            set
            {
                Value = value;
                ValueChanged?.Invoke(this, value);
            }
        }

        public delegate void ValueChangedHandler(HackySlider sender, double newValue);

        public event ValueChangedHandler ValueChanged;

        public HackySlider()
        {
            brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.Blue, 0));
            startStop = new GradientStop(Colors.Blue, 0);
            endStop = new GradientStop(Colors.Gray, 0);
            brush.GradientStops.Add(startStop);
            brush.GradientStops.Add(endStop);
            brush.GradientStops.Add(new GradientStop(Colors.Gray, 1));
            Background = brush;
            MouseMove += HackySlider_MouseMove;
            MouseDown += HackySlider_MouseDown;
            MouseUp += HackySlider_MouseUp;
        }

        private void HackySlider_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
        }

        private void HackySlider_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CaptureMouse();
            HackySlider_MouseMove(sender, e);
        }

        private void HackySlider_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                InternalValue = e.GetPosition(this).X / this.ActualWidth * Maximum;
            }
        }
    }
}
