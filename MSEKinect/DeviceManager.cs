using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IntAirAct;
using System.Diagnostics;
using MSELocator;

namespace MSEKinect
{
    public class DeviceManager
    {
        private static TraceSource logger = new TraceSource("MSEKinect");

        IAIntAirAct ia;
        LocatorInterface locator;

        #region Constructor, Start and Stop

        public DeviceManager(LocatorInterface locator, IAIntAirAct intAirAct)
        {
            this.locator = locator;
            this.ia = intAirAct;
        }

        public void StartDeviceManager()
        {
            ia.DeviceFound += DeviceFound;
            ia.DeviceLost += DeviceLost;
            
        }

        public void StopDeviceManager()
        {
            ia.DeviceFound -= DeviceFound;
            ia.DeviceLost -= DeviceLost;
        }

        #endregion

        #region Device discovery and loss event handlers

        public void DeviceFound(IADevice iaDevice, bool ownDevice)
        {
            PairableDevice pairabledevice = new PairableDevice
            {
                Identifier = iaDevice.Name,
                PairingState = PairingState.NotPaired
            };

            locator.Devices.Add(pairabledevice);
            //TODO New device event notification goes here
        }

        public void DeviceLost(IADevice iaDevice)
        {
            List<PairablePerson> pairablePersons = locator.Persons.OfType<PairablePerson>().ToList<PairablePerson>();
            List<PairableDevice> pairableDevices = locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>();

            // Find and remove the pairable device from the Locator's list of devices
            PairableDevice pairableDevice = pairableDevices.Find(d => d.Identifier.Equals(iaDevice.Name));
            locator.Devices.Remove(pairableDevice);

            // If the device was paired, we mark its corresponding person unpaired.
            PairablePerson person = pairablePersons.Find(p => p.Identifier.Equals(pairableDevice.HeldByPersonIdentifier));
            if (person != null)
            {
                //Remove the pairing information associated with the Perpon
                person.HeldDeviceIdentifier = null;
                person.PairingState = PairingState.NotPaired;
            }
            pairableDevice.HeldByPersonIdentifier = null;

            //TODO Device removed event notfication goes here

        }

        #endregion








    }
}
