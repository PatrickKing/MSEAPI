using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace MSELocator
{
    public class Locator : LocatorInterface
    {

        #region Properties and Constructor
        
        private List<Person> _persons;
        public List<Person> Persons
        {
            get { return _persons; }
            set { _persons = value; }
        }

        private List<Device> _devices;
        public List<Device> Devices
        {
            get { return _devices; }
            set { _devices = value; }
        }

        // Tracker does not actually need to be a part of the locator to work, though it makes sense to keep tabs on trackers in a central place.
        private List<Tracker> _trackers;
        public List<Tracker> Trackers
        {
            get { return _trackers; }
            set { _trackers = value; }
        }

        public Locator()
        {

            Persons = new List<Person>();
            Devices = new List<Device>();
            Trackers = new List<Tracker>();

        }

        public override string ToString()
        {
            return String.Format
                (
                    "Locator[Trackers: [{0}], Devices: [{1}], Persons: [{2}]]",
                    String.Join(",", Trackers),
                    String.Join(",", Devices),
                    String.Join(",", Persons)
                );
        }


        #endregion

        #region GetDevicesInView

        public List<Device> GetDevicesInView(String identifier)
        {
            return GetDevicesInView(_devices.Find(x => x.Identifier.Equals(identifier)));
        }

        /// <summary>
        /// Computes the intersection point of two devices. 
        /// </summary>
        /// <param name="observer"></param>
        /// <param name="target"></param>
        /// <param name="Position"></param>
        /// <returns>Returns the intersection point of the two devices.</returns>
        public Point GetIntersectionPoint(Device observer, Device target, String Position)
        {
            Point returnPoint = new Point();

            if (Position.Equals("Back"))
            {
                Double y = (Double)(target.Location.Value.Y + (target.Height) / 2);
                Double slope = (Double)observer.Orientation * Math.PI / 180;
                slope = Math.Tan(slope);
                Double n = observer.Location.Value.Y - (slope * observer.Location.Value.X);
                Double x = (Double)((y - n) / slope);

                returnPoint = new Point(x, y);
            }
            else if (Position.Equals("Front"))
            {
                Double y = (Double)(target.Location.Value.Y - (target.Height) / 2);
                Double slope = (Double)observer.Orientation * Math.PI / 180;
                slope = Math.Tan(slope);
                Double n = observer.Location.Value.Y - (slope * observer.Location.Value.X);
                Double x = (Double)((y - n) / slope);

                returnPoint = new Point(x, y);
            }
            else if (Position.Equals("Right"))
            {
                Double x = (Double)(target.Location.Value.X + (target.Width) / 2);
                Double slope = (Double)observer.Orientation * Math.PI / 180;
                slope = Math.Tan(slope);
                Double n = observer.Location.Value.Y - (slope * observer.Location.Value.X);
                Double y = (Double)(slope * x + n);

                returnPoint = new Point(x, y);
            }
            else if (Position.Equals("Left"))
            {
                Double x = (Double)(target.Location.Value.X - (target.Width) / 2);
                Double slope = (Double)observer.Orientation * Math.PI / 180;
                slope = Math.Tan(slope);
                Double n = observer.Location.Value.Y - (slope * observer.Location.Value.X);
                Double y = (Double)(slope * x + n);

                returnPoint = new Point(x, y);
            }

            return returnPoint;
        }

        /// <summary>
        /// Checks if the the observer device's line of sight intersects the target device.
        /// </summary>
        /// <param name="observer"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool DoesObserverInteresectTarget(Device observer, Device target)
        {
            Double topYline = (Double) (target.Location.Value.Y + (target.Height) / 2);
            Double bottomYline = (Double)(target.Location.Value.Y - (target.Height) / 2);
            Double rightXline = (Double)(target.Location.Value.X + (target.Width) / 2);
            Double leftXline = (Double)(target.Location.Value.X - (target.Width) / 2);

            Double slope = (Double)observer.Orientation * Math.PI / 180;
            slope = Math.Tan(slope);
            Double n = observer.Location.Value.Y - (slope * observer.Location.Value.X);

            Double x = (topYline - n) / slope;
            if (x >= leftXline && x <= rightXline)
                return true;

            x = (bottomYline - n) / slope;
            if (x >= leftXline && x <= rightXline)
                return true;

            double y = slope * rightXline + n;
            if (y >= bottomYline && y <= topYline)
                return true;

            y = slope * leftXline + n;
            if (y >= bottomYline && y <= topYline)
                return true;


            return false;
        }


        /// <summary>
        /// Computes the devices within the field of view of the observer alongside the intersection point 
        /// with each of these devices. Returns an empty dictionary if FieldOfView or Location are null on the observer.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>Devices in the field of view of the observer and their intersection points.</returns>
        public Dictionary<Device, Point> GetDevicesInViewWithIntersectionPoints(Device observer)
        {                        
            Dictionary<Device, Point> returnDevices = new Dictionary<Device, Point>();

            List<Device> devicesInView = GetDevicesInView(observer);

            foreach (Device device in devicesInView)
            {
               //if the observer device doesn't intersect the target device then skip device.
                if(!DoesObserverInteresectTarget(observer,device))
                 continue;

                // Find Relative Position to the observer
                if (observer.Location.Value.Y > device.Location.Value.Y + (device.Height)/2)
                {
                    // Device is in the back, check right, center, or left
                    if (observer.Location.Value.X > device.Location.Value.X)
                    {
                        // Device is in the upper right corner
                        Point returnPoint = GetIntersectionPoint(observer, device, "Back");
                        returnDevices.Add(device, returnPoint);
                    }
                    else if (observer.Location.Value.X < device.Location.Value.X)
                    {
                        // Device is in the upper left corner
                        Point returnPoint = GetIntersectionPoint(observer, device, "Back");
                        returnDevices.Add(device, returnPoint);
                    }
                    else
                    {
                        // Device is strictly in the back
                        Point returnPoint = GetIntersectionPoint(observer, device, "Back");
                        returnDevices.Add(device, returnPoint);
                    }

                }

                else if (observer.Location.Value.Y < device.Location.Value.Y - (device.Height) / 2)
                {
                    // Device is in the front, check right, center, or left
                    if (observer.Location.Value.X > device.Location.Value.X)
                    {
                        // Device is in the lower right corner
                        Point returnPoint = GetIntersectionPoint(observer, device, "Front");
                        returnDevices.Add(device, returnPoint);
                    }
                    else if (observer.Location.Value.X < device.Location.Value.X)
                    {
                        // Device is in the lower left corner
                        Point returnPoint = GetIntersectionPoint(observer, device, "Front");
                        returnDevices.Add(device, returnPoint);
                    }
                    else
                    {
                        // Device is strictly in the front
                        Point returnPoint = GetIntersectionPoint(observer, device, "Front");
                        returnDevices.Add(device, returnPoint);
                    }
                }
                else
                {
                    // Device is either to the right or left
                    if (observer.Location.Value.X > device.Location.Value.X)
                    {
                        // Device is to the right
                        Point returnPoint = GetIntersectionPoint(observer, device, "Right");
                        returnDevices.Add(device, returnPoint);
                    }
                    else
                    {
                        // Device is to the left
                        Point returnPoint = GetIntersectionPoint(observer, device, "Left");
                        returnDevices.Add(device, returnPoint);
                    }
                }
                

            }

            return returnDevices;
        }

        /// <summary>
        /// Computes the devices within the field of view of the observer alongside the intersection point 
        /// with each of these devices. Returns an empty dictionary if FieldOfView or Location are null on the observer.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>Devices in the field of view of the observer and their intersection points.</returns>
        public Dictionary<Device, Point> GetDevicesInViewWithIntersectionPoints2(Device observer)
        {
            Dictionary<Device, Point> returnDevices = new Dictionary<Device, Point>();

            List<Device> devicesInView = GetDevicesInView(observer);

            foreach (Device target in devicesInView)
            {
                //if the observer device doesn't intersect the target device then skip to the next device.
                if (!DoesObserverInteresectTarget(observer, target))
                    continue;

                //line equation of the observer's line of sight : y = slope*x + n
                Double slope = (Double)observer.Orientation * Math.PI / 180;
                slope = Math.Tan(slope);
                Double n = observer.Location.Value.Y - (slope * observer.Location.Value.X);

                //all 4 sides of the target device
                Double topYline = (Double)(target.Location.Value.Y + (target.Height) / 2);
                Double bottomYline = (Double)(target.Location.Value.Y - (target.Height) / 2);
                Double rightXline = (Double)(target.Location.Value.X + (target.Width) / 2);
                Double leftXline = (Double)(target.Location.Value.X - (target.Width) / 2);

                Double x, y;
                Point intersection = new Point();

                //observer device is on top
                if (observer.Location.Value.Y >= topYline && observer.Location.Value.X <= rightXline && observer.Location.Value.X >= leftXline)
                {
                    x = (topYline - n) / slope;
                    intersection = new Point(x, topYline);
                }
                //observer device is on the bottom
                else if (observer.Location.Value.Y <= bottomYline && observer.Location.Value.X <= rightXline && observer.Location.Value.X >= leftXline)
                {
                    x = (bottomYline - n) / slope;
                    intersection = new Point(x, bottomYline);
                }
                //observer device is on the right
                else if (observer.Location.Value.X >= rightXline && observer.Location.Value.Y >= bottomYline && observer.Location.Value.Y <= topYline)
                {
                    y = slope * rightXline + n;
                    intersection = new Point(rightXline, y);
                }
                //observer device is on the left
                else if (observer.Location.Value.X <= leftXline && observer.Location.Value.Y >= bottomYline && observer.Location.Value.Y <= topYline)
                {
                    y = slope * leftXline + n;
                    intersection = new Point(leftXline, y);
                }
                //observer device is in the top right corner
                else if (observer.Location.Value.X >= rightXline && observer.Location.Value.Y >= topYline)
                {
                    x = (topYline - n) / slope;
                    if (x <= rightXline)
                        intersection =  new Point(x, topYline);
                    else
                    {
                        y = slope * rightXline + n;
                        intersection = new Point(rightXline, y);
                    }
                }
                //observer device is in the bottom right corner
                else if (observer.Location.Value.X >= rightXline && observer.Location.Value.Y <= bottomYline)
                {
                    x = (bottomYline - n) / slope;
                    if (x <= rightXline)
                        intersection = new Point(x, bottomYline);
                    else
                    {
                        y = slope * rightXline + n;
                        intersection = new Point(rightXline, y);
                    }
                }
                //observer device is in the bottom left corner
                else if (observer.Location.Value.X <= leftXline && observer.Location.Value.Y <= bottomYline)
                {
                    x = (bottomYline - n) / slope;
                    if (x >= leftXline)
                        intersection = new Point(x, bottomYline);
                    else
                    {
                        y = slope * leftXline + n;
                        intersection = new Point(leftXline, y);
                    }
                }
                //observer device is in the top left corner
                else if (observer.Location.Value.X <= leftXline && observer.Location.Value.Y >= topYline)
                {
                    x = (topYline - n) / slope;
                    if (x >= leftXline)
                        intersection = new Point(x, topYline);
                    else
                    {
                        y = slope * leftXline + n;
                        intersection = new Point(leftXline, y);
                    }
                }

                returnDevices.Add(target, intersection);
            }

            return returnDevices;
        
        }

        /// <summary>
        /// Computes the devices within the field of view of the observer. Returns an empty list if FieldOfView or Location are null on the observer.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>Devices in the field of view of the observer.</returns>
        public List<Device> GetDevicesInView(Device observer)
        {
            List<Device> returnDevices = new List<Device>();

            //(CB - Should we throw an exception here? Rather then just returning an empty list?)
            if (observer.Location == null || observer.Orientation == null)
                return returnDevices;
            if (observer.FieldOfView == 0.0)
                return returnDevices;

            // We imagine the field of view as two vectors, pointing away from the observing device. Targets between the vectors are in view.
            // We will use angles to represent these vectors.
            double leftFieldOfView = Util.NormalizeAngle(observer.Orientation.Value + observer.FieldOfView.Value / 2);
            double rightFieldOfView = Util.NormalizeAngle(observer.Orientation.Value - observer.FieldOfView.Value / 2);


            foreach (Device target in _devices)
            {
                if (target == observer || !target.Location.HasValue)
                    continue;

                // Atan2 is the inverse tangent function, given lengths for the opposite and adjacent legs of a right triangle, it returns the angle
                double angle = Util.NormalizeAngle(Math.Atan2(target.Location.Value.Y - observer.Location.Value.Y, target.Location.Value.X - observer.Location.Value.X) * 180 / Math.PI);

                // Ordinarily, the angle defining the left boundary of the field of view will be larger than the angle for the right.
                // For example, if our device has an orientation of 90.0 and a field of view of 15 degrees, then the left and right FoV vectors are at 97.5 and 82.5 degrees.
                // In this case, the target must be at an angle between left and right to be in view.
                if (leftFieldOfView > rightFieldOfView && angle < leftFieldOfView && angle > rightFieldOfView)
                {
                    returnDevices.Add(target);
                }
                // If the field of view includes the X axis, then the left field of view will be smaller than the right field of view.
                // For example, if our device has an orientation of 0.0 and a field of view of 15 degrees, then the left FoV vector will be at 7.5 degrees,
                // and the right FoV will be at 352.5 degrees.
                else if (leftFieldOfView < rightFieldOfView)
                {
                    if (angle < leftFieldOfView || angle > rightFieldOfView)
                        returnDevices.Add(target);
                }


            }

            return returnDevices;

        }
        #endregion

        #region GetNearestDeviceInView

        public Device GetNearestDeviceInView(String identifier)
        {
            return GetNearestDeviceInView(_devices.Find(x => x.Identifier.Equals(identifier)));
        }

        public Device GetNearestDeviceInView(Device observer)
        {
            List<Device> devicesInView = GetDevicesInView(observer);
            return FindNearestDevice(observer, devicesInView);
        }



        #endregion
       
        #region GetDevicesWithinRange

        public List<Device> GetDevicesWithinRange(String identifier, double distance)
        {
            return GetDevicesWithinRange(_devices.Find(x => x.Identifier.Equals(identifier)), distance);
        }

        public List<Device> GetDevicesWithinRange(Device observer, double distance)
        {
            List<Device> returnDevices = new List<Device>();

            if (!observer.Location.HasValue)
                return returnDevices;

            foreach (Device device in _devices)
            {
                if (device == observer)
                    continue;
                else if (device.Location.HasValue && Util.DistanceBetweenPoints(observer.Location.Value, device.Location.Value) < distance)
                {
                    returnDevices.Add(device);
                }
            }

            return returnDevices;
        }



        #endregion

        #region GetNearestDeviceWithinRange

        public Device GetNearestDeviceWithinRange(String identifier, double distance)
        {
            return GetNearestDeviceWithinRange(_devices.Find(x => x.Identifier.Equals(identifier)), distance);
        }

        public Device GetNearestDeviceWithinRange(Device observer, double distance)
        {
            List<Device> devicesInView = GetDevicesWithinRange(observer, distance);
            return FindNearestDevice(observer, devicesInView);
        }


        #endregion


        private static Device FindNearestDevice(Device observer, List<Device> deviceList)
        {
            if (deviceList.Count == 0)
                return null;
            else
            {
                Device nearest = null;

                //First, find a device with a location to compare against
                foreach (Device device in deviceList)
                {
                    if (device != observer && device.Location.HasValue)
                    {
                        nearest = device;
                    }
                }
                if (nearest == null)
                    return null;

                //Find the device with the least distance to the observer
                foreach (Device device in deviceList)
                {
                    if (device != observer && device.Location.HasValue &&
                        Util.DistanceBetweenPoints(device.Location.Value, observer.Location.Value) < Util.DistanceBetweenPoints(nearest.Location.Value, observer.Location.Value))
                    {
                        nearest = device;
                    }
                }
                return nearest;

            }
        }

    }
}
