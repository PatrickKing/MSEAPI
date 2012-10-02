using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy; 
using IntAirAct;

namespace MSEKinect.Modules
{
    public class OrientationModule: NancyModule
    {
        public OrientationModule(IAIntAirAct intAirAct)
        {
            //TODO Refactor this to work on a RESTful 
            Action<IADevice, Orientation> action = delegate(IADevice device, Orientation orientation)
            {
                //Retrieve the Room variable from Tiny IoC 
                Room ri = TinyIoC.TinyIoCContainer.Current.Resolve<Room>();

                //Find the associated device in the Current Devices 
                Device updateDevice = ri.CurrentDevices.Find(d => d.Identifier.Equals(device.name));
                updateDevice.Orientation = orientation; 
            };

            Put["action/orientationUpdate"] = _ => Response.Execute(action);
             
            
        }
    }
}
