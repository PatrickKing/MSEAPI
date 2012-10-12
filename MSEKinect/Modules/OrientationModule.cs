using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy; 
using IntAirAct;
using Nancy.ModelBinding;
using System.Diagnostics;

namespace MSEKinect.Modules
{
    public class OrientationModule: NancyModule
    {
        private static TraceSource logger = new TraceSource("MSEKinect");

        public OrientationModule(Room room)
        {

            Get["device/{id}"] = parameters =>
            {
                String name = Uri.UnescapeDataString(parameters.id);

                //Find the associated device in the Current Devices 
                Device device = room.CurrentDevices.Find(d => d.Identifier.Equals(name));

                return Response.RespondWith(device, "devices");
            };

            Put["device/{id}"] = parameters =>
            {
                Device d = this.Bind();

                logger.TraceEvent(TraceEventType.Information, 0, d.ToString());

                return "Hello " + parameters.id;
            };
        }
    }
}
