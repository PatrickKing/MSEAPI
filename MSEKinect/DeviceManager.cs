using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IntAirAct;
using ZeroConf;
using System.Diagnostics;
using MSELocator;

namespace MSEKinect
{
    public class DeviceManager
    {
        private static TraceSource logger = new TraceSource("MSEKinect")

        event ServiceUpdateEventHandler deviceUpdateEventHandler;
        IAIntAirAct ia;
        LocatorInterface locator;

        public DeviceManager(LocatorInterface locator, IAIntAirAct intAirAct) {
            this.locator = locator;
            this.ia = intAirAct;
        }

        public void StartDeviceManager()
        {
            deviceUpdateEventHandler = new ServiceUpdateEventHandler(DeviceListUpdated);
            ia.deviceUpdateEventHandler += deviceUpdateEventHandler;
        }

        public void StopDeviceManager()
        {
            ia.deviceUpdateEventHandler -= deviceUpdateEventHandler;
        }

        internal void DeviceListUpdated(object sender, EventArgs e)
        {
            logger.TraceEvent(TraceEventType.Information, 0, "Device List Updated");

            //Capture the updated devices from IntAirAct
            List<PairableDevice> updatedDevices = GetDevices(ia.devices);


            //Cast Into List Containing PairableDevices, PairablePersons
            List<PairablePerson> pairablePersons = locator.Persons.OfType<PairablePerson>().ToList<PairablePerson>(); 
            List<PairableDevice> pairableDevices = locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>(); 

            locator.Devices = ProcessDevicesOnUpdated(updatedDevices, pairableDevices, pairablePersons);
 
        }

        private List<Device> ProcessDevicesOnUpdated(List<PairableDevice> updatedDevices, List<PairableDevice> currentDevices, List<PairablePerson> currentPersons)
        {

            var missing = from cd in currentDevices
                          where !updatedDevices.Contains(cd)
                          select cd;

            List<PairableDevice> missingDevices = missing.ToList<PairableDevice>();

            ProcessMissingDevices(missingDevices, currentPersons); 

            var added = from ud in updatedDevices
                        where !currentDevices.Contains(ud)
                        select ud;

            List<Device> addedDevices = added.ToList<Device>();

            //ProcessAddedDevices(addedDevices); 

            List<Device> processedDevices = new List<Device>();
            processedDevices.AddRange(currentDevices);
            processedDevices.AddRange(addedDevices);
            processedDevices.RemoveAll(device => missingDevices.Contains(device));

            return processedDevices;           
        }

        //TODO Why is the self device missing?
        internal void ProcessMissingDevices(List<PairableDevice> missingDevices, List<PairablePerson> currentPersons)
        {
            foreach (Device d in missingDevices)
            {
                PairablePerson p = currentPersons.Find(person => person.Identifier.Equals(d.HeldByPersonIdentifier));

                if (p != null)
                {
                    //Remove the pairing information associated with the Person
                    p.HeldDeviceIdentifier = null;
                    p.PairingState = PairingState.NotPaired;
                }

                d.HeldByPersonIdentifier = null;
            }
        }

        List<PairableDevice> GetDevices(List<IADevice> updatedDevices)
        {
            List<PairableDevice> devices = new List<PairableDevice>(); 

            foreach (IADevice iad in updatedDevices) {

                //TODO Figure out if we should attach an IADevice
                //Create a new Device for each IADevice found
                PairableDevice d = new PairableDevice
                {
                    Identifier = iad.name, 
                    PairingState = PairingState.NotPaired
                };

                devices.Add(d); 
            }

            return devices; 
        }
    }
}
