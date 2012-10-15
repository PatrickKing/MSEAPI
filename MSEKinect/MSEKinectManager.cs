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
        PersonManager km;
        DeviceManager dm;
        GestureController gc; 
        PairingRecognizer ri;
        IAIntAirAct ia;

        public void Start()
        {
            ia = new IAIntAirAct();
            ia.client = false;
            ia.capabilities.Add(new IACapability("PUT action/pairWith"));
            ia.capabilities.Add(new IACapability("PUT action/orientationUpdate"));
            ia.capabilities.Add(new IACapability("PUT /device/:identifier"));
            ia.AddMappingForClass(typeof(Device), "mse-device");

            LocatorInterface locator = new Locator();

            //Instantiate Components 
            ri = new PairingRecognizer(locator, ia);

            gc = new GestureController();
            km = new PersonManager(locator, gc);
            dm = new DeviceManager(locator, ia);

            ia.Start();

            km.StartPersonManager();
            dm.StartDeviceManager();

            gc.GestureRecognized += ri.PersonPairAttempt;

            TinyIoC.TinyIoCContainer.Current.Register<LocatorInterface>(locator);
            
        }

        public void Stop()
        {
            km.StopPersonManager();
            dm.StopDeviceManager();
            ia.Stop();
        }


    }
}
