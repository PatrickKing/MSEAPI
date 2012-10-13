﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSEKinect
{ 
    public class Device
    {
        private double _Orientation;
        public double Orientation
        {
            get { return _Orientation; }
            set { _Orientation = value; }
        }
        
        private PairingState _pairingState;
        public PairingState PairingState
        {
            get { return _pairingState; }
            set { _pairingState = value; }
        }

        private String _HeldByPersonIdentifier;
        public String HeldByPersonIdentifier
        {
            get { return _HeldByPersonIdentifier; }
            set { _HeldByPersonIdentifier = value; }
        } 

        private String _Identifier;
        public String Identifier
        {
            get
            {
                return _Identifier;
            }
            set
            {
                _Identifier = value;
            }
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            Device d = obj as Device;
            
            if ((System.Object)d == null)
            {
                return false;
            }

            return (this.Identifier == d.Identifier);
        }


        public bool Equals(Device d)
        {
            return (d.Identifier == this.Identifier);
        }

        public override string ToString()
        {
            return String.Format("Device[Orientation: {0}, PairingState: {1}, HeldByPersonIdentifier: {2}]", Orientation, PairingState, HeldByPersonIdentifier);
        }

    }
}
