using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSELocator;

namespace MSEKinect
{
    public class PairablePerson : Person
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
                "PairablePerson[Orientation: {0}, HeldDeviceIdentifier: {2}, PairingState: {3}]",
                Orientation,
                HeldDeviceIdentifier,
                PairingState);
        }

    }
}
