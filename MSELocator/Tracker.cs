using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;
using KinectServer;
//using Microsoft.Kinect;

namespace MSELocator
{
    /// <summary>
    /// A Tracker is a device that can physically track the locations of other devices in the room. Usage: set the Tracker's orientation and location to match where the physical device 
    /// is in the room, and then use the Tracker's methods to update Persons and Devices, passing in X,Y coordinates for those entities in the Tracker's coordinate space. 
    /// </summary>
    public class Tracker : Device
    {

        public double? Orientation
        {
            get { return base.Orientation; }
            set
            {
                base.Orientation = value;
                if(base.Orientation != null)
                    _kinectServer.updateKinectOrientation(this.Identifier, (double)base.Orientation);
            }
        }

        public Point? Location
        {
            get { return base.Location; }
            set
            {
                base.Location = value;
                if (base.Location != null)
                    _kinectServer.updateKinectLocation(this.Identifier,(Point) base.Location);
            }
        }

        public delegate void TrackerEventSignature(Tracker sender);
        public event TrackerEventSignature RangeChanged;

        private MSEKinectServer _kinectServer;
            
        private double? _MinRange;

        public Tracker(string KinectID, MSEKinectServer ks)
        {
            this.Identifier = KinectID;
            this._kinectServer = ks;
            this.State = CallibrationState.NotCalibrated;
        }

        public enum CallibrationState { Calibrated, NotCalibrated };
        private CallibrationState? _state;
        public CallibrationState? State
        {
            get { return _state; }
            set
            {
                _state = value;
            }
        }

        public Tracker()
        { }

        /// <summary>
        /// A distance in meters where the tracker cannot accurately track closer to that point. The distance of 0 - MinRange is a dead zone for tracking
        /// </summary>
        public double? MinRange
        {
            get { return _MinRange; }
            set
            {
                _MinRange = value;

                if (RangeChanged != null)
                    RangeChanged(this);
            }
        }


        private double? _MaxRange;
        /// <summary>
        /// A distance in meters where the tracker cannot accurately track past that point. The distance of MaxRange - infinity is a dead zone for tracking
        /// </summary>
        public double? MaxRange
        {
            get { return _MaxRange; }
            set
            {
                _MaxRange = value;

                if (RangeChanged != null)
                    RangeChanged(this);
            }
        }

        /// <summary>
        /// Calling this function will send a message to the kinect client of this tracker telling it to start sending skeleton data.
        /// </summary>
        public void StartStreaming()
        {
            this._kinectServer.startKinectStream(this.Identifier);
        }

        /// <summary>
        /// Calling this function will send a message to the kinect client of this tracker telling it stop sending skeleton data.
        /// </summary>
        public void StopStreaming()
        {
            this._kinectServer.stopKinectStream(this.Identifier);
        }



        // To use data received from the Kinect, or other tracker, the coordinates need to be translated from the Kinect's coordinate space, to the tracker's coordinate space, to the locator's coordinate space. 

        // In the Locator's coordinate space, the tracker is at the position and orientation stored in the tracker's instance variables

        // In the Tracker's coordinate space, it is at the origin and with orientation 0 (facing down the X axis).

        // In the Kinect's coordinate space, it is at the origin, and faces down the Z axis.
        // Left-right movement in front of the camera is mapped to the X axis, and vertical movement is mapped to the Y axis.

        // So, the Kinect's Z axis corresponds to the Tracker's X axis, and the Kinect's X axis corresponds to the Tracker's Y axis.        
        // To translate from the Kinect's coordinate space to the Tracker's coordinate space, you can create a Vector(SkeletonPoint.Z, SkeletonPoint.X).

        // To translate from the Tracker's coordinate space to the locator's coordinate space, use this class's methods.

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

        public void UpdatePositionForPerson(Person person, Vector vector)
        {
            Vector updatedPosition = Util.TranslateFromCoordinateSpace(vector, Orientation, new Vector(Location.Value.X, Location.Value.Y));
            person.Location = new Point(updatedPosition.X, updatedPosition.Y);
        }

        public Point ConvertSkeletonToRoomSpace(Vector skeletonPosition)
        {
            Vector v = Util.TranslateFromCoordinateSpace(skeletonPosition, Orientation, new Vector(Location.Value.X, Location.Value.Y));
            return new Point(v.X,v.Y);
        }


        /// <summary>
        /// Returns the device's orientation in the locator's coordinate space, under the assumption that the device is pointed toward the tracker.
        /// For use with a calibration button/function on the device, that the user will trigger while holding the device so that it faces the tracker. 
        /// Also sets the device's orientation to the calculated value.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public double ZeroDeviceOrientation(Device device)
        {
            if (!device.Location.HasValue)
                throw new ArgumentException("Tracker.ZeroDeviceOrientation : device Location was null. ");

            // find the device's orientation with respect to the tracker's location
            // i.e., find the angle formed by two lines: x axis and the line between the tracker and the device
            device.Orientation = Math.Atan2(device.Location.Value.Y - this.Location.Value.Y, device.Location.Value.X - this.Location.Value.X) * 180/Math.PI;
            return device.Orientation.Value;

        }

        /*public void UpdatePositionForDevice(Device device, SkeletonPoint skeletonPoint)
        {
            Vector updatedPosition = Util.TranslateFromCoordinateSpace(new Vector(skeletonPoint.Z, skeletonPoint.X), Orientation, new Vector(Location.X, Location.Y));
            device.Location = new Point(updatedPosition.X, updatedPosition.Y);
        }*/

    }
}
