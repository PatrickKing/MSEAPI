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

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPointsTests()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0, 0), 45, 1, 1);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(3, 2.5), 0, 1, 5);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(2,2);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints3Tests_normalInput()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0, 0), 46, 1, 1);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(3, 3), 45, null, null); 

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(2, 2);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            Assert.IsTrue(dic.Count == 0);
            //Assert.AreEqual(dic[table].X, p.X, 0.01);
            //Assert.AreEqual(dic[table].Y, p.Y, 0.01);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPointsTests2()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            //Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0, 2.5), 0, 0.5, 0.5);
            //Device table = GenerateDeviceWithDimensions("TableTop", new Point(3, 2.5), 0, 1, 5);

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0,0), 45, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(5,5), 0, 2, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(4, 4);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            //Assert.IsTrue(dic.Count == 0);
            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPointsTests3()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(5, 0), 90, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(5, 5), 90, 2, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(5, 4);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);

            //Assert.IsTrue(dic.Count == 0);
            Assert.AreEqual(dic[table].X, p.X, 0.01);
            Assert.AreEqual(dic[table].Y, p.Y, 0.01);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPointsTests4()
        {
            //Create Locator with Devices which should intersect
            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0,0), 45, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(5,5), 90, 2, 2);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(0, 1);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);
            //List<Device> lis = locator.GetDevicesInView(iPad);

             //Assert.IsTrue(dic.Count == 0);
             Assert.AreEqual(dic[table].X, p.X, 0.01);
             Assert.AreEqual(dic[table].Y, p.Y, 0.01);
        }

        [TestMethod()]
        public void GetDevicesInViewWithIntersectionPoints_NullValuesInDevices()
        {

            Locator locator = new Locator();

            Device iPad = GenerateDeviceWithDimensions("iPad", new Point(0, 0), 90, 0.5, 0.5);
            Device table = GenerateDeviceWithDimensions("TableTop", new Point(0, 5), 0, null, null);

            locator.Devices.Add(iPad);
            locator.Devices.Add(table);

            Point p = new Point(5, 4);

            Dictionary<Device, Point> dic = locator.GetDevicesInViewWithIntersectionPoints(iPad);
            //List<Device> lis = locator.GetDevicesInView(iPad);

            Assert.IsTrue(dic.Count == 0);


        }

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

    }
}
