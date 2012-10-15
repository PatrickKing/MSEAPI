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

        public PairingModule(PairingRecognizer room)
        {
            Action<IADevice> action = delegate(IADevice device) 
            {
                logger.TraceEvent(TraceEventType.Information, 0, "Device" + device.name + "requesting pairing");
                room.DevicePairAttempt(device.name); 
            }; 

            Put["action/pairWith"] = _ => Response.Execute(action);
        }
    }
}
