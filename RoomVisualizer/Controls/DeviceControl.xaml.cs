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
using MSEAPI_SharedNetworking;
using IntAirAct;

namespace RoomVisualizer
{
    /// <summary>
    /// Interaction logic for DeviceControl.xaml
    /// </summary>
    public partial class DeviceControl : UserControl
    {
        private enum DisplayState
        {
            UnpairedAndOnStackPanel,
            PairedAndOnCanvas,
            UnlocatedAndOnStackPanel,
            LocatedAndOnStackPanel
        }

        private IADevice iaDevice;

        // DeviceControl can be displayed on the room visualizer canvas, or the stack panel of unpaired devices.
        private DisplayState myDisplayState;
        private DisplayState MyDisplayState
        {
            get
            {
                return myDisplayState;
            }
            set
            {
                if (value == DisplayState.PairedAndOnCanvas && myDisplayState == DisplayState.UnpairedAndOnStackPanel)
                {
                    //Handle transition to display on Canvas
                    MainWindow.SharedDeviceStackPanel.Children.Remove(this);
                    formatForCanvas();
                    MainWindow.SharedCanvas.Children.Add(this);

                }
                else if (value == DisplayState.UnpairedAndOnStackPanel && myDisplayState == DisplayState.PairedAndOnCanvas)
                {
                    //Handle transition to display on StackPanel
                    MainWindow.SharedCanvas.Children.Remove(this);
                    formatForStackPanel();
                    MainWindow.SharedDeviceStackPanel.Children.Add(this);
                }
                else if (value == DisplayState.UnlocatedAndOnStackPanel && myDisplayState == DisplayState.LocatedAndOnStackPanel)
                {
                    //Handle transition to display on StackPanel
                    MainWindow.SharedCanvas.Children.Remove(this);
                    formatForStackPanel();
                    MainWindow.SurfaceWrapPanel.Children.Add(this);

                }
                else if (value == DisplayState.LocatedAndOnStackPanel && myDisplayState == DisplayState.UnlocatedAndOnStackPanel)
                {
                    //Handle transition to display on Canvas
                    MainWindow.SurfaceWrapPanel.Children.Remove(this);
                    formatForCanvas();
                    MainWindow.SharedCanvas.Children.Add(this);
                }

                myDisplayState = value;
            }
        }

        public void formatForCanvas()
        {
            double deviceSize = 0.5 * MainWindow.SharedCanvas.ActualWidth / DrawingResources.ROOM_WIDTH;
            InnerBorder.Width = Math.Ceiling(deviceSize);
            InnerBorder.Height = Math.Ceiling(deviceSize);

            InnerBorder.BorderBrush = DrawingResources.pairedBrush;

            Canvas.SetLeft(DeviceNameLabel, -35);
            Canvas.SetTop(DeviceNameLabel, -65);

            DeviceNameLabel.Width = 200;
            DeviceNameLabel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            DeviceNameLabel.Margin = new Thickness(0, 25, 0, 0);
            DeviceNameLabel.FontSize = 22;
            InnerBorder.Margin = new Thickness(0);

            Canvas.SetLeft(LeftLine, InnerBorder.Width / 2);
            Canvas.SetTop(LeftLine, InnerBorder.Height / 2);

            Canvas.SetLeft(RightLine, InnerBorder.Width / 2);
            Canvas.SetTop(RightLine, InnerBorder.Height / 2);

        }

        public void formatForStackPanel()
        {
            InnerBorder.Width = 64;
            InnerBorder.Height = 64;

            DeviceNameLabel.Width = this.Width;
            DeviceNameLabel.Margin = new Thickness(0);

            Canvas.SetTop(DeviceNameLabel, 70);
            Canvas.SetLeft(DeviceNameLabel, 0);

            DeviceNameLabel.FontSize = 12;
            InnerBorder.Margin = new Thickness(18,0,0,0);

            LeftLine.Visibility = System.Windows.Visibility.Hidden;
            RightLine.Visibility = System.Windows.Visibility.Hidden;
        }

