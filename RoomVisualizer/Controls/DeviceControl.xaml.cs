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
        private enum DisplayState
        {
            UnpairedAndOnStackPanel,
            PairedAndOnCanvas,
        }

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
                    MainWindow.SharedCanvas.Children.Add(this);


                    double deviceSize = 0.5 * MainWindow.SharedCanvas.ActualWidth / DrawingResources.ROOM_WIDTH;
                    this.Width = Math.Ceiling(deviceSize);
                    this.Height = Math.Ceiling(deviceSize);

                    InnerBorder.Margin = new Thickness(0);

                }
                else if (value == DisplayState.UnpairedAndOnStackPanel && myDisplayState == DisplayState.PairedAndOnCanvas)
                {
                    //Handle transition to display on StackPanel
                    MainWindow.SharedCanvas.Children.Remove(this);
                    MainWindow.SharedDeviceStackPanel.Children.Add(this);

                    this.Width = 150;
                    this.Height = 150;

                    Canvas.SetTop(this, 0);
                    Canvas.SetLeft(this, 0);
                    InnerBorder.Margin = new Thickness(25);


                }

                myDisplayState = value;
            }
        }

        public DeviceControl(PairableDevice pairableDevice)
        {
            InitializeComponent();

            //Setup Events
            pairableDevice.LocationChanged += onLocationChanged;
            pairableDevice.OrientationChanged += onOrientationChanged;
            pairableDevice.PairingStateChanged += onPairingStateChanged;

            //Setup Display
            DeviceNameLabel.Content = pairableDevice.Identifier;
            InnerBorder.BorderBrush = DrawingResources.unpairedBrush;
            MyDisplayState = DisplayState.UnpairedAndOnStackPanel;

        }

        public void onOrientationChanged(Device device)
        {


        }

        public void onLocationChanged(Device device)
        {
            PairableDevice pairableDevice = (PairableDevice)device;
            if (pairableDevice.PairingState == PairingState.Paired)
            {
                if (pairableDevice.Location.HasValue)
                {
                    Point newPoint = DrawingResources.ConvertFromMetersToDisplayCoordinates(pairableDevice.Location.Value, MainWindow.SharedCanvas);
                    Canvas.SetLeft(this, newPoint.X);
                    Canvas.SetTop(this, newPoint.Y);
                }
            }
            else 
            {
 
            }

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
