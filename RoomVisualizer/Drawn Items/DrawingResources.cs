using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;

namespace RoomVisualizer
{
    public class DrawingResources
    {
        public static Brush unpairedBrush = Brushes.DarkRed;
        public static Brush pairingAttemptBrush = Brushes.Orange;
        public static Brush pairedBrush = Brushes.Green;

        public static const int DOT_WIDTH = 20;
        public static const int STROKE_WIDTH = 2;


    }
}
