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
using MSELocator;

namespace RoomVisualizer
{
    /// <summary>
    /// Interaction logic for TrackerControl.xaml
    /// </summary>
    /// 

    public partial class TrackerControl : UserControl
    {

        public TrackerControl(Tracker tracker)
        {
            InitializeComponent();


            LeftLine.StrokeThickness = DrawingResources.TRACKER_FOV_WIDTH;
            RightLine.StrokeThickness = DrawingResources.TRACKER_FOV_WIDTH;

            tracker.LocationChanged += onLocationChanged;
            tracker.OrientationChanged += onOrientationChanged;
            tracker.FOVChanged += onFOVChanged;
            tracker.RangeChanged += onRangeChanged;
        }


        public void onOrientationChanged(Device device)
        {
            // We are using RotateTransform now to make things easier. Everything should be drawn pointing downwards (270 degrees)
            this.RenderTransform = new RotateTransform((device.Orientation.Value * -1) + 270);
        }

        public void onLocationChanged(Device device)
        {
            if (device.Location.HasValue)
            {
                Point newPoint = DrawingResources.ConvertFromMetersToDisplayCoordinates(device.Location.Value, MainWindow.SharedCanvas);
                Canvas.SetLeft(this, newPoint.X);
                Canvas.SetTop(this, newPoint.Y);
            }
        }

        public void onRangeChanged(Tracker tracker)
        {
            updateRange(tracker);
        }

        public void onFOVChanged(Device device)
        {
            Tracker tracker = (Tracker)device;
            updateFOV(tracker);
            updateRange(tracker);
        }

        /// <summary>
        /// This updates the FOV draw lines for a given tracker
        /// </summary>
        /// <param name="tracker"></param>
        public void updateFOV(Tracker tracker)
        {
            double FOVAngle = tracker.FieldOfView.Value / 2;

            // Draw two lines to serve as field of view indicators
            double topAngle = Util.NormalizeAngle(270 + FOVAngle);
            double topX = Math.Cos(topAngle * Math.PI / 180);
            double topY = Math.Sin(topAngle * Math.PI / 180);
            Point newLeft = DrawingResources.ConvertPointToProperLength(new Point(topX, topY), DrawingResources.TRACKER_FOV_LENGTH);
            LeftLine.X2 = newLeft.X;
            LeftLine.Y2 = -newLeft.Y;

            double bottomAngle = Util.NormalizeAngle(270 - FOVAngle);
            double bottomX = Math.Cos(bottomAngle * Math.PI / 180);
            double bottomY = Math.Sin(bottomAngle * Math.PI / 180);
            Point newRight = DrawingResources.ConvertPointToProperLength(new Point(bottomX, bottomY), DrawingResources.TRACKER_FOV_LENGTH);
            RightLine.X2 = newRight.X;
            RightLine.Y2 = -newRight.Y;
        }


        /// <summary>
        /// This updates the Range drawing for a given tracker
        /// </summary>
        /// <param name="tracker"></param>
        public void updateRange(Tracker tracker)
        {
            double FOVAngle = tracker.FieldOfView.Value / 2;

            // We want to adjust the MinRange triangle, so we're using sin law
            if (tracker.MinRange.HasValue)
            {
                double leftX = (tracker.MinRange.Value * Math.Sin(Util.DEGREES_TO_RADIANS * FOVAngle)) / (Util.DEGREES_TO_RADIANS * (180.0 - FOVAngle - 90));
                double leftXPixels = DrawingResources.ConvertFromMetersToPixelsX(leftX, MainWindow.SharedCanvas);
                double YPixels = DrawingResources.ConvertFromMetersToPixelsY(tracker.MinRange.Value, MainWindow.SharedCanvas);

                NearTriangle.Points.Clear();
                NearTriangle.Points.Add(new Point(0, 0));
                NearTriangle.Points.Add(new Point(leftXPixels, YPixels));
                NearTriangle.Points.Add(new Point(-leftXPixels, YPixels));
            }

            // Adjusting the FarLine
            if (tracker.MaxRange.HasValue)
            {
                double leftX = (tracker.MaxRange.Value * Math.Sin(Util.DEGREES_TO_RADIANS * FOVAngle)) / (Util.DEGREES_TO_RADIANS * (180.0 - FOVAngle - 90));
                double leftXPixels = DrawingResources.ConvertFromMetersToPixelsX(leftX, MainWindow.SharedCanvas);
                double YPixels = DrawingResources.ConvertFromMetersToPixelsY(tracker.MaxRange.Value, MainWindow.SharedCanvas);

                FarLine.X1 = leftXPixels;
                FarLine.Y1 = YPixels;
                FarLine.X2 = -leftXPixels;
                FarLine.Y2 = YPixels;
            }
        }

        /// <summary>
        /// Hides the Range drawing
        /// </summary>
        public void hideRange()
        {
            FarLine.Visibility = Visibility.Hidden;
            NearTriangle.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Shows the Range drawing
        /// </summary>
        public void showRange()
        {
            FarLine.Visibility = Visibility.Visible;
            NearTriangle.Visibility = Visibility.Visible;
        }

    }
}
