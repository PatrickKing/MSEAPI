using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSEGestureRecognizer; 

namespace MSEKinect
{
    public class MSEKinectManager
    {
        PersonManager km;
        DeviceManager dm;
        GestureController gc; 
        Room ri;

        public void Start()
        {
            //Instantiate Components 
            ri = new Room();
            

            gc = new GestureController();
            km = new PersonManager(ri, gc);
            dm = new DeviceManager(ri); 

            km.StartPersonManager();
            dm.StartDeviceManager();

            gc.GestureRecognized += ri.PersonPairGestureRecognized;

            TinyIoC.TinyIoCContainer.Current.Register<Room>(ri);
        }

        public void Stop()
        {
            km.StopPersonManager();
            dm.StopDeviceManager();
        }


    }
}
