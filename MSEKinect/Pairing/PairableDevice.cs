using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSELocator;
using System.Runtime.CompilerServices;
using System.Timers;
using MSEAPI_SharedNetworking;

[assembly: InternalsVisibleTo("MSEKinectTests")]

namespace MSEKinect
{

    public class PairableDevice : Device
    {
        #region Instance Variables

        private const int TIMEOUT_TIME = 3000; // miliseconds

        public delegate void PairableDeviceEventSignature(PairableDevice sender);
        public event PairableDeviceEventSignature PairingStateChanged;
        
        private PairingState _pairingState;
        private Timer pairingTimeoutTimer;

        #endregion

        public PairingState PairingState
        {
            get { return _pairingState; }
            set 
            {   
                //Pairing State is set to attempt, start timer to remove attempt after timeou
                if (value == PairingState.PairingAttempt)
                {
                    pairingTimeoutTimer = new Timer(TIMEOUT_TIME);
                    pairingTimeoutTimer.Elapsed += pairingTimeout;
                    pairingTimeoutTimer.AutoReset = false;
                    pairingTimeoutTimer.Start();
                }

                //Update the Pairing State Value
                _pairingState = value;

                //If event has subscribers, then call the event
                if (PairingStateChanged != null)
                {
                    PairingStateChanged(this);
                }

            
            }
        }

        public override string ToString()
        {
            return String.Format(
                "PairableDevice[Orientation: {0}, HeldByPersonIdentifier: {1}, PairingState: {2}]",
                Orientation,
                HeldByPersonIdentifier,
                PairingState); 
        }

        /// <summary>
        /// Handler for the pairing timeout timer, which sets us to 'not pairing' if we haven't paired yet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pairingTimeout(Object sender, ElapsedEventArgs e)
        {
            // Only reset our pairing state if we haven't paired, and if this timer is the most recent timer created
            if (_pairingState == PairingState.PairingAttempt && sender == pairingTimeoutTimer)
            {
                this.PairingState = PairingState.NotPaired;
                pairingTimeoutTimer = null;
            }
        }







        #region Serialization

        // For transmission, we create objects with an anonymous type where the instance variable names precisely match the ones on iOS.
        // ie, identifier instead of Identifier
        // This makes deserialization on the client easier.
        public static List<IntermediateDevice> GetCompleteIntermediateDevicesList(List<Device> devices)
        {
            List<IntermediateDevice> intermediateDevices = new List<IntermediateDevice>();
            foreach (Device device in devices)
            {
                IntermediateDevice intermediateDevice = new IntermediateDevice
                {
                    orientation = device.Orientation,
                    identifier = device.Identifier,
                    location = device.Location
                };

                if (intermediateDevice.isComplete)
                {
                    intermediateDevices.Add(intermediateDevice);
                }

            }

            return intermediateDevices;
        }


        public static IntermediateDevice GetCompleteIntermediateDevice(Device device)
        {
            if (device == null)
            {
                return null;
            }
            IntermediateDevice intermediateDevice = new IntermediateDevice();

            intermediateDevice.identifier = device.Identifier;
            intermediateDevice.orientation = device.Orientation;
            intermediateDevice.location = device.Location;

            // We only want to return devices for which all of the properties are known
            if (intermediateDevice.isComplete)
                return intermediateDevice;
            else
                return null;
        }



        #endregion







    }
}
