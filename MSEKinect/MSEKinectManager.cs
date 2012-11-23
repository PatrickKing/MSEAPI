using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSEGestureRecognizer;
using IntAirAct;
using MSELocator; 

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


        public void Start()
        {
            intAirAct = IAIntAirAct.New();

            locator = new Locator();

            //Instantiate Components 
            pairingRecognizer = new PairingRecognizer(locator, intAirAct);

            gestureController = new GestureController();
            personManager = new PersonManager(locator, gestureController);
            deviceManager = new DeviceManager(locator, intAirAct);


            personManager.StartPersonManager();
            deviceManager.StartDeviceManager();

            intAirAct.Start();

            gestureController.GestureRecognized += pairingRecognizer.PersonPairAttempt;

            communicationManager = new CommunicationManager(intAirAct, pairingRecognizer, gestureController, locator);
            
        }

        public void Stop()
        {
            personManager.StopPersonManager();
            deviceManager.StopDeviceManager();
            intAirAct.Stop();
        }


    }
}
