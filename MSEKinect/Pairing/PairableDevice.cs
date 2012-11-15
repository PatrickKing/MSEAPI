using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSELocator;
using System.Runtime.CompilerServices;
using System.Timers;

[assembly: InternalsVisibleTo("MSEKinectTests")]

namespace MSEKinect
{

    public class PairableDevice : Device
    {
        private PairingState _pairingState;
        private Timer timer;

        // Time in ms before the PairingState gets reset from PairingAttempt to NotPaired
        const int resetTime = 5000;

        public PairableDevice() {

            // Creates a new timer and attaches an event handler.
            timer = new Timer(resetTime);
            timer.Elapsed += new ElapsedEventHandler(resetPairingState);

            // Will only trigger the Elapsed event once per time Start() is called
            timer.AutoReset = false;
        }

        public PairingState PairingState
        {
            get { return _pairingState; }
            set {
                _pairingState = value;

                // If we try to set PairingState to PairingAttempt, start the timer
                if (value == PairingState.PairingAttempt)
                {                  
                    timer.Start();               
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

        private void resetPairingState(object source, ElapsedEventArgs e)
        {
            // This function gets called resetTime milliseconds after timer.Start() is called.
            timer.Stop();

            // If we have not paired yet, reset the PairingState to NotPaired
            if (this.PairingState != PairingState.Paired)
                this.PairingState = PairingState.NotPaired;
        }
        
    }
}
