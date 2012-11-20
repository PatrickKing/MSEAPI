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


        public const double ROOM_WIDTH = 4.5;
        public const double ROOM_HEIGHT = 4.5;

        //Utility Function - Coverting from Meters into Pixels
        public static Point ConvertFromMetersToDisplayCoordinates(Point myPoint, Canvas canvas)
        {
            return new Point(myPoint.X * canvas.ActualWidth / ROOM_WIDTH, canvas.ActualHeight - (myPoint.Y * canvas.ActualHeight / ROOM_HEIGHT));
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


    }
}
