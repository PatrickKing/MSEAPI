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

            intAirAct.Start();

            personManager.StartPersonManager();
            deviceManager.StartDeviceManager();

            gestureController.GestureRecognized += pairingRecognizer.PersonPairAttempt;

            TinyIoC.TinyIoCContainer.Current.Register<LocatorInterface>(locator);
            
        }

        public void Stop()
        {
            personManager.StopPersonManager();
            deviceManager.StopDeviceManager();
            intAirAct.Stop();
        }


    }
}
