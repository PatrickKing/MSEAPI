using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MSEAPI_SharedNetworking
{
    /// <summary>
    /// The serializer doesn't properly handle System.Windows.Point, possibly because it is a struct and not an object.
    /// So, we convert Point to our IntermediatePoint for serialization.
    /// Sigh. 
    /// </summary>
    public class IntermediatePoint
    {
        public double X;
        public double Y;

        public IntermediatePoint()
        {

        }

        public IntermediatePoint(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }


        public IntermediatePoint(Point inPoint)
        {
            X = inPoint.X;
            Y = inPoint.Y;
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }


    }
}
