using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IntAirAct;
using ZeroConf; 

namespace MSEKinect
{
    public class DeviceManager
    {
        IAIntAirAct ia;
        Room room;

        public DeviceManager(Room room) {
            this.room = room;
        }

        public void StartDeviceManager()
        {
            ia = new IAIntAirAct();
            ia.client = false;
            ia.capabilities.Add(new IACapability("PUT action/pairWith"));
            ia.capabilities.Add(new IACapability("PUT action/orientationUpdate"));
            ia.capabilities.Add(new IACapability("PUT /device/:identifier"));

            ia.AddMappingForClass(typeof(Orientation), "orientations"); 

            ia.deviceUpdateEventHandler += new ServiceUpdateEventHandler(DeviceListUpdated); 

            ia.Start();
        }

        public void StopDeviceManager()
        {
            ia.Stop();
        }

        internal void DeviceListUpdated(object sender, EventArgs e)
        {
            Console.WriteLine("Device List Updated");

            //Capture the updated devices from IntAirAct
            List<Device> updatedDevices = GetDevices(ia.devices);

            room.CurrentDevices = ProcessDevicesOnUpdated(updatedDevices, room.CurrentDevices, room.CurrentPersons);
 
        }

        private List<Device> ProcessDevicesOnUpdated(List<Device> updatedDevices, List<Device> currentDevices, List<Person> currentPersons)
        {

            var missing = from cd in currentDevices
                          where !updatedDevices.Contains(cd)
                          select cd;

            List<Device> missingDevices = missing.ToList<Device>();

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
        internal void ProcessMissingDevices(List<Device> missingDevices, List<Person> currentPersons)
        {
            foreach (Device d in missingDevices)
            {
                Person p = currentPersons.Find(person => person.Identifier.Equals(d.HeldByPersonIdentifier));

                if (p != null)
                {
                    //Remove the pairing information associated with the Person
                    p.HeldDeviceIdentifier = null;
                    p.PairingState = PairingState.NotPaired;
                }

                d.HeldByPersonIdentifier = null;
            }
        }

        List<Device> GetDevices(List<IADevice> updatedDevices)
        {
            List<Device> devices = new List<Device>(); 

            foreach (IADevice iad in updatedDevices) {

                //TODO Figure out if we should attach an IADevice
                //Create a new Device for each IADevice found
                Device d = new Device {
                    Identifier = iad.name 
                };

                devices.Add(d); 
            }

            return devices; 
        }
    }
}
