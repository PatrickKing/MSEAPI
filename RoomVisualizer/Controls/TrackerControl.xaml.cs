using System;
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
using MSELocator;

namespace RoomVisualizer
{
    /// <summary>
    /// Interaction logic for TrackerControl.xaml
    /// </summary>
    /// 

    public partial class TrackerControl : UserControl
    {

        DeviceRotationControl deviceRotationControl;

        private enum DisplayState
        {
            OnStackPanel,
            OnCanvas           
        }
        private DisplayState myDisplayState;
        private DisplayState MyDisplayState
        {
            get
            {
                return myDisplayState;
            }
            set
            {
                if (value == DisplayState.OnCanvas && myDisplayState == DisplayState.OnStackPanel)
                {
                    //Handle transition to display on Canvas
                    MainWindow.SharedDeviceStackPanel.Children.Remove(this);
                    MainWindow.KinectWrapPanel.Children.Remove(this);
                    //formatForCanvas();
                    MainWindow.SharedCanvas.Children.Add(this);
                }
                else if (value == DisplayState.OnStackPanel && myDisplayState == DisplayState.OnCanvas)
                {
                    //Handle transition to display on StackPanel
                    MainWindow.SharedCanvas.Children.Remove(this);
                    formatForStackPanel();
                    MainWindow.KinectWrapPanel.Children.Add(this);
                }
            }
        }

        private Tracker _Tracker;
        public Tracker Tracker
        {
            set { }
            get { return _Tracker; }

        }

        public TrackerControl(Tracker tracker)
        {
            this._Tracker = tracker;

            InitializeComponent();

            deviceRotationControl = new DeviceRotationControl();
            deviceRotationControl.onSliderValueChanged += new EventHandler<RotationSliderEventArgs>(onOrientationSliderChanged);
            canvas.Children.Add(deviceRotationControl);
            Canvas.SetLeft(deviceRotationControl, -150);
            Canvas.SetTop(deviceRotationControl, -10);
            deviceRotationControl.Opacity = 0;

            TrackerNameLabel.Text = tracker.Identifier;
            LeftLine.StrokeThickness = DrawingResources.TRACKER_FOV_WIDTH;
            RightLine.StrokeThickness = DrawingResources.TRACKER_FOV_WIDTH;

            tracker.LocationChanged += onLocationChanged;
            tracker.OrientationChanged += onOrientationChanged;
            tracker.FOVChanged += onFOVChanged;
            tracker.RangeChanged += onRangeChanged;

            //formatForStackPanel();
        }

        private void formatForStackPanel()
        {
            NearTriangle.Visibility = System.Windows.Visibility.Hidden;
            LeftLine.Visibility = System.Windows.Visibility.Hidden;
            RightLine.Visibility = System.Windows.Visibility.Hidden;
            deviceRotationControl.Visibility = System.Windows.Visibility.Hidden;
            FarLine.Visibility = System.Windows.Visibility.Hidden;
            TrackerEllipse.Margin = new Thickness(50, 50, 0, 0);
        }


        void onOrientationSliderChanged(object sender, RotationSliderEventArgs e)
        {
            this.Tracker.Orientation = e.Time;
        }

        private void Shape_MouseEnter(object sender, MouseEventArgs e)
        {
            deviceRotationControl.Opacity = 1;
        }

        private void Shape_MouseLeave(object sender, MouseEventArgs e)
        {
            deviceRotationControl.Opacity = 0;
        }

        #region Drag and Drop

        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);

