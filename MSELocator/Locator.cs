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
        /// Computes the devices within the field of view of the observer alongside the intersection point 
        /// with each of these devices. Returns an empty dictionary if FieldOfView or Location are null on the observer.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>Devices in the field of view of the observer and their intersection points.</returns>
        public Dictionary<Device, Point> GetDevicesInViewWithIntersectionPoints(Device observer)
        {
            Dictionary<Device, Point> returnDevices = new Dictionary<Device, Point>();

            Line obseverLineOfSight = new Line(observer.Location, observer.Orientation);

            List<Device> devicesInView = GetDevicesInView(observer);

            foreach (Device target in devicesInView)
            {
                if (target.Width == null || target.Width == null)
                    continue;

                List<Line> sides = getLinesOfShape(target);
                List<Point?> intersectionPoints = new List<Point?>();

                foreach (Line side in sides)
                {
                    Point? intPoint = Line.getIntersectionPoint(obseverLineOfSight, side);
                    if (intPoint != null)
                        intersectionPoints.Add(intPoint);
                }

                if (intersectionPoints.Count == 0)
                    continue;

                Point? nearestPoint = intersectionPoints[0];
                double shortestDistance = Line.getDistanceBetweenPoints((Point)observer.Location, (Point)nearestPoint);

                foreach (Point point in intersectionPoints)
                {
                    double distance = Line.getDistanceBetweenPoints((Point)observer.Location, point);
                    if (distance < shortestDistance)
                    {
                        nearestPoint = point;
                        shortestDistance = distance;
                    }
                }

                Point ratioOnScreen = GetRatioPositionOnScreen(target, (Point)nearestPoint);
                returnDevices.Add(target, ratioOnScreen);
            }

            return returnDevices;

        }

        /// <summary>
        /// Uses the the size and orientation of the device to compute all 4 line equations a device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns>The line euqations of all 4 sides of a device</returns>
        public List<Line> getLinesOfShape(Device device)
        {
            List<Line> returnLines = new List<Line>();
            List<Point> corners = getCornersOfShape(device);

            Line topSide = new Line(corners[0], corners[1]);
            Line rightSide = new Line(corners[1], corners[2]);
            Line bottomSide = new Line(corners[2], corners[3]);
            Line leftSide = new Line(corners[3], corners[0]);

            returnLines.Add(topSide);
            returnLines.Add(rightSide);
            returnLines.Add(bottomSide);
            returnLines.Add(leftSide);

            return returnLines;

        }

        /// <summary>
        /// Computes the position of all 4 corners of a device using size and orientation of a device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns>A list of points of all 4 corners of a device.</returns>
        public List<Point> getCornersOfShape(Device device)
        {
            List<Point> returnPoints = new List<Point>();
            List<Point> intPoints = new List<Point>();
            Point deviceLocation = device.Location.Value;

            intPoints.Add(new Point((double)(deviceLocation.X + device.Width / 2), (double)(deviceLocation.Y + device.Height / 2)));
            intPoints.Add(new Point((double)(deviceLocation.X + device.Width / 2), (double)(deviceLocation.Y - device.Height / 2)));
            intPoints.Add(new Point((double)(deviceLocation.X - device.Width / 2), (double)(deviceLocation.Y - device.Height / 2)));
            intPoints.Add(new Point((double)(deviceLocation.X - device.Width / 2), (double)(deviceLocation.Y + device.Height / 2)));

            foreach (Point point in intPoints)
            {
                double angle;
                // Check if the device's orientation is not null
                if (device.Orientation != null)
                { angle = (Double)device.Orientation * Math.PI / 180; }
                else { angle = 0; }
                double xValue = (point.X - deviceLocation.X) * Math.Cos(angle) - (point.Y - deviceLocation.Y) * Math.Sin(angle) + deviceLocation.X;
                double yValue = (point.Y - deviceLocation.Y) * Math.Cos(angle) + (point.X - deviceLocation.X) * Math.Sin(angle) + deviceLocation.Y;

                Point newPoint = new Point(xValue, yValue);
                returnPoints.Add(newPoint);
            }

            return returnPoints;
        }

        /// <summary>
        /// This function takes a point located in the room and turns it into an intersection point on the target device.
        /// For example if an intersection point is on the bottom left corner of a device, this function will return (0,0) or if 
        /// the intersection point is on the top right corner, it will return (1,1).
        /// </summary>
        /// <param name="target"></param>
        /// <param name="intersection"></param>
        /// <returns>A point </returns>
        public Point GetRatioPositionOnScreen(Device target, Point intersection){
            List<Point> cornersOfShape = getCornersOfShape(target);

            Double distance1 = Line.getDistanceBetweenPoints(intersection,cornersOfShape[0]);
            Double distance2 = Line.getDistanceBetweenPoints(intersection,cornersOfShape[1]);
            Double distance3 = Line.getDistanceBetweenPoints(cornersOfShape[0],cornersOfShape[1]);
            if (Math.Abs(distance3 - (distance1 + distance2)) < 0.01)
            {
                Double xRatio = 1;
                Double yRatio = distance1 / distance3;
                //Double xRatio = distance1 / distance3;
                //Double yRatio = 0;
                return new Point(xRatio, yRatio);
            }

            distance1 = Line.getDistanceBetweenPoints(intersection, cornersOfShape[2]);
            distance2 = Line.getDistanceBetweenPoints(intersection, cornersOfShape[1]);
            distance3 = Line.getDistanceBetweenPoints(cornersOfShape[1], cornersOfShape[2]);
            if (Math.Abs(distance3 - (distance1 + distance2)) < 0.01)
            {
                Double yRatio = 1;
                Double xRatio = distance1 / distance3;
                //Double xRatio = 1;
                //Double yRatio = distance2 / distance3;
                return new Point(xRatio, yRatio);
            }

            distance1 = Line.getDistanceBetweenPoints(intersection, cornersOfShape[3]);
            distance2 = Line.getDistanceBetweenPoints(intersection, cornersOfShape[2]);
            distance3 = Line.getDistanceBetweenPoints(cornersOfShape[2], cornersOfShape[3]);
            if (Math.Abs(distance3 - (distance1 + distance2)) < 0.01)
            {
                Double xRatio = 0;
                Double yRatio = distance1 / distance3;
                //Double xRatio = distance1 / distance3;
                //Double yRatio = 1;
                return new Point(xRatio, yRatio);
            }

            distance1 = Line.getDistanceBetweenPoints(intersection, cornersOfShape[3]);
            distance2 = Line.getDistanceBetweenPoints(intersection, cornersOfShape[0]);
            distance3 = Line.getDistanceBetweenPoints(cornersOfShape[3], cornersOfShape[0]);
            if (Math.Abs(distance3 - (distance1 + distance2)) < 0.01)
            {
                Double yRatio = 0;
                Double xRatio = distance1 / distance3;
                //Double xRatio = 0;
                //Double yRatio = distance2 / distance3;
                return new Point(xRatio, yRatio);
            }

            return new Point(-1, -1);
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
