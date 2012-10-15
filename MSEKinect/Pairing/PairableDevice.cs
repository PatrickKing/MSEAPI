using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSELocator;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MSEKinectTests")]

namespace MSEKinect
{

    internal class PairableDevice : Device
    {
        private PairingState _pairingState;
        public PairingState PairingState
        {
            get { return _pairingState; }
            set { _pairingState = value; }
        }


        public override string ToString()
        {
            return String.Format(
                "PairableDevice[Orientation: {0}, HeldByPersonIdentifier: {2}, PairingState: {3}]",
                Orientation,
                HeldByPersonIdentifier,
                PairingState); 
        }
        
    }
}