        public DeviceControl(PairableDevice pairableDevice, IADevice iaDevice)
        {
            InitializeComponent();

            this.iaDevice = iaDevice;

            //Setup Events
            pairableDevice.LocationChanged += onLocationChanged;
            pairableDevice.OrientationChanged += onOrientationChanged;
            pairableDevice.PairingStateChanged += onPairingStateChanged;

            LeftLine.StrokeThickness = DrawingResources.DEVICE_FOV_WIDTH;
            RightLine.StrokeThickness = DrawingResources.DEVICE_FOV_WIDTH;

            //Setup Display
            DeviceNameLabel.Text = pairableDevice.Identifier;
            InnerBorder.BorderBrush = DrawingResources.unpairedBrush;

            // If it supports this route, then we know it's a surface
            if (iaDevice.SupportedRoutes.Contains(Routes.GetLocationRoute))
            {
                MyDisplayState = DisplayState.UnlocatedAndOnStackPanel;
            }
            // Likewise, if it supports this route, we know it's a pairable device
            else if (iaDevice.SupportedRoutes.Contains(Routes.BecomePairedRoute))
            {
                MyDisplayState = DisplayState.UnpairedAndOnStackPanel;
            }

            formatForStackPanel();

        }

        public void onOrientationChanged(Device device)
        {
            PairableDevice pairableDevice = (PairableDevice)device;
            // Draw two lines to serve as field of view indicators
            double topAngle = Util.NormalizeAngle(pairableDevice.Orientation.Value + pairableDevice.FieldOfView.Value);
            double topX = Math.Cos(topAngle * Math.PI / 180);
            double topY = Math.Sin(topAngle * Math.PI / 180);


            double bottomAngle = Util.NormalizeAngle(pairableDevice.Orientation.Value - pairableDevice.FieldOfView.Value);
            double bottomX = Math.Cos(bottomAngle * Math.PI / 180);
            double bottomY = Math.Sin(bottomAngle * Math.PI / 180);

            Point newLeft = DrawingResources.ConvertPointToProperLength(new Point(topX, topY), DrawingResources.DEVICE_FOV_LENGTH);
            Point newRight = DrawingResources.ConvertPointToProperLength(new Point(bottomX, bottomY), DrawingResources.DEVICE_FOV_LENGTH);

            //Dispatch UI Changes to Main Thread
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LeftLine.X2 = newLeft.X;
                LeftLine.Y2 = -newLeft.Y;

                RightLine.X2 = newRight.X;
                RightLine.Y2 = -newRight.Y;

                if (pairableDevice.PairingState == PairingState.Paired)
                {
                    LeftLine.Visibility = System.Windows.Visibility.Visible;
                    RightLine.Visibility = System.Windows.Visibility.Visible;
                }

            }));

        }

        public void onLocationChanged(Device device)
        {
            PairableDevice pairableDevice = (PairableDevice)device;

            //Dispatch UI Changes to Main Thread
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (pairableDevice.Location.HasValue)
                {
                    if (iaDevice.SupportedRoutes.Contains(Routes.GetLocationRoute))
                    {
                        MyDisplayState = DisplayState.LocatedAndOnStackPanel;
                    }

                    Point newPoint = DrawingResources.ConvertFromMetersToDisplayCoordinates(pairableDevice.Location.Value, MainWindow.SharedCanvas);

                    // InnerBorder.Width / 2 is to make it so that the point that the DeviceControl is drawn at is actually the center of the Border
                    Canvas.SetLeft(this, newPoint.X - (InnerBorder.Width / 2));
                    Canvas.SetTop(this, newPoint.Y - (InnerBorder.Height / 2));
                }
            }));

        }

        public void onPairingStateChanged(PairableDevice pairableDevice)
        {
            //Dispatch UI Changes to Main Thread
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //Set device border to appropriate colour
                InnerBorder.BorderBrush = DrawingResources.GetBrushFromPairingState(pairableDevice.PairingState);

                //Set the control's owner
                if (pairableDevice.PairingState == PairingState.Paired)
                {
                    // When paired, we move the device to the canvas.
                    this.MyDisplayState = DisplayState.PairedAndOnCanvas;
                }
                else 
                {   
                    // If we are not paired or in pairing attempt, we go to stackpanel
                    this.MyDisplayState = DisplayState.UnpairedAndOnStackPanel;
                }

            }));

        }
    }
}
