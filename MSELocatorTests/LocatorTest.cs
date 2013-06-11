using MSELocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MSELocatorTests
{
    [TestClass]
    public class LocatorTest
    {
        #region Helper Functions

        public Device GenerateDevice(String id, Point loc, Double orn)
        {
            return new Device
            {
                Identifier = id,
                Location = loc,
                Orientation = orn
            }; 
        }

        public Device GenerateDeviceWithDimensions(String id, Point loc, Double? orn, Double? height, Double? width)
        {
            return new Device
            {
                Identifier= id,
                Location = loc, 
                Orientation = orn,
                Height = height,
                Width = width
            };
        }

        #endregion 

        #region GetDevicesInViewWithIntersectionPoint Tests

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_normalInput()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(3, 0), 70, 1, 1);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(4, 3), null, 1, 5);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(0.48,1);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_nullHeightAndWidth()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0, 0), 45, 1, 1);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(3, 3), 45, null, null); 

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.IsTrue(dic.Count == 0);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_lowerLeftCorner()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0,0), 45, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(5,5), null, 2, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(0, 1);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_upperLeftCorner()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0, 6), 315, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(6, 2), null, 2, 6);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(0, 0);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_lowerRightCorner()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(4,0), 135, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(2,2), null, 2, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(1, 1);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_upperRightCorner()
        {

            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(4, 4), 225, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(2, 2), null, 2, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(1, 0);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);


        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_belowCenter()
        {

            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(2, 0), 90, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(2, 2), null, 2, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(0.5, 1);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);


        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_aboveCenter()
        {

            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(2, 4), 270, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(2, 2), null, 2, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(0.5, 0);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);


        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_rightToCenter()
        {

            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(4, 2), 180, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(2, 2), null, 2.5, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(1, 0.5);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);


        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_leftToCenter()
        {

            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0, 2), 0, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(2, 2), null, 2.5, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(0, 0.5);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);


        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_55DegreesFromLowerLeft()
        {

            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(1, 0), 55, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(2, 2), null, 2.5, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(0.26, 1);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);


        }

        [TestMethod()]
        public void GetDevicesInFront()
        {

            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0, 0), 45, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(4, 4), 90, 2, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(1, 0);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic.Count , 0);
            //Assert.AreEqual(dic[table].X, p.X, 0.01);
            //Assert.AreEqual(dic[table].Y, p.Y, 0.01);


        }

        #endregion

        #region GetDevicesInView Tests

        [TestMethod()]
        public void GetDevicesInViewTest()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device observing = GenerateDevice("DeviceOne", new Point(10, 10), 90);
            Device observed = GenerateDevice("DeviceTwo", new Point(10, 20), 270);
            Device notObserved = GenerateDevice("DeviceThree", new Point(20, 20), 180);

            locator.Devices.Add(observing);
            locator.Devices.Add(observed);
            locator.Devices.Add(notObserved);

            //Compute intersecting devices for the observing device
            List<Device> actualIntersectingDevices = locator.GetDevicesInView(observing);

            //Confirm that observed is contained in the list of observed devices
            Assert.IsTrue(actualIntersectingDevices.Exists(d => d.Identifier == observed.Identifier), "Observed is Missing"); 

            //Confirm that notObserved is NOT contained in the list of observed devices
            Assert.IsFalse(actualIntersectingDevices.Exists(d => d.Identifier == notObserved.Identifier), "Not Observed is Present"); 
        }

        [TestMethod()]
        public void Angle45DegreesBetweenPointsTest()
        {
            double result = Util.AngleBetweenPoints(new Point(0, 0), new Point(1, 1));
            Assert.AreEqual(45.0, result, 0.001);
        }

        [TestMethod()]
        public void Angle135DegreesBetweenPointsTest()
        {
            double result = Util.AngleBetweenPoints(new Point(0, 0), new Point(-1, 1));
            Assert.AreEqual(135.0, result, 0.001);
        }

        [TestMethod()]
        public void Angle225DegreesBetweenPointsTest()
        {
            double result = Util.AngleBetweenPoints(new Point(0, 0), new Point(-1, -1));
            Assert.AreEqual(225.0, result, 0.001);
        }

        [TestMethod()]
        public void Angle315DegreesBetweenPointsTest()
        {
            double result = Util.AngleBetweenPoints(new Point(0, 0), new Point(1, -1));
            Assert.AreEqual(315.0, result, 0.001);
        }

        #endregion 
    }
}
