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
using System.Timers;
using System.Windows.Threading;
using IntAirAct;

using MSEKinect;
using MSELocator;

namespace RoomVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Shared accessor for the window's canvas object.
        /// </summary>
        public static Canvas SharedCanvas
        {
            get { return sharedCanvas; }
        }
        private static Canvas sharedCanvas;


        public static WrapPanel SharedDeviceStackPanel
        {
            get { return sharedWrapPanel; }
        }
        private static WrapPanel sharedWrapPanel;

        #region Instance Variables

        MSEKinectManager kinectManager;
        //DispatcherTimer dispatchTimer;

        /// <summary>
        /// Rendering code from the SkeletonBasics example, for demonstration purposes 
        /// </summary>
        private SkeletonRenderer skeletonRenderer;

        private Dictionary<PairablePerson, PersonControl> PersonControlDictionary;
        private Dictionary<string, DeviceControl> DeviceControlDictionary;
        private Dictionary<string, TrackerControl> TrackerControlDictionary;
        //private DrawnTracker drawnTracker;

        #endregion

        #region constants

        ///// <summary>
        ///// Width of output drawing
        ///// </summary>
        //private const float RenderWidth = 640.0f;

        ///// <summary>
        ///// Height of our output drawing
        ///// </summary>
        //private const float RenderHeight = 640.0f;

        //const double deviceDrawWidth = 0.25 * RenderWidth / ROOM_WIDTH;
        //const double deviceDrawHeight = 0.25 * RenderHeight / ROOM_HEIGHT;

        //const double trackerDrawWidth = 0.10 * RenderWidth / ROOM_WIDTH;
        //const double trackerDrawHeight = 0.10 * RenderHeight / ROOM_HEIGHT;

        //const int FPS = 60;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            sharedCanvas = canvas;
            sharedWrapPanel = unpairedDeviceStackPanel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DrawingResources.GenerateGridLines(canvas, GridLines, GridLinesScaleSlider.Value);
            GridLines.ShowGridLines = true;

            // When we do the event handling through XAML, an event fires before the Window is loaded, and it freezes the program, so we do event binding after Window is loaded
            GridLinesScaleSlider.ValueChanged += UpdateGridlines;
            GridLinesCheckBox.Checked += ChangeGridlineVisibility;
            GridLinesCheckBox.Unchecked += ChangeGridlineVisibility;
            RangeCheckBox.Checked += ChangeRangeVisibility;
            RangeCheckBox.Unchecked += ChangeRangeVisibility;
                   
            //Create Dictionaries for DeviceControl, PersonControl
            DeviceControlDictionary = new Dictionary<string, DeviceControl>();
            PersonControlDictionary = new Dictionary<PairablePerson, PersonControl>();
            TrackerControlDictionary = new Dictionary<string, TrackerControl>();


            //Initialize and Start MSEKinectManager
            kinectManager = new MSEKinectManager();
            kinectManager.Start();

            // The tracker is created in the PersonManager constructor, so there's actually no way for us to listen for its creation the first time
            trackerChanged(kinectManager.PersonManager, kinectManager.PersonManager.Tracker);

            //Setup Events for Device Addition and Removal, Person Addition and Removal 
            kinectManager.DeviceManager.DeviceAdded += deviceAdded;
            kinectManager.DeviceManager.DeviceRemoved += deviceRemoved;
            kinectManager.PersonManager.PersonAdded += personAdded;
            kinectManager.PersonManager.PersonRemoved += personRemoved;

            //Seperate components for displaying the visible skeletons
            skeletonRenderer = new SkeletonRenderer(SkeletonBasicsImage);


            //Hardcode tracker position and orientation
            Tracker tracker = kinectManager.Locator.Trackers[0];

            //TODO - Set up event handling for new Trackers and put this code in there.
            TrackerControlDictionary[tracker.Identifier] = new TrackerControl(tracker);
            canvas.Children.Add(TrackerControlDictionary[tracker.Identifier]);

            // Values retrieved from:
            // http://blogs.msdn.com/b/kinectforwindows/archive/2012/01/20/near-mode-what-it-is-and-isn-t.aspx
            // http://msdn.microsoft.com/en-us/library/jj131033.aspx
            tracker.MinRange = 0.8;
            tracker.MaxRange = 4;
            tracker.FieldOfView = 57;

            tracker.Location = new Point(DrawingResources.ROOM_WIDTH / 2, DrawingResources.ROOM_HEIGHT);
            tracker.Orientation = 270;
        }

        //Window Close (End the Kinect Manager) 
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kinectManager.Stop();
        }  

        #region Handlers for Person and Device manager events
        
        void deviceAdded(DeviceManager deviceManager, PairableDevice pairableDevice)
        {
            // Finds the matching IADevice from the pairableDevice Identifier
            IADevice iaDevice = deviceManager.IntAirAct.Devices.Find(d => d.Name.Equals(pairableDevice.Identifier));

            // Iterate over all supported routes
            foreach (IARoute route in iaDevice.SupportedRoutes)
            {
                // If the device has the route with resource "/pairingState/paired", then we know it is a device running the MSEAPI Client, so we'll add it to the unpaired device list
                if (route.Resource.Equals("/pairingState/paired") && route.Action.Equals("PUT"))
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DeviceControlDictionary[pairableDevice.Identifier] = new DeviceControl(pairableDevice);
                        unpairedDeviceStackPanel.Children.Add(DeviceControlDictionary[pairableDevice.Identifier]);
                    }));
                    break;
                }
            }
        }

        void deviceRemoved(DeviceManager deviceManager, PairableDevice pairableDevice)
        {

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DeviceControlDictionary.ContainsKey(pairableDevice.Identifier))
                {
                    canvas.Children.Remove(DeviceControlDictionary[pairableDevice.Identifier]);
                    unpairedDeviceStackPanel.Children.Remove(DeviceControlDictionary[pairableDevice.Identifier]);

                    DeviceControlDictionary.Remove(pairableDevice.Identifier);
                }
            }));

        }

        void personAdded(PersonManager personManager, PairablePerson pairablePerson)
        {
            PersonControlDictionary[pairablePerson] = new PersonControl(pairablePerson);
            canvas.Children.Add(PersonControlDictionary[pairablePerson]);
        }

        void personRemoved(PersonManager personManager, PairablePerson pairablePerson)
        {
            if (PersonControlDictionary.ContainsKey(pairablePerson))
            {
                canvas.Children.Remove(PersonControlDictionary[pairablePerson]);
                PersonControlDictionary.Remove(pairablePerson);
            }
        }

        void trackerChanged(PersonManager sender, Tracker tracker)
        {
            if (tracker != null)
            {
                //drawnTracker = new DrawnTracker(tracker);
            }

        }

        #endregion

        // Updates the scale of the Gridlines
        private void UpdateGridlines(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (GridLinesScaleSlider.Value == 1)
                MetersTextBlock.Text = " meter";
            else
                MetersTextBlock.Text = " meters";

            DrawingResources.GenerateGridLines(canvas, GridLines, GridLinesScaleSlider.Value);
        }

        private void ChangeRangeVisibility(object sender, RoutedEventArgs e)
        {
            if (RangeCheckBox.IsChecked.HasValue && RangeCheckBox.IsChecked.Value == true)
            {
                // Show Range
                foreach (KeyValuePair<string,TrackerControl> pair in TrackerControlDictionary)
                {
                    pair.Value.showRange();
                }
            }
            else if (RangeCheckBox.IsChecked.HasValue && RangeCheckBox.IsChecked.Value == false)
            {
                // Hide Range
                foreach (KeyValuePair<string, TrackerControl> pair in TrackerControlDictionary)
                {
                    pair.Value.hideRange();
                }
            }
        }

        // Hides/Shows the Gridlines and Slider based on the Checkbox's state
        private void ChangeGridlineVisibility(object sender, RoutedEventArgs e)
        {
            if (GridLinesCheckBox.IsChecked.HasValue && GridLinesCheckBox.IsChecked.Value == true)
            {
                // Show Gridlines
                GridLines.ShowGridLines = true;
                GridLinesScaleSlider.Visibility = System.Windows.Visibility.Visible;
                GridLinesScaleStackPanel.Visibility = System.Windows.Visibility.Visible;

            }
            else if (GridLinesCheckBox.IsChecked.HasValue && GridLinesCheckBox.IsChecked.Value == false)
            {
                // Hide Gridlines
                GridLines.ShowGridLines = false;
                GridLinesScaleSlider.Visibility = System.Windows.Visibility.Collapsed;
                GridLinesScaleStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Environment.Exit(0);
            }
        }


    }
}
