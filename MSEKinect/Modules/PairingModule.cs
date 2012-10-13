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
        public PairingModule(Room room)
        {
            Action<IADevice> action = delegate(IADevice device) 
            {
                //Call the Device Gesture Recognized Method 
                room.DevicePairGestureRecognized(device.name); 
            }; 
            Put["action/pairWith"] = _ => Response.Execute(action);
        }
    }
}
