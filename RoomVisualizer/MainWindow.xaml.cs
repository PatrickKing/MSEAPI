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

        #region ivars
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

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

        #endregion

        const double xRange = 4.5;
        const double yRange = 3.0;

        const int FPS = 60;

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
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.White, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                // Draw the room's bounding box
                dc.DrawLine(new Pen(Brushes.Black, 1.0), new Point(0.0, 0.0), new Point(0.0, RenderHeight));
                dc.DrawLine(new Pen(Brushes.Black, 1.0), new Point(0.0, RenderHeight), new Point(RenderWidth, RenderHeight));
                dc.DrawLine(new Pen(Brushes.Black, 1.0), new Point(RenderWidth, RenderHeight), new Point(RenderWidth, 0.0));
                dc.DrawLine(new Pen(Brushes.Black, 1.0), new Point(RenderWidth, 0.0), new Point(0.0, 0.0));

                foreach (Person person in kinectManager.Locator.Persons)
                {
                    if (person.Location.HasValue == false)
                        continue;

                    // Find the person's device, if they are paired with one
                    List<PairableDevice> pairableDevices = kinectManager.Locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>();
                    PairableDevice device = pairableDevices.Find(x => x.Identifier.Equals(person.HeldDeviceIdentifier));
                    Brush brush;

                    // Colour the person's dot by whether they have a device, and whether they see any devices
                    if (device != null)
                    {
                        List<Device> results = kinectManager.Locator.GetDevicesInView(device);
                        if (results != null && results.Count >= 1)
                        {
                            brush = Brushes.Green;
                        }
                        else
                            brush = Brushes.Yellow;
                    }
                    else
                        brush = Brushes.Red;


                    // Draw a dot for each person seen by the tracker
                    if (person.Location.Value.X != 0.0 && person.Location.Value.Y != 0.0)
                        dc.DrawEllipse(
                            Brushes.Red,
                            null,
                            ConvertFromMetersToPixels(person.Location.Value),
                            0.25 * RenderWidth / xRange,
                            0.25 * RenderHeight / yRange);


                    // If we are pair a device, we can use its orientation data to draw its field of view
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
    }
}
