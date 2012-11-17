using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace MSELocator
{
    public class Person
    {
        
        private String _HeldDeviceIdentifier;
        public String HeldDeviceIdentifier
        {
            get { return _HeldDeviceIdentifier; }
            set { _HeldDeviceIdentifier = value; }
        } 

        private Point? _Location;
        public Point? Location
        {
            get { return _Location; }
            set 
            { 
                _Location = value;
                if (LocationChanged != null)
                    LocationChanged(this);
            }
        }

        private double? _Orientation;
        public double? Orientation
        {
            get { return _Orientation; }
            set 
            {
                if (value.HasValue)
                {
                    _Orientation = Util.NormalizeAngle(value.Value);
                }
                else
                {
                    _Orientation = null;
                }

                if (OrientationChanged != null)
                    OrientationChanged(this);
            }
        }

        private String _Identifier;
        public String Identifier
        {
            get { return _Identifier; }
            set { _Identifier = value; }
        }

        public delegate void PersonEventSignature(Person sender);
        public event PersonEventSignature LocationChanged;
        public event PersonEventSignature OrientationChanged;

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
