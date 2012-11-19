using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSEKinect;
using MSELocator;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace RoomVisualizer
{
    /// <summary>
    /// Represents a device in the RoomVisualiser's canvas, gathers together several shapes used to represent it. 
    /// </summary>
    class DrawnDevice
    {
        Ellipse deviceDot;
        Label deviceLabel;


        public DrawnDevice(PairableDevice pairableDevice)
        {
            pairableDevice.LocationChanged += onLocationChanged;
            pairableDevice.OrientationChanged += onOrientationChanged;
            pairableDevice.PairingStateChanged += onPairingStateChanged;



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
