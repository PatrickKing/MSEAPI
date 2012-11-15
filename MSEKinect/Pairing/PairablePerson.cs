using System;
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

        private PairingState _pairingState;
        public PairingState PairingState
        {
            get { return _pairingState; }
            set
            {
                if (value == PairingState.PairingAttempt)
                {
                    pairingTimeoutTimer = new Timer(TIMEOUT_TIME);
                    pairingTimeoutTimer.Elapsed += pairingTimeout;
                    pairingTimeoutTimer.AutoReset = false;
                    pairingTimeoutTimer.Start();
                }
                _pairingState = value;
            }
        }


        private Timer pairingTimeoutTimer;

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


    }
}
