using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IntAirAct;

namespace MSEAPI_SharedNetworking
{
    // Static members for all of the routes used in MSEAPI

    public class Routes
    {
        //TODO: Test whether reusing routes this way is acceptable to intairact. Seems to be fine...

        #region Routes Hosted by MSEAPI Server

        // Pairing
        public static IARoute RequestPairingRoute = IARoute.Put("/device/{identifier}/pairWith");

        // Orientation and Location
        public static IARoute SetOrientationRoute = IARoute.Put("/devices/{identifier}/orientation");
        public static IARoute GetOffsetAngleRoute = IARoute.Get("/device/{identifier}/offsetAngle");
        public static IARoute SetLocationRoute = IARoute.Put("/device/{identifier}/location");

        // Locator
        public static IARoute GetDeviceInfoRoute = IARoute.Get("/device/{identifier}");
        public static IARoute GetAllDeviceInfoRoute = IARoute.Get("/devices");
        public static IARoute GetNearestDeviceInViewRoute = IARoute.Get("/device/view/{identifier}");
        public static IARoute GetAllDevicesInViewRoute = IARoute.Get("/devices/view/{identifier}");
        public static IARoute GetNearestDeviceInRangeRoute = IARoute.Get("/device/range/{identifier}/{range}");
        public static IARoute GetAllDevicesInRangeRoute = IARoute.Get("/devices/range/{identifier}/{range}");

        #endregion

        #region Routes Hosted by MSEAPI Client

        //Pairing
        public static IARoute BecomePairedRoute = IARoute.Put("/pairingState/paired");
        public static IARoute BecomeUnpairedRoute = IARoute.Put("/pairingState/notpaired");

        // Inter-device communication
        public static IARoute ImageRoute = IARoute.Post("/image/{imagename}");
        public static IARoute DataRoute = IARoute.Post("/data");
        public static IARoute DictionaryRoute = IARoute.Post("/dictionary/{dictionarytype}");


        //NB: For each object type, we define a unique route using this string as the base
        //ie: IARoute.Post(ObjectRouteString + "mytype");
        public static String ObjectRouteBaseString = "/object/";

        
        #endregion


        


        

        
        
        

    }
}
