using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using System.Diagnostics;
using Newtonsoft.Json;

namespace MSELocator
{
    public class IntersectionModule : NancyModule
    {

        private static TraceSource logger = new TraceSource("MSELocator");

        //(TODO) Move this into a class accessible by all Nancy Modules (maybe an extension on NancyModule itself)
        public Response RespondWithObjectAsJson(object o)
        {
            //Serialize Object
            Response response = JsonConvert.SerializeObject(o);

            //Set Header Information
            response.WithContentType("application/json; charset=utf-8");
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public IntersectionModule(LocatorInterface locator)
        {
            Get["device/{identifier}/intersections"] = parameters =>
            {
                String deviceIdentifier = Uri.UnescapeDataString(parameters.identifier); 

                //Device Does Not Exist
                if(!locator.Devices.Exists(d => d.Identifier == deviceIdentifier)) 
                {
                    logger.TraceEvent(TraceEventType.Error, 0, String.Format("Device Requested but Not Found {0}", deviceIdentifier); 
                    return HttpStatusCode.NotFound;
                }

                //Device Exists
                else 
                {
                    //Find device that corresponds with the identifier
                    Device observingDevice = locator.Devices.Find(d => d.Identifier == deviceIdentifier);

                    //Compute the list of intersecting devices
                    List<Device> intersectingDevices = locator.GetDevicesInView(observingDevice); 

                    return RespondWithObjectAsJson(intersectingDevices);
                }
            };
        }
    }
}
