﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSELocator;
using System.Timers;


namespace MSEKinect
{
    public class PairablePerson : Person
    {
        private const int TIMEOUT_TIME = 3000; // miliseconds

        // Time in milliseconds that the device will remain in the devicesBeingRemoved dictionary
        private const double SAVINGTIME = 5000;


        public delegate void PairablePersonEventSignature(PairablePerson sender);
        public event PairablePersonEventSignature PairingStateChanged;
        public event PairablePersonEventSignature CalibrationStateChanged;


        private PairingState _pairingState;
        public PairingState PairingState
        {
            get { return _pairingState; }
            set
            {
                //Pairing State is set to attempt, start timer to remove attempt after timeout
                if (value == PairingState.PairingAttempt)
                {
                    pairingTimeoutTimer = new Timer(TIMEOUT_TIME);
                    pairingTimeoutTimer.Elapsed += pairingTimeout;
                    pairingTimeoutTimer.AutoReset = false;
                    pairingTimeoutTimer.Start();
                }
                else if (value == PairingState.PairedButOccluded)
                {
                    occludedTimeoutTimer = new Timer(SAVINGTIME);
                    occludedTimeoutTimer.Elapsed += occludedTimeout;
                    occludedTimeoutTimer.AutoReset = false;
                    occludedTimeoutTimer.Start();
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

        public enum CallibrationState { UsedForCalibration, NotUsedCalibration };
        private CallibrationState _calibrationState;
        public CallibrationState CalibrationState
        {
            get { return _calibrationState; }
            set
            {
                _calibrationState = value;

                if (CalibrationStateChanged != null)
                {
                    CalibrationStateChanged(this);
                }
            }
        }

        private Timer pairingTimeoutTimer;
        private Timer occludedTimeoutTimer;

        public override string ToString()
        {
            return String.Format(
                "PairablePerson[Orientation: {0}, HeldDeviceIdentifier: {2}, PairingState: {3}]",
                Orientation,
                HeldDeviceIdentifier,
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

        private void occludedTimeout(Object sender, ElapsedEventArgs e)
        {
            if (_pairingState == PairingState.PairedButOccluded && sender == occludedTimeoutTimer)
            {
                this.PairingState = PairingState.NotPaired;
                occludedTimeoutTimer = null;
            }
        }


    }
}
