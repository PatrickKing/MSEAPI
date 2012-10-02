using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSEKinect
{
    public class Person
    {
        private PairingState _pairingState;
        public PairingState PairingState
        {
            get { return _pairingState; }
            set { _pairingState = value; }
        }

        private String _HeldDeviceIdentifier;
        public String HeldDeviceIdentifier
        {
            get { return _HeldDeviceIdentifier; }
            set { _HeldDeviceIdentifier = value; }
        } 

        private Location _Location;
        public Location Location
        {
            get { return _Location; }
            set { _Location = value; }
        }

        private Orientation _Orientation;
        public Orientation Orientation
        {
            get { return _Orientation; }
            set { _Orientation = value; }
        }

        private String _Identifier;
        public String Identifier
        {
            get { return _Identifier; }
            set { _Identifier = value; }
        }

        /// <summary>
        /// This method overrrides the equals, to allow comparison of Persons, based on their identifier.
        /// If 2 different Persons have the same identifier, then the Persons are equal.
        /// </summary>
        /// <param name="obj">Object (or Person) to compare with </param>
        /// <returns>True or False after comparing the identifiers</returns>
        public override bool Equals(object obj)
        {
            //If object is null
            if (obj == null)
                return false;

            Person p = obj as Person;
            if ((System.Object)p == null)
            {
                return false;
            }

            //Return comparison of the identifiers
            return (p.Identifier == this.Identifier);
        }

        /// <summary>
        /// Type comparison of Person's object, at a class level. 
        /// If 2 different Persons have the same identifier, then the Persons are equal.
        /// </summary>
        /// <param name="p">Person to compare with</param>
        /// <returns>True or False after comparing the identifiers</returns>
        public bool Equals(Person p)
        {
            //If the parameter is null, return false
            if ((System.Object)p == null)
            {
                return false;
            }
            //Return comparison of the identifiers
            return (p.Identifier == this.Identifier);
        }

    }
}