            startDragging();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            //if (e.OriginalSource == DeviceRectangle)
            {
                base.OnMouseDown(e);

                // We consider it a drag only if the Device is a stationary Device, and the mouse button is pushed
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    startDragging();
                }
            }
        }


        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);

            if (e.Effects.HasFlag(DragDropEffects.Move))
            {
                Mouse.SetCursor(Cursors.Pen);

            }
            else
            {
                Mouse.SetCursor(Cursors.No);
            }

            e.Handled = true;
        }

        private void startDragging()
        {
            // Drag event started on a device supporting setting location
            DataObject data = new DataObject();
            data.SetData("trackerControl", this);

            // Start Dragging
            DragDrop.DoDragDrop(this, data, DragDropEffects.Move);

        }
        #endregion


        public void onOrientationChanged(Device device)
        {
            // We are using RotateTransform now to make things easier. Everything should be drawn pointing downwards (270 degrees);
            LeftLine.RenderTransform = new RotateTransform((device.Orientation.Value * -1) + 270, 50, 15);
            RightLine.RenderTransform = new RotateTransform((device.Orientation.Value * -1) + 270, 50, 15);
            NearTriangle.RenderTransform = new RotateTransform((device.Orientation.Value * -1) + 270, 50, 15);
            FarLine.RenderTransform = new RotateTransform((device.Orientation.Value * -1) + 270, 50, 15);

        } 

        public void onLocationChanged(Device device)
        {
            if (device.Location.HasValue)
            {
                Point newPoint = DrawingResources.ConvertFromMetersToDisplayCoordinates(device.Location.Value, MainWindow.SharedCanvas);
                Canvas.SetLeft(this, newPoint.X);
                Canvas.SetTop(this, newPoint.Y);
            }
        }

        public void onRangeChanged(Tracker tracker)
        {
            updateRange(tracker);
        }

        public void onFOVChanged(Device device)
        {
            Tracker tracker = (Tracker)device;
            updateFOV(tracker);
            updateRange(tracker);
        }

        /// <summary>
        /// This updates the FOV draw lines for a given tracker
        /// </summary>
        /// <param name="tracker"></param>
        public void updateFOV(Tracker tracker)
        {
            double FOVAngle = tracker.FieldOfView.Value / 2;
            double leftX = (tracker.MaxRange.Value * Math.Sin(Util.DEGREES_TO_RADIANS * FOVAngle)) / (Util.DEGREES_TO_RADIANS * (180.0 - FOVAngle - 90));
            double leftXPixels = DrawingResources.ConvertFromMetersToPixelsX(leftX, MainWindow.SharedCanvas);
            double YPixels = DrawingResources.ConvertFromMetersToPixelsY(tracker.MaxRange.Value, MainWindow.SharedCanvas);

            RightLine.X2 = 50 + leftXPixels;
            RightLine.Y2 = YPixels;
            LeftLine.X2 = 50-leftXPixels;
            LeftLine.Y2 = YPixels;

            //double topAngle = Util.NormalizeAngle(270 + FOVAngle);
            //double topX = Math.Cos(topAngle * Math.PI / 180);
            //double topY = Math.Sin(topAngle * Math.PI / 180);            
            //Point newLeft = DrawingResources.ConvertPointToProperLength(new Point(topX, topY), DrawingResources.TRACKER_FOV_LENGTH);
            //LeftLine.X2 = newLeft.X;
            //LeftLine.Y2 = -newLeft.Y;

            //double bottomAngle = Util.NormalizeAngle(270 - FOVAngle);
            //double bottomX = Math.Cos(bottomAngle * Math.PI / 180);
            //double bottomY = Math.Sin(bottomAngle * Math.PI / 180);            
            //Point newRight = DrawingResources.ConvertPointToProperLength(new Point(bottomX, bottomY), DrawingResources.TRACKER_FOV_LENGTH);
            //RightLine.X2 = newRight.X;
            //RightLine.Y2 = -newRight.Y;            
        }


        /// <summary>
        /// This updates the Range drawing for a given tracker
        /// </summary>
        /// <param name="tracker"></param>
        public void updateRange(Tracker tracker)
        {
            double FOVAngle = tracker.FieldOfView.Value / 2;

            // We want to adjust the MinRange triangle, so we're using sin law
            if (tracker.MinRange.HasValue)
            {
                double leftX = (tracker.MinRange.Value * Math.Sin(Util.DEGREES_TO_RADIANS * FOVAngle)) / (Util.DEGREES_TO_RADIANS * (180.0 - FOVAngle - 90));
                double leftXPixels = DrawingResources.ConvertFromMetersToPixelsX(leftX, MainWindow.SharedCanvas);
                double YPixels = DrawingResources.ConvertFromMetersToPixelsY(tracker.MinRange.Value, MainWindow.SharedCanvas);

                NearTriangle.Points.Clear();

                NearTriangle.Points.Add(new Point(50, 15));
                NearTriangle.Points.Add(new Point(50 + leftXPixels, 15+YPixels));
                NearTriangle.Points.Add(new Point(50 - leftXPixels, 15+YPixels));
            }

            // Adjusting the FarLine
            if (tracker.MaxRange.HasValue)
            {
                double leftX = (tracker.MaxRange.Value * Math.Sin(Util.DEGREES_TO_RADIANS * FOVAngle)) / (Util.DEGREES_TO_RADIANS * (180.0 - FOVAngle - 90));
                double leftXPixels = DrawingResources.ConvertFromMetersToPixelsX(leftX, MainWindow.SharedCanvas);
                double YPixels = DrawingResources.ConvertFromMetersToPixelsY(tracker.MaxRange.Value, MainWindow.SharedCanvas);

                FarLine.X1 = 50+ leftXPixels;
                FarLine.Y1 = YPixels;
                FarLine.X2 = 50-leftXPixels;
                FarLine.Y2 = YPixels;
            }
        }

        /// <summary>
        /// Hides the Range drawing
        /// </summary>
        public void hideRange()
        {
            FarLine.Visibility = Visibility.Hidden;
            NearTriangle.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Shows the Range drawing
        /// </summary>
        public void showRange()
        {
            FarLine.Visibility = Visibility.Visible;
            NearTriangle.Visibility = Visibility.Visible;
        }

    }
}
