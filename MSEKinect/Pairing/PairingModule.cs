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

        public PairingModule(PairingRecognizer recognizer)
        {
            Action<IADevice> action = delegate(IADevice device) 
            {
                logger.TraceEvent(TraceEventType.Information, 0, "Device" + device.Name + "requesting pairing");
                recognizer.DevicePairAttempt(device.Name); 
            }; 

            Put["action/pairWith"] = _ => Response.Execute(action);
        }
    }
}
