using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoomVisualizer
{
    /// <summary>
    /// Interaction logic for DeviceRotationControl.xaml
    /// </summary>
    public partial class DeviceRotationControl : UserControl
    {
        private Boolean _started = false;

        public event EventHandler<RotationSliderEventArgs> onSliderValueChanged;

        public DeviceRotationControl()
        {
            InitializeComponent();

            slabel.FontSize = 20;
            slabel.Content = "270°";
            this.Opacity = 0;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_started)
            {
                slabel.Content = ((int)rotationSlider.Value).ToString() + "°";
                onSliderValueChanged(this, new RotationSliderEventArgs((int)rotationSlider.Value));
            }
        }

        private void timeSlider_Initialized(object sender, EventArgs e)
        {
            _started = true;
        }

        private void Viewport3D_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Opacity = 1;
        }

        private void Viewport3D_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Opacity = 0;
        }
    }

    public class RotationSliderEventArgs : EventArgs
    {
        private int _time;

        public int Time
        {
            get { return _time; }
            set { _time = value; }
        }

        public RotationSliderEventArgs(int t)
        {
            _time = t;
        }
    }
}
