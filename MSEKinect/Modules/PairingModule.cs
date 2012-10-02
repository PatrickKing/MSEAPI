using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using IntAirAct;
using TinyIoC; 
namespace MSEKinect
{
    public class PairingModule : NancyModule
    {
        public PairingModule(IAIntAirAct intAirAct)
        {
            Action<IADevice> action = delegate(IADevice device) 
            {
                //Log the device as attempting pair 
                //Console.Out.WriteLine("Device" + device.name + "requesting pairing"); 

                //Retrieve the Room variable from Tiny IoC 
                Room ri = TinyIoC.TinyIoCContainer.Current.Resolve<Room>(); 

                //Call the Device Gesture Recognized Method 
                ri.DevicePairGestureRecognized(device.name); 
            }; 
            Put["action/pairWith"] = _ => Response.Execute(action);            
        }


    }
}
