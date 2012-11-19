using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MSELocator;
using System.Windows.Shapes;

namespace RoomVisualizer
{
    /// <summary>
    /// Represents a kinect tracker in the RoomVisualiser's canvas, gathers together several shapes used to represent it. 
    /// </summary>
    class DrawnTracker
    {

        Ellipse trackerDot;
        Line leftFieldOfViewLine;
        Line rightFieldOfViewLine;


        public DrawnTracker(Tracker tracker)
        {
            tracker.LocationChanged += onLocationChanged;
            tracker.OrientationChanged += onOrientationChanged;
        }

        public void onOrientationChanged(Device device)
        {


        }

        public void onLocationChanged(Device device)
        {

        }

    }
}
