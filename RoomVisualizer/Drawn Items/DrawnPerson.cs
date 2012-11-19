using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSEKinect;
using MSELocator;

using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace RoomVisualizer
{
    /// <summary>
    /// Represents a person in the RoomVisualiser's canvas, gathers together several shapes used to represent it.  
    /// </summary>
    class DrawnPerson
    {
        #region Instance Variables
        Ellipse personDot;
        Line leftFieldOfViewLine;
        Line rightFieldOfViewLine;

        #endregion


        public DrawnPerson(PairablePerson pairablePerson)
        {
            pairablePerson.LocationChanged += onLocationChanged;
            pairablePerson.OrientationChanged += onOrientationChanged;
            pairablePerson.PairingStateChanged += onPairingStateChanged;

            Canvas canvas = MainWindow.SharedCanvas;

            personDot = new Ellipse();
            personDot.Width = DrawingResources.DOT_WIDTH;
            personDot.Height = DrawingResources.DOT_WIDTH;
            personDot.Stroke = DrawingResources.unpairedBrush;
            personDot.Fill = Brushes.White;
            personDot.StrokeThickness = DrawingResources.STROKE_WIDTH;

            //Canvas.SetTop(personDot, pairablePerson.Location.Value.Y);

            leftFieldOfViewLine = new Line();

            rightFieldOfViewLine = new Line();

            canvas.Children.Add(personDot);

        }

        public void onOrientationChanged(Person person)
        {


        }

        public void onLocationChanged(Person person)
        {

        }

        public void onPairingStateChanged(PairablePerson pairablePerson)
        {


        }
    }
}
