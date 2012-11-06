using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy; 
using IntAirAct;
using Nancy.ModelBinding;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using MSELocator;

namespace MSEKinect.Modules
{
    public class OrientationModule: NancyModule
    {
        private static TraceSource logger = new TraceSource("MSEKinect");

        public OrientationModule(IAIntAirAct intAirAct, LocatorInterface room)
        {
            Get["device/{identifier}"] = parameters =>
            {
                String name = Uri.UnescapeDataString(parameters.identifier);

                //Find the associated device in the Current Devices 
                Device device = room.Devices.Find(d => d.Identifier.Equals(name));

                return Response.RespondWith(device, "devices");
            };

            Put["device/{identifier}"] = parameters =>
            {
                string json = Request.BodyAsString();
                logger.TraceEvent(TraceEventType.Verbose, 0, "PUT /device/:identifier ", json);
                object obj = intAirAct.DeserializeObject(JObject.Parse(json));
                Type type1 = obj.GetType();
                if (obj.GetType().Equals(typeof(Device)))
                {
                    String name = Uri.UnescapeDataString(parameters.identifier);
                    Device device = room.Devices.Find(d => d.Identifier.Equals(name));

                    if (device != null)
                    {
                        device.Orientation = ((Device)obj).Orientation;
                        logger.TraceEvent(TraceEventType.Verbose, 0, "Updated orientation of {0}", device);

                        Response response = new Response();
                        response.StatusCode = HttpStatusCode.Created; 
                        response.ContentType = "application/json";

                        return response; 
                    }
                    else
                    {
                        logger.TraceEvent(TraceEventType.Error, 0, "Could not find device with identifier: {0}", name);
                        return HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    logger.TraceEvent(TraceEventType.Error, 0, "Did not receive a Device object");
                    return HttpStatusCode.BadRequest;
                }
            };
        }
    }
}
