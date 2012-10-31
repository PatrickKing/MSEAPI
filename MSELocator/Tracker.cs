using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;
//using Microsoft.Kinect;

namespace MSELocator
{
    /// <summary>
    /// A Tracker is a device that can physically track the locations of other devices in the room.
    /// </summary>
    public class Tracker : Device
    {


        // In the Tracker's coordinate space, it is at the origin and with orientation 0 (facing down the X axis).

        // In the Kinect's coordinate space, it is at 0.0, and faces down the Z axis.
        // Left-right movement in front of the camera is mapped to the X axis, and vertical movement is mapped to the Y axis.
        // So, the Kinect's Z axis corresponds to the Tracker's X axis, and the Kinect's X axis corresponds to the Tracker's Y axis.
        
        // To translate from the Kinect's coordinate space to the Tracker's coordinate space, you want to pass a new Vector(SkeletonPoint.Z, SkeletonPoint.X).

        /// <summary>
        /// Accepts a point in the Tracker's coordinate space, translates it into the room's coordinate space using the Tracker's position and orientation, and updates the location of the device with it. 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="vector"></param>
        public void UpdatePositionForDevice(Device device, Vector vector)
        {
            Vector updatedPosition = Util.TranslateFromCoordinateSpace(vector, Orientation, new Vector(Location.Value.X, Location.Value.Y));
            device.Location = new Point(updatedPosition.X, updatedPosition.Y);            
        }

        /*public void UpdatePositionForDevice(Device device, SkeletonPoint skeletonPoint)
        {
            Vector updatedPosition = Util.TranslateFromCoordinateSpace(new Vector(skeletonPoint.Z, skeletonPoint.X), Orientation, new Vector(Location.X, Location.Y));
            device.Location = new Point(updatedPosition.X, updatedPosition.Y);
        }*/

    }
}
