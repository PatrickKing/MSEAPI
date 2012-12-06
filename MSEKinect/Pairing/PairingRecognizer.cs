using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSEGestureRecognizer;
using System.Diagnostics;
using IntAirAct;
using MSELocator;

using MSEAPI_CS_Routes;

namespace MSEKinect
{
    public class PairingRecognizer
    {
        private static TraceSource logger = new TraceSource("MSEKinect");

        private IAIntAirAct intAirAct;
        private LocatorInterface locator;

        public PairingRecognizer(LocatorInterface locator, IAIntAirAct intAirAct)
        {
            this.locator = locator;
            this.intAirAct = intAirAct;
        }

        private void AttemptPairing()
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, "Attempting To Pair");
            
            List<PairableDevice> pDevices = locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>();
            List<PairablePerson> pPersons = locator.Persons.OfType<PairablePerson>().ToList<PairablePerson>();

            AttemptPairing(pDevices, pPersons); 
        }

        //TODO Deal with the cause where more then two Devices (or persons) are Attempting to Pair
        //TODO Consider how the timer 
        /// <summary>
        /// This function attempts to pair a Person and a Device. This occurs if both have had their states 
        /// sets to 'PairingAttempt'. 
        /// </summary>
        /// <param name="devices"> The list of current devices</param>
        /// <param name="persons"> The list of current persons</param>
        /// <returns></returns>
        internal bool AttemptPairing(List<PairableDevice> devices, List<PairablePerson> persons)
        {
            //Find a device set to PairingAttempt
            PairableDevice pairingDevice = devices.Find(device => device.PairingState == PairingState.PairingAttempt);

            //Find a person set to PairingAttempt
            PairablePerson pairingPerson = persons.Find(person => person.PairingState == PairingState.PairingAttempt);

            //Check Device & Person 
            if (pairingDevice == null)
            {
                logger.TraceEvent(TraceEventType.Error, 0, "Cannot Pair Because No Device Is Marked For Pairing");
            }

            if (pairingPerson == null)
            {
                logger.TraceEvent(TraceEventType.Error, 0, "Cannot Pair Because No Person Is Marked For Pairing");
            }

            //Debug Intermittent Failure 
            String deviceExists = (pairingDevice != null) ? "Pairing Device" : "Null";
            String personExists = (pairingPerson != null) ? "Pairing Person" : "Null";
            
            logger.TraceEvent(TraceEventType.Verbose, 0, "Device: {0} and Person: {1}", deviceExists, personExists);

            if (pairingDevice != null && pairingPerson != null)
            {
                Pair(pairingDevice, pairingPerson);
                return true;
            }
            else
            {
                return false;
            }
        }


        internal void Pair(PairableDevice pairingDevice, PairablePerson pairingPerson)
        {
            //Change the Pairing State 
            pairingDevice.PairingState = PairingState.Paired;
            pairingPerson.PairingState = PairingState.Paired; 

            //Create a Holds-Device and Held-By-Person Relationship
            pairingPerson.HeldDeviceIdentifier = pairingDevice.Identifier;
            pairingDevice.HeldByPersonIdentifier = pairingPerson.Identifier;

            List<IADevice> devices = intAirAct.Devices;
            IADevice device = devices.Find(d => d.Name == pairingDevice.Identifier);
            if (device != null)
            {
                //TODO: Verify that this works at all! 
                IARequest request = new IARequest(Routes.BecomePairedRoute); 
                intAirAct.SendRequest(request, device);
                
            }

            logger.TraceEvent(TraceEventType.Information, 0, "Pairing Succeeded with Device {0} and Person {1}", pairingDevice.Identifier, pairingPerson.Identifier); 

        }


        /// <summary>
        /// This function is called when the Gesture Recognizer recognizes a Pair gesture. This function determines 
        /// which Person is performing this gesture, and changes the PairingState of that Person.
        /// </summary>s
        /// <param name="sender"> The Gesture Recognizer</param>
        /// <param name="e"> The Gesture Event Arguments, which includes the tracking id</param>
        public void PersonPairAttempt(object sender, GestureEventArgs e)
        {
            logger.TraceEvent(TraceEventType.Information, 0, "Person Wave Gesture Recognized");

            //Cast Into List Containing PairablePersons
            List<PairablePerson> pairablePersons = locator.Persons.OfType<PairablePerson>().ToList<PairablePerson>(); 

            PersonPairAttempt(e.TrackingId.ToString(), pairablePersons); 
        }

        internal void PersonPairAttempt(String SkeletonId, List<PairablePerson> Persons)
        {
            //Set the Person involved to the AttemptingPair state 
            PairablePerson p = Persons.Find(person => person.Identifier.Equals(SkeletonId));

            //Because the pairing can overlap, we do not want to reset a paired state 
            if (p != null && p.PairingState != PairingState.Paired)
            {
                p.PairingState = PairingState.PairingAttempt;
            }
            AttemptPairing();

            logger.TraceEvent(TraceEventType.Information, 0, "Kinect Wave Gesture Recognized");
        }

        public void DevicePairAttempt(String DeviceId)
        {
            List<PairableDevice> pairableDevices = locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>(); 

            DevicePairAttempt(DeviceId, pairableDevices); 
        }

        internal void DevicePairAttempt(String DeviceId, List<PairableDevice> Devices)
        {
            PairableDevice d = Devices.Find(device => device.Identifier.Equals(DeviceId));

            logger.TraceEvent(TraceEventType.Information, 0, "Device Wave Gesture Recognized");
            if (d != null && d.PairingState != PairingState.Paired)
            {
                d.PairingState = PairingState.PairingAttempt;
            }
            AttemptPairing(); 
        }
    }
}
