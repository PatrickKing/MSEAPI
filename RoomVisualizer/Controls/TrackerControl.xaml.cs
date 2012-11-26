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
        }


        // Since Tracker is a subclass of Device, we and the events are handled in Device, we must cast it to a Tracker
        public void onOrientationChanged(Device device)
        {
            Tracker tracker = (Tracker)device;
            
            // Draw two lines to serve as field of view indicators
            double topAngle = Util.NormalizeAngle(tracker.Orientation.Value + tracker.FieldOfView.Value/2);
            double topX = Math.Cos(topAngle * Math.PI / 180);
            double topY = Math.Sin(topAngle * Math.PI / 180);
            Point newLeft = DrawingResources.ConvertPointToProperLength(new Point(topX, topY), DrawingResources.TRACKER_FOV_LENGTH);
            LeftLine.X2 = newLeft.X;
            LeftLine.Y2 = -newLeft.Y;

            double bottomAngle = Util.NormalizeAngle(tracker.Orientation.Value - tracker.FieldOfView.Value/2);
            double bottomX = Math.Cos(bottomAngle * Math.PI / 180);
            double bottomY = Math.Sin(bottomAngle * Math.PI / 180);
            Point newRight = DrawingResources.ConvertPointToProperLength(new Point(bottomX, bottomY), DrawingResources.TRACKER_FOV_LENGTH);
            RightLine.X2 = newRight.X;
            RightLine.Y2 = -newRight.Y; ;

        }

        // Since Tracker is a subclass of Device, we and the events are handled in Device, we must cast it to a Tracker
        public void onLocationChanged(Device device)
        {
            Tracker tracker = (Tracker)device;
            if (tracker.Location.HasValue)
            {
                Point newPoint = DrawingResources.ConvertFromMetersToDisplayCoordinates(tracker.Location.Value, MainWindow.SharedCanvas);
                Canvas.SetLeft(this, newPoint.X);
                Canvas.SetTop(this, newPoint.Y);
            }

        }

    }
}
