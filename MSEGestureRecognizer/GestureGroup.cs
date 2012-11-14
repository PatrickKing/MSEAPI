using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSEGestureRecognizer
{
     /// <summary>
     /// A GestureGroup is a set of gestures belonging to one kinect skeleton.
     /// </summary>
    public class GestureGroup
    {
        public List<Gesture> gestures;

        public GestureGroup()
        {
            gestures = new List<Gesture>();
        }

    }
}
