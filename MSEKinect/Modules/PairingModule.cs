using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using IntAirAct;
using TinyIoC;
using System.Diagnostics;
namespace MSEKinect
{
    public class PairingModule : NancyModule
    {
        private static TraceSource logger = new TraceSource("MSEKinect");

        public PairingModule(IAIntAirAct intAirAct)
        {
            Action<IADevice> action = delegate(IADevice device) 
            {
                //Log the device as attempting pair 
                //logger.TraceEvent(TraceEventType.Information, 0, "Device" + device.name + "requesting pairing");

                //Retrieve the Room variable from Tiny IoC 
                Room ri = TinyIoC.TinyIoCContainer.Current.Resolve<Room>(); 

                //Call the Device Gesture Recognized Method 
                ri.DevicePairGestureRecognized(device.name); 
            }; 
            Put["action/pairWith"] = _ => Response.Execute(action);            
        }


    }
}
