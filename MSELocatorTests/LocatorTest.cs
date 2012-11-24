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
        public void AngleBetweenPointsTest()
        {
            double result = Util.AngleBetweenPoints(new Point(0, 0), new Point(1, 1));

            Assert.AreEqual(45.0, result, 0.001);
        }

    }
}
