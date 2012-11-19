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

using MSEKinect;
using MSELocator;

namespace RoomVisualizer
{
    /// <summary>
    /// Interaction logic for DeviceControl.xaml
    /// </summary>
    public partial class DeviceControl : UserControl
    {
        public DeviceControl(PairableDevice pairableDevice)
        {
            InitializeComponent();

            //Setup Events
            pairableDevice.LocationChanged += onLocationChanged;
            pairableDevice.OrientationChanged += onOrientationChanged;
            pairableDevice.PairingStateChanged += onPairingStateChanged;

            DeviceNameLabel.Content = pairableDevice.Identifier;
            InnerBorder.BorderBrush = DrawingResources.unpairedBrush;

        }

        public void onOrientationChanged(Device device)
        {


        }

        public void onLocationChanged(Device device)
        {

        }

        public void onPairingStateChanged(PairableDevice pairableDevice)
        {


        }
    }
}
