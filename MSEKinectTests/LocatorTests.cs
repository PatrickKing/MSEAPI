using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSELocator;
using MSEKinect;

namespace MSEKinectTests
{
    [TestClass]
    public class LocatorTests
    {
        [TestMethod]
        public void CastingTest()
        {

            //Create A List of Pairable Devices 
            List<Device> listOfDevices = new List<Device>(); 

            //Add Devices to List 
            String testIdentifier = "ChrisBurnsiPhone"; 
            PairableDevice pairableDevice = new PairableDevice 
            {
                Identifier = testIdentifier, 
                Orientation = null, 
                PairingState = PairingState.NotPaired
            };

            listOfDevices.Add(pairableDevice); 

            //Cast the List Using the OfType Operator
            List<PairableDevice> listOfPD = listOfDevices.OfType<PairableDevice>().ToList<PairableDevice>();

            Assert.AreEqual(pairableDevice, listOfPD[0]); 


        }
    }
}
