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

        #region ivars
        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        MSEKinectManager kinectManager;
        DispatcherTimer dispatchTimer;

        // Dictionary for the unpaired devices that are currently drawn in the unpaired device area
        private Dictionary<string, StackPanel> drawnUnpairedDeviceDictionary = new Dictionary<string, StackPanel>();

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
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectManager = new MSEKinectManager();
            kinectManager.Start();

            
            dispatchTimer = new DispatcherTimer(new TimeSpan(1000 / FPS * 1000), DispatcherPriority.Normal, new EventHandler(Redraw), Dispatcher.CurrentDispatcher);
            dispatchTimer.Start();

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            Point oldLocation = kinectManager.Locator.Trackers[0].Location.Value;
            Point? newLocation = new Point(xRange / 2, yRange);

            Tracker tracker = kinectManager.Locator.Trackers[0];

            tracker.Location = newLocation;
            tracker.Orientation = 270;


        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kinectManager.Stop();
            dispatchTimer.Stop();
        }


    
        private Point ConvertFromMetersToPixels(Point myPoint)
        {
            return new Point(myPoint.X * RenderWidth / xRange, RenderHeight - (myPoint.Y * RenderHeight / yRange));
        }

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


                // Removes all currently drawn unpaired devices
                unpairedDeviceStackPanel.Children.Clear();

                // Removes from dictionary as well
                drawnUnpairedDeviceDictionary.Clear();

                // Updates the screen
                addDevicesFromDeviceList();

                foreach (Person person in kinectManager.Locator.Persons)
                {
                    if (person.Location.HasValue == false)
                        continue;

                    // Find the person's device, if they are paired with one
                    List<PairableDevice> pairableDevices = kinectManager.Locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>();
                    PairableDevice device = pairableDevices.Find(x => x.Identifier.Equals(person.HeldDeviceIdentifier));
                    Brush brush;

                    // Colour the person's dot by whether they have a device, and whether they see any devices
                    PairablePerson pperson = (PairablePerson)person;
                    if (pperson.PairingState == PairingState.NotPaired)
                        brush = Brushes.Red;
                    else if (pperson.PairingState == PairingState.PairingAttempt)
                        brush = Brushes.Yellow;
                    else
                    { 
                        brush = createBrushWithTextAndBackground(pperson.HeldDeviceIdentifier, Brushes.Green);
                    }


                    // Draw a dot for each person seen by the tracker
                    if (person.Location.Value.X != 0.0 && person.Location.Value.Y != 0.0) {
                        dc.DrawEllipse(
                            brush,
                            null,
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
            // stackPanel will contain a TextBlock for the name, and then a Ellipse for the Device
            StackPanel stackPanel = new StackPanel();
            
            // Oriented Vertically
            stackPanel.Orientation = Orientation.Vertical;

            // Creating the Ellipse
            Ellipse myEllipse = new Ellipse();
            myEllipse.StrokeThickness = 2;
            myEllipse.Stroke = Brushes.Red;
            if (state == PairingState.PairingAttempt)
            {
                myEllipse.Stroke = Brushes.Yellow;
            }

            myEllipse.Width = 55;
            myEllipse.Height = 55;
            myEllipse.Margin = new Thickness(10, 0, 10, 0);
            
            // Creating the TextBlock
            TextBlock text = new TextBlock();
            text.Text = name;
            text.Margin = new Thickness(10, 0, 10, 5);
            text.FontFamily = new FontFamily("Arial");
            text.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            // Adding the items to the Stack Panel
            stackPanel.Children.Add(text);
            stackPanel.Children.Add(myEllipse);

            // Adding the Stack Panel to the larger Stack Panel
            unpairedDeviceStackPanel.Children.Add(stackPanel);

            // Adding an entry to the Dictionary to be able to remove it later
            if (!drawnUnpairedDeviceDictionary.ContainsKey(name))
            {
                drawnUnpairedDeviceDictionary.Add(name, stackPanel);
            }

        }

        public void removeUnpairedDeviceFromScreen(string name)
        {
            if (drawnUnpairedDeviceDictionary.ContainsKey(name))
            {
                // Remove from Dictionary and from Stack Panel
                unpairedDeviceStackPanel.Children.Remove(drawnUnpairedDeviceDictionary[name]);
                drawnUnpairedDeviceDictionary.Remove(name);
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
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.Padding = new Thickness(5, deviceDrawHeight - textBlock.FontSize, 5, 0);

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
        }
    }
}
