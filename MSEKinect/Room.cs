using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSEGestureRecognizer;
using System.Diagnostics;
using IntAirAct;
using RestSharp; 

namespace MSEKinect
{
    public class Room
    {

        private static TraceSource logger = new TraceSource("MSEKinect");

        private IAIntAirAct intAirAct;

        public Room(IAIntAirAct intAirAct)
        {
            CurrentPersons = new List<Person>();
            CurrentDevices = new List<Device>();

            this.intAirAct = intAirAct;
        }

        private List<Person> _currentPersons;
        public List<Person> CurrentPersons
        {
            get { return _currentPersons; }
            set { _currentPersons = value; }
        }

        private List<Device> _currentDevices;
        public List<Device> CurrentDevices
        {
            get { return _currentDevices; }
            set { _currentDevices = value; }
        }

        public void AttemptPairing()
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, "Attempting To Pair");
            AttemptPairing(CurrentDevices, CurrentPersons); 

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
        internal bool AttemptPairing(List<Device> devices, List<Person> persons)
        {

            //Find a device set to PairingAttempt
            Device pairingDevice = devices.Find(device => device.PairingState == PairingState.PairingAttempt);

            //Find a person set to PairingAttempt
            Person pairingPerson = persons.Find(person => person.PairingState == PairingState.PairingAttempt);

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
                return false; 
        }


        internal void Pair(Device pairingDevice, Person pairingPerson)
        {
            //Change the Pairing State 
            pairingDevice.PairingState = PairingState.Paired;
            pairingPerson.PairingState = PairingState.Paired; 

            //Create a Holds-Device and Held-By-Person Relationship
            pairingPerson.HeldDeviceIdentifier = pairingDevice.Identifier;
            pairingDevice.HeldByPersonIdentifier = pairingPerson.Identifier;

            List<IADevice> devices = intAirAct.devices;
            IADevice device = devices.Find(d => d.name == pairingDevice.Identifier);
            if (device != null)
            {
                IAAction action = new IAAction();
                action.action = "pairingSucceeded";
                intAirAct.CallAction(action, device);
            }

            logger.TraceEvent(TraceEventType.Information, 0, "Pairing Succeeded with Device {0} and Person {1}", pairingDevice.Identifier, pairingPerson.Identifier); 

        }


        /// <summary>
        /// This function is called when the Gesture Recognizer recognizes a Pair gesture. This function determines 
        /// which Person is performing this gesture, and changes the PairingState of that Person.
        /// </summary>
        /// <param name="sender"> The Gesture Recognizer</param>
        /// <param name="e"> The Gesture Event Arguments, which includes the tracking id</param>
        public void PersonPairGestureRecognized(object sender, GestureEventArgs e)
        {
            logger.TraceEvent(TraceEventType.Information, 0, "Person Wave Gesture Recognized"); 

            PersonPairGestureRecognized(e.TrackingId.ToString(), CurrentPersons); 
        }

        internal void PersonPairGestureRecognized(String SkeletonId, List<Person> Persons)
        {
            //Set the Person involved to the AttemptingPair state 
            Person p = Persons.Find(person => person.Identifier.Equals(SkeletonId));

            //Because the pairing can overlap, we do not want to reset a paired state 
            if (p != null && p.PairingState != PairingState.Paired)
            {
                p.PairingState = PairingState.PairingAttempt;
            }
            AttemptPairing();

            logger.TraceEvent(TraceEventType.Information, 0, "Kinect Wave Gesture Recognized");
        }


        public void DevicePairGestureRecognized(String DeviceId)
        {
            DevicePairGestureRecognized(DeviceId, CurrentDevices); 
        }
        internal void DevicePairGestureRecognized(String DeviceId, List<Device> Devices)
        {
            Device d = Devices.Find(device => device.Identifier.Equals(DeviceId));

            logger.TraceEvent(TraceEventType.Information, 0, "Device Wave Gesture Recognized");
            if (d != null && d.PairingState != PairingState.Paired)
            {
                d.PairingState = PairingState.PairingAttempt;
            }
            AttemptPairing(); 
        }
    }

}
