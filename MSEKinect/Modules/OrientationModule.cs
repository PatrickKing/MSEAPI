using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy; 
using IntAirAct;
using Nancy.ModelBinding;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace MSEKinect.Modules
{
    public class OrientationModule: NancyModule
    {
        private static TraceSource logger = new TraceSource("MSEKinect");

        public OrientationModule(IAIntAirAct intAirAct, Room room)
        {
            Get["device/{identifier}"] = parameters =>
            {
                String name = Uri.UnescapeDataString(parameters.identifier);

                //Find the associated device in the Current Devices 
                Device device = room.CurrentDevices.Find(d => d.Identifier.Equals(name));

                return Response.RespondWith(device, "devices");
            };

            Put["device/{identifier}"] = parameters =>
            {
                string json = Request.BodyAsString();
                logger.TraceEvent(TraceEventType.Verbose, 0, json);
                JObject msedeviceJson = JObject.Parse(json);
                object obj = intAirAct.DeserializeObject(msedeviceJson);
                Type type1 = obj.GetType();
                if (obj.GetType().Equals(typeof(Device)))
                {
                    String name = Uri.UnescapeDataString(parameters.identifier);
                    Device device = room.CurrentDevices.Find(d => d.Identifier.Equals(name));

                    device.Orientation = ((Device)obj).Orientation;
                    logger.TraceEvent(TraceEventType.Verbose, 0, "Successfully updated orientation");
                    return "OK";
                }
                else
                {
                    return HttpStatusCode.BadRequest;
                }
            };
        }
    }
}
