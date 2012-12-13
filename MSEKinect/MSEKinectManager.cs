using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSEGestureRecognizer;
using IntAirAct;
using MSELocator;
using Microsoft.Kinect;

namespace MSEKinect
{
    public class MSEKinectManager
    {
        PersonManager personManager;
        DeviceManager deviceManager;
        GestureController gestureController; 
        PairingRecognizer pairingRecognizer;
        IAIntAirAct intAirAct;
        CommunicationManager communicationManager;

        private LocatorInterface locator;
        public LocatorInterface Locator
        {
            get{ return locator; }
        }

        public PersonManager PersonManager
        {
            get { return personManager; }
        }

        public DeviceManager DeviceManager
        {
            get { return deviceManager; }
        }

        public IAIntAirAct IntAirAct
        {
            get { return intAirAct; }
        }


        public void Start()
        {
            TestKinectAvailability();
            
            intAirAct = IAIntAirAct.New();

            locator = new Locator();

            //Instantiate Components 
            pairingRecognizer = new PairingRecognizer(locator, intAirAct);

            gestureController = new GestureController();
            personManager = new PersonManager(locator, gestureController, intAirAct);
            deviceManager = new DeviceManager(locator, intAirAct);


            personManager.StartPersonManager();
            deviceManager.StartDeviceManager();

            intAirAct.Start();

            gestureController.GestureRecognized += pairingRecognizer.PersonPairAttempt;

            communicationManager = new CommunicationManager(intAirAct, pairingRecognizer, locator, personManager);
            
        }

        public void Stop()
        {
            personManager.StopPersonManager();
            deviceManager.StopDeviceManager();
            intAirAct.Stop();
        }


        /// <summary>
        /// For testing purposes, we don't want to exit the program if a Kinect sensor is not available. So, we do the test here, away from tested code paths.
        /// </summary>
        private void TestKinectAvailability()
        {
            // Checks to see how many Kinects are connected to the system. If None then exit.
            if (KinectSensor.KinectSensors.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("No Kinect detected. Please plug in a Kinect and restart the program", "No Kinect Detected!");
                Environment.Exit(0);
            }
        }

    }
}
