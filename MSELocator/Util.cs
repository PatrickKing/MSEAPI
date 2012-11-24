using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows;

namespace MSELocator
{
    public class Util
    {
        public const double DEFAULT_FIELD_OF_VIEW = 25.0; // degrees

        public const double RADIANS_TO_DEGREES = 180 / Math.PI;
        public const double DEGREES_TO_RADIANS = Math.PI / 180;
        
        /// <summary>
        /// Clamps angles so that they are in 0 &lt;= angle &lt; 360 degrees.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double NormalizeAngle(double value)
        {
            if (value >= 0)
            {
                return value % 360;
            }
            else
            {
                return (360 - Math.Abs((double)value % 360)) % 360;
            }
        }



        /// <summary>
        /// Translates coordinates from a custom 2D cartesian coordinate space back to the reference coordinate space. 
        /// </summary>
        /// <param name="inputCoordinate"></param>
        /// <param name="inputCoordinateSpaceRotation"></param>
        /// <param name="inputCoordinateSpaceTranslation"></param>
        /// <returns></returns>
        public static Vector TranslateFromCoordinateSpace(Vector inputCoordinate, double? inputCoordinateSpaceRotation, Vector inputCoordinateSpaceTranslation)
        {
            if (inputCoordinateSpaceRotation == null)
            {
                throw new ArgumentException("inputCoordinateSpaceRotation must not be null.");
            }

            double rotationAngle = 360 - (double)inputCoordinateSpaceRotation; // as we are rotating back to the room's coordinate space

            Matrix rotationMatrix = new Matrix(
                Math.Cos(rotationAngle * Math.PI / 180), -Math.Sin(rotationAngle * Math.PI / 180),
                Math.Sin(rotationAngle * Math.PI / 180), Math.Cos(rotationAngle * Math.PI / 180),
                0, 0);

            //First, we rotate the input coordinates, getting some intermediate coordinates
            Vector outputCoordinate = Vector.Multiply(inputCoordinate, rotationMatrix);

            //Second, we translate the intermediate coordinates
            outputCoordinate = new Vector(inputCoordinateSpaceTranslation.X + outputCoordinate.X, inputCoordinateSpaceTranslation.Y + outputCoordinate.Y);

            return outputCoordinate;
        }


        /// <summary>
        /// Calculates the distance between two points. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double DistanceBetweenPoints(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// Finds the angle (in degrees) formed by two points and the x axis.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>Angle in degrees.</returns>
        public static double AngleBetweenPoints(Point start, Point end)
        {
            double unnormalizedDegrees = Math.Atan2(end.Y - start.Y, end.X - start.X) * RADIANS_TO_DEGREES;
            return NormalizeAngle(unnormalizedDegrees);
        }

    }
}
