using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

using MSEKinect;

namespace RoomVisualizer
{
    public class DrawingResources
    {
        public static Brush unpairedBrush = Brushes.DarkRed;
        public static Brush pairingAttemptBrush = Brushes.Orange;
        public static Brush pairedBrush = Brushes.Green;

        public const int DOT_WIDTH = 20;
        public const int SQUARE_LENGTH = 20;
        public const int STROKE_WIDTH = 4;

        public const int DEVICE_FOV_WIDTH = 1;
        public const int TRACKER_FOV_WIDTH = 2;

        public const double DEVICE_FOV_LENGTH = 100;
        public const double TRACKER_FOV_LENGTH = 760;


        public const double ROOM_WIDTH = 4.5;
        public const double ROOM_HEIGHT = 4.5;

        //Utility Function - Coverting from Meters into Pixels
        public static Point ConvertFromMetersToDisplayCoordinates(Point myPoint, Canvas canvas)
        {
            return new Point(myPoint.X * canvas.ActualWidth / ROOM_WIDTH, canvas.ActualHeight - (myPoint.Y * canvas.ActualHeight / ROOM_HEIGHT));
        }

        public static Point ConvertFromDisplayCoordinatesToMeters(Point myPoint, Canvas canvas)
        {
            return new Point( (myPoint.X * ROOM_WIDTH) / canvas.ActualWidth, ((canvas.ActualHeight - myPoint.Y) * ROOM_WIDTH / canvas.ActualHeight));
        }

        /// <summary>
        /// Converts from meters on the X axis to pixels
        /// </summary>
        /// <param name="x"></param>
        /// <param name="canvas"></param>
        /// <returns></returns>
        public static double ConvertFromMetersToPixelsX(double x, Canvas canvas)
        {
            return x * canvas.ActualWidth / ROOM_WIDTH;
        }

        /// <summary>
        /// Converts from meters on the Y axis to pixels
        /// </summary>
        /// <param name="y"></param>
        /// <param name="canvas"></param>
        /// <returns></returns>
        public static double ConvertFromMetersToPixelsY(double y, Canvas canvas)
        {
            return y * canvas.ActualHeight / ROOM_HEIGHT;
        }

        // Takes a Point and a Length and returns a Point on the line between point and origin (0,0) with the specified length
        public static Point ConvertPointToProperLength(Point point, double length)
        {
            double t = Math.Sqrt( Math.Pow(length,2)  / ( Math.Pow(point.X,2) + Math.Pow(point.Y,2) ));
            return new Point(point.X * t, point.Y * t);
        }



        /// <summary>
        /// Converts a pairing state to a Brush. This is useful so that if we want to change the color scheme for different states, we only need to do it here.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static Brush GetBrushFromPairingState(PairingState state)
        {
            switch (state)
            {
                case (PairingState.NotPaired): return Brushes.DarkRed;
                case (PairingState.PairingAttempt): return Brushes.Orange;
                case (PairingState.Paired): return Brushes.Green;
                default: return Brushes.White;
            }
        }

        // Adds Gridlines based on the width and height of the canvas onto the Grid gridLines
        public static void GenerateGridLines(Canvas canvas, Grid gridLines, double scale)
        {
            gridLines.ColumnDefinitions.Clear();
            gridLines.RowDefinitions.Clear();

            // Finds out how many pixels 1 meter is
            double gridWidth = DrawingResources.ConvertFromMetersToDisplayCoordinates(new Point(scale, scale), canvas).X;

            // Calculates the number of rows and columns to add
            double numberOfLinesWide = canvas.ActualWidth / gridWidth;
            double numberOfLinesHigh = canvas.ActualHeight / gridWidth;

            // Generate Columns
            for (int i = 0; i < numberOfLinesWide; i++)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                columnDefinition.Width = new GridLength(gridWidth);
                gridLines.ColumnDefinitions.Add(columnDefinition);
            }

            // Generate Rows
            for (int i = 0; i < numberOfLinesHigh; i++)
            {
                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.Height = new GridLength(gridWidth);
                gridLines.RowDefinitions.Add(rowDefinition);
            }
        }


    }
}
