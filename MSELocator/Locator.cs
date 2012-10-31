using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace MSELocator
{
    public class Locator : LocatorInterface
    {
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



        public Device GetNearestDeviceInView(String identifier)
        {
            return GetNearestDeviceInView(_devices.Find(x => x.Identifier.Equals(identifier)));
        }

        public Device GetNearestDeviceInView(Device observer)
        {
            List<Device> devicesInView = GetDevicesInView(observer);
            if (devicesInView.Count == 0)
                return null;
            else
            {
                Device nearest = null;

                //First, find a device with a location to compare against
                foreach (Device device in devicesInView)
                {
                    if (device.Location.HasValue)
                    {
                        nearest = device;
                    }
                }
                if (nearest == null)
                    return null;

                //Find the device with the least distance to the observer
                foreach (Device device in devicesInView)
                {
                    if (device.Location.HasValue && 
                        Util.DistanceBetweenPoints(device.Location.Value, observer.Location.Value) < Util.DistanceBetweenPoints(nearest.Location.Value, observer.Location.Value))
                    {
                        nearest = device;
                    }
                }
                return nearest;

            }
        }


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
                else if ( device.Location.HasValue &&  Util.DistanceBetweenPoints(observer.Location.Value, device.Location.Value) < distance)
                {
                    returnDevices.Add(device);
                }
            }

            return returnDevices;
        }

        public List<Device> GetDevicesInView(String identifier)
        {
            return GetDevicesInView(_devices.Find(x => x.Identifier.Equals(identifier)));
        }


        /// <summary>
        /// Computes the devices within the field of view of the observer. Returns an empty list if FieldOfView or Location are null on the observer.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>Devices in the field of view of the observer.</returns>
        public List<Device> GetDevicesInView(Device observer)
        {
            List<Device> returnDevices = new List<Device>();

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
                if (target == observer)
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

    }
}
