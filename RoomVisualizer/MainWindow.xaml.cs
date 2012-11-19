﻿using System;
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
using System.Timers;
using System.Windows.Threading;

using MSEKinect;
using MSELocator;

namespace RoomVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Shared accessor for the window's canvas object.
        /// </summary>
        public static Canvas SharedCanvas
        {
            get { return sharedCanvas; }
        }
        private static Canvas sharedCanvas;


        public static StackPanel SharedDeviceStackPanel
        {
            get { return sharedStackPanel; }
        }
        private static StackPanel sharedStackPanel;

        #region Instance Variables
        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        MSEKinectManager kinectManager;
        //DispatcherTimer dispatchTimer;

        /// <summary>
        /// Rendering code from the SkeletonBasics example, for demonstration purposes 
        /// </summary>
        private SkeletonRenderer skeletonRenderer;

        private Dictionary<string, PersonControl> PersonControlDictionary;
        private Dictionary<string, DeviceControl> DeviceControlDictionary;
        private DrawnTracker drawnTracker;

        #endregion

        #region constants

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 640.0f;

        const double xRange = 4.5;
        const double yRange = 4.5;

        const double deviceDrawWidth = 0.25 * RenderWidth / xRange;
        const double deviceDrawHeight = 0.25 * RenderHeight / yRange;

        const double trackerDrawWidth = 0.10 * RenderWidth / xRange;
        const double trackerDrawHeight = 0.10 * RenderHeight / yRange;

        const int FPS = 60;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            sharedCanvas = canvas;
            sharedStackPanel = unpairedDeviceStackPanel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Create Dictionaries for DeviceControl, PersonControl
            DeviceControlDictionary = new Dictionary<string, DeviceControl>();
            PersonControlDictionary = new Dictionary<string, PersonControl>();


            //Initialize and Start MSEKinectManager
            kinectManager = new MSEKinectManager();
            kinectManager.Start();

            // The tracker is created in the PersonManager constructor, so there's actually no way for us to listen for its creation the first time
            trackerChanged(kinectManager.PersonManager, kinectManager.PersonManager.Tracker);

            //Setup Events for Device Addition and Removal, Person Addition and Removal 
            kinectManager.DeviceManager.DeviceAdded += deviceAdded;
            kinectManager.DeviceManager.DeviceRemoved += deviceRemoved;
            kinectManager.PersonManager.PersonAdded += personAdded;
            kinectManager.PersonManager.PersonRemoved += personRemoved;

            //Seperate components for displaying the visible skeletons
            skeletonRenderer = new SkeletonRenderer(SkeletonBasicsImage);

            //Hardcode tracker position and orientation
            Tracker tracker = kinectManager.Locator.Trackers[0];
            tracker.Location = new Point(xRange / 2, yRange); 
            tracker.Orientation = 270; 



        }

        //Window Close (End the Kinect Manager) 
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kinectManager.Stop();
        }
    
        //Utility Function - Coverting from Meters into Pixels
        private Point ConvertFromMetersToPixels(Point myPoint)
        {
            return new Point(myPoint.X * RenderWidth / xRange, RenderHeight - (myPoint.Y * RenderHeight / yRange));
        }


        #region Handlers for Person and Device manager events
        
        void deviceAdded(DeviceManager deviceManager, PairableDevice pairableDevice)
        {
            DeviceControlDictionary[pairableDevice.Identifier] = new DeviceControl(pairableDevice);
            unpairedDeviceStackPanel.Children.Add(DeviceControlDictionary[pairableDevice.Identifier]);
        }

        void deviceRemoved(DeviceManager deviceManager, PairableDevice pairableDevice)
        {
            DeviceControlDictionary.Remove(pairableDevice.Identifier);
        }

        void personAdded(PersonManager personManager, PairablePerson pairablePerson)
        {
            PersonControlDictionary[pairablePerson.Identifier] = new PersonControl(pairablePerson);
            canvas.Children.Add(PersonControlDictionary[pairablePerson.Identifier]);
        }

        void personRemoved(PersonManager personManager, PairablePerson pairablePerson)
        {
            PersonControlDictionary.Remove(pairablePerson.Identifier);
        }

        void trackerChanged(PersonManager sender, Tracker tracker)
        {
            if (tracker != null)
            {
                drawnTracker = new DrawnTracker(tracker);
            }

        }

        #endregion




        /*
        public void Redraw(object s, EventArgs ev)
        {

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw the room's bounding box
                dc.DrawLine(new Pen(Brushes.Black, 1.0), new Point(0.0, 0.0), new Point(0.0, RenderHeight));
                dc.DrawLine(new Pen(Brushes.Black, 1.0), new Point(0.0, RenderHeight), new Point(RenderWidth, RenderHeight));
                dc.DrawLine(new Pen(Brushes.Black, 1.0), new Point(RenderWidth, RenderHeight), new Point(RenderWidth, 0.0));
                dc.DrawLine(new Pen(Brushes.Black, 1.0), new Point(RenderWidth, 0.0), new Point(0.0, 0.0));

                // Draw the Kinect(s)
                foreach (Tracker tracker in kinectManager.Locator.Trackers)
                {
                    if (tracker.Location.HasValue == false)
                        continue;

                    if (tracker.Location.Value.X != 0.0 && tracker.Location.Value.Y != 0.0)
                        dc.DrawEllipse(
                            Brushes.Black,
                            null,
                            ConvertFromMetersToPixels(tracker.Location.Value),
                            trackerDrawWidth,
                           trackerDrawHeight);


                    if (tracker.Orientation.HasValue == false)
                        continue;

                    // Draw two lines to serve as field of view indicators
                    double topAngle = Util.NormalizeAngle(tracker.Orientation.Value + 45);
                    double topX = Math.Cos(topAngle * Math.PI / 180);
                    double topY = Math.Sin(topAngle * Math.PI / 180);
                    dc.DrawLine(
                        new Pen(Brushes.Black, 0.3),
                        ConvertFromMetersToPixels(tracker.Location.Value),
                        ConvertFromMetersToPixels(new Point(tracker.Location.Value.X + topX, tracker.Location.Value.Y + topY)));

                    double bottomAngle = Util.NormalizeAngle(tracker.Orientation.Value - 45);
                    double bottomX = Math.Cos(bottomAngle * Math.PI / 180);
                    double bottomY = Math.Sin(bottomAngle * Math.PI / 180);
                    dc.DrawLine(
                        new Pen(Brushes.Black, 0.3),
                        ConvertFromMetersToPixels(tracker.Location.Value),
                        ConvertFromMetersToPixels(new Point(tracker.Location.Value.X + bottomX, tracker.Location.Value.Y + bottomY)));

                }


                //// Removes all currently drawn unpaired devices
                //unpairedDeviceStackPanel.Children.Clear();

                //// Updates the screen
                //addDevicesFromDeviceList();

                foreach (Person person in kinectManager.Locator.Persons)
                {
                    if (person.Location.HasValue == false)
                        continue;

                    // Find the person's device, if they are paired with one
                    List<PairableDevice> pairableDevices = kinectManager.Locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>();
                    PairableDevice device = pairableDevices.Find(x => x.Identifier.Equals(person.HeldDeviceIdentifier));

                    // Colour the person's dot by whether they are paired with a device
                    PairablePerson pperson = (PairablePerson)person;

                    Brush penBrush = getBrushFromPairingState(pperson.PairingState);
                    Brush backgroundBrush = Brushes.White;

                    if (pperson.PairingState == PairingState.Paired)
                    {
                        backgroundBrush = createBrushWithTextAndBackground(pperson.HeldDeviceIdentifier, backgroundBrush);
                    }


                    // Draw a dot for each person seen by the tracker
                    if (person.Location.Value.X != 0.0 && person.Location.Value.Y != 0.0)
                    {
                        dc.DrawEllipse(
                            backgroundBrush,
                            new Pen(penBrush, 2.0),
                            ConvertFromMetersToPixels(person.Location.Value),
                            deviceDrawWidth,
                            deviceDrawHeight);
                    }


                    // If we are pair a device, we can use its orientation data to draw its field of view and name
                    if (device != null)
                    {

                        if (device.Orientation.HasValue == false || device.FieldOfView.HasValue == false)
                            continue;

                        // Draw two lines to serve as field of view indicators
                        double topAngle = Util.NormalizeAngle(device.Orientation.Value + device.FieldOfView.Value / 2);
                        double topX = Math.Cos(topAngle * Math.PI / 180);
                        double topY = Math.Sin(topAngle * Math.PI / 180);
                        dc.DrawLine(
                            new Pen(Brushes.Black, 1.0),
                            ConvertFromMetersToPixels(device.Location.Value),
                            ConvertFromMetersToPixels(new Point(device.Location.Value.X + topX, device.Location.Value.Y + topY)));

                        double bottomAngle = Util.NormalizeAngle(device.Orientation.Value - device.FieldOfView.Value / 2);
                        double bottomX = Math.Cos(bottomAngle * Math.PI / 180);
                        double bottomY = Math.Sin(bottomAngle * Math.PI / 180);
                        dc.DrawLine(
                            new Pen(Brushes.Black, 1.0),
                            ConvertFromMetersToPixels(device.Location.Value),
                            ConvertFromMetersToPixels(new Point(device.Location.Value.X + bottomX, device.Location.Value.Y + bottomY)));
                    }

                }


                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                
            }
        }

        public void addUnpairedDeviceToScreen(string name, PairingState state)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = deviceDrawWidth * 2;
            ellipse.Height = deviceDrawHeight * 2;
            ellipse.StrokeThickness = 2;
            ellipse.Fill = createBrushWithTextAndBackground(name, Brushes.White);
            ellipse.Stroke = getBrushFromPairingState(state);
            ellipse.Margin = new Thickness(0, 0, 10, 0);

            unpairedDeviceStackPanel.Children.Add(ellipse);
        }


        /// <summary>
        /// Converts a pairing state to a Brush. This is useful so that if we want to change the color scheme for different states, we only need to do it here.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public Brush getBrushFromPairingState(PairingState state)
        {
            switch (state)
            {
                case (PairingState.NotPaired): return Brushes.DarkRed;
                case (PairingState.PairingAttempt): return Brushes.Orange;
                case (PairingState.Paired): return Brushes.Green;
                default: return Brushes.White;
            }
        }

        public void addDevicesFromDeviceList()
        {
            List<PairableDevice> deviceList = kinectManager.Locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>();
            foreach(PairableDevice device in deviceList) {

                // If the device is currently unpaired, then draw it to the screen
                if (device.PairingState == PairingState.NotPaired || device.PairingState == PairingState.PairingAttempt)
                {
                    addUnpairedDeviceToScreen(device.Identifier, device.PairingState);
                }
            }
        }


        /// <summary>
        /// Creates a VisualBrush with background as the Fill color, and text centered in the middle
        /// </summary>
        /// <param name="text">Text to be displayed</param>
        /// <param name="background">Brush Color</param>
        /// <returns>Properly formatted VisualBrush</returns>
        public VisualBrush createBrushWithTextAndBackground(string text, Brush background)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.FontFamily = new FontFamily("Arial");
            textBlock.Text = text;
            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            textBlock.TextWrapping = TextWrapping.WrapWithOverflow;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.Padding = new Thickness(5, deviceDrawHeight - textBlock.FontSize-5, 5, 0);

            textBlock.Height = deviceDrawHeight * 2;
            textBlock.Width = deviceDrawWidth * 2;

            StackPanel stackPanel = new StackPanel();
            stackPanel.Width = deviceDrawWidth * 2;
            stackPanel.Height = deviceDrawHeight * 2;
            stackPanel.Background = background;
            stackPanel.Children.Add(textBlock);

            VisualBrush visualBrush = new VisualBrush();
            visualBrush.Stretch = Stretch.None;
            visualBrush.Visual = stackPanel;
            visualBrush.TileMode = TileMode.None;

            return visualBrush;
        }*/
    }
}
