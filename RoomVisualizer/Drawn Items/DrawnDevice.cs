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
    class DrawnDevice : Grid
    {
        Rectangle deviceSquare;
        Label deviceLabel;


        public DrawnDevice(PairableDevice pairableDevice) : base()
        {
            //Setup Events
            pairableDevice.LocationChanged += onLocationChanged;
            pairableDevice.OrientationChanged += onOrientationChanged;
            pairableDevice.PairingStateChanged += onPairingStateChanged;



            //Add Device to Stack Panel (of connected devices)
            deviceSquare = new Rectangle();
            deviceSquare.Width = DrawingResources.SQUARE_LENGTH;
            deviceSquare.Height = DrawingResources.SQUARE_LENGTH;
            deviceSquare.Stroke = DrawingResources.unpairedBrush;
            deviceSquare.StrokeThickness = DrawingResources.STROKE_WIDTH;
            //this.Children.Add(deviceSquare);

            deviceLabel = new Label();
            deviceLabel.Content = pairableDevice.Identifier;
            //this.Children.Add(deviceLabel);

            //MainWindow.SharedDeviceStackPanel.Children.Add(this);
            //MainWindow.SharedDeviceStackPanel.Children.Add(deviceSquare);


            DeviceControl deviceControl = new DeviceControl(pairableDevice);
            deviceControl.DeviceNameLabel.Content = pairableDevice.Identifier;
            MainWindow.SharedDeviceStackPanel.Children.Add(deviceControl);

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
