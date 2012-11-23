﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MSEKinect;
using MSELocator;
using System.Windows.Threading;

namespace RoomVisualizer
{
    /// <summary>
    /// Interaction logic for PersonControl.xaml
    /// </summary>
    public partial class PersonControl : UserControl
    {


        public PersonControl(PairablePerson pairablePerson)
        {
            InitializeComponent();

            //Setup Events
            pairablePerson.LocationChanged += OnLocationChanged;
            pairablePerson.OrientationChanged += OnOrientationChanged;
            pairablePerson.PairingStateChanged += OnPairingStateChanged;

            //Setup the person's 'dot'
            PersonEllipse.StrokeThickness = DrawingResources.STROKE_WIDTH;
            PersonEllipse.Stroke = DrawingResources.unpairedBrush;

            // Assuming people have a diameter of about 0.5m
            double personWidth = 0.5 * MainWindow.SharedCanvas.ActualWidth / DrawingResources.ROOM_WIDTH;
            PersonEllipse.Width = personWidth;
            PersonEllipse.Height = personWidth;

            
            //Setup the person's identifier
            IdentifierLabel.Content = pairablePerson.Identifier;
            IdentifierLabel.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            IdentifierLabel.Width = 50;
            Canvas.SetTop(IdentifierLabel, -5);
            Canvas.SetLeft(IdentifierLabel, -50);

            //Setup the person's identifier
            CoordinatesLabel.Content = pairablePerson.Identifier;
            CoordinatesLabel.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            CoordinatesLabel.Width = 50;
            Canvas.SetTop(CoordinatesLabel, 15);
            Canvas.SetLeft(CoordinatesLabel, -50);

        }


        public void OnOrientationChanged(Person person)
        {

        }

        public void OnLocationChanged(Person person)
        {
            if(person.Location.HasValue)
            {
                Point newPoint = DrawingResources.ConvertFromMetersToDisplayCoordinates(person.Location.Value, MainWindow.SharedCanvas);
                Canvas.SetLeft(this, newPoint.X);
                Canvas.SetTop(this, newPoint.Y);       
    
                CoordinatesLabel.Content = string.Format("({0:0.0},{1:0.0})", person.Location.Value.X, person.Location.Value.Y);
            }
        }


        public void OnPairingStateChanged(PairablePerson pairablePerson)
        {
            //Dispatch Brush Change to Main Thread
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {

                //Set Color of Ellipse to Appropriate Color
                PersonEllipse.Stroke = DrawingResources.GetBrushFromPairingState(pairablePerson.PairingState);
            }));
   
        }

    }
}
