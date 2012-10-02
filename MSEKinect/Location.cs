using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSEKinect
{
    public class Location
    {
 
        private double _x;
        public double X
        {
            get{ return _x; }
            set{_x = value;}
        }

        private double _y;
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }
    }
}
