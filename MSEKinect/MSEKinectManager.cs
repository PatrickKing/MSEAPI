using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSEGestureRecognizer;
using IntAirAct; 

namespace MSEKinect
{
    public class MSEKinectManager
    {
        PersonManager km;
        DeviceManager dm;
        GestureController gc; 
        Room ri;
        IAIntAirAct ia;

        public void Start()
        {
            ia = new IAIntAirAct();
            ia.client = false;
            ia.capabilities.Add(new IACapability("PUT action/pairWith"));
            ia.capabilities.Add(new IACapability("PUT action/orientationUpdate"));
            ia.capabilities.Add(new IACapability("PUT /device/:identifier"));
            ia.AddMappingForClass(typeof(Device), "mse-device");

            //Instantiate Components 
            ri = new Room(ia);

            gc = new GestureController();
            km = new PersonManager(ri, gc);
            dm = new DeviceManager(ri, ia);

            ia.Start();

            km.StartPersonManager();
            dm.StartDeviceManager();

            gc.GestureRecognized += ri.PersonPairGestureRecognized;

            TinyIoC.TinyIoCContainer.Current.Register<Room>(ri);
        }

        public void Stop()
        {
            km.StopPersonManager();
            dm.StopDeviceManager();
            ia.Stop();
        }


    }
}
