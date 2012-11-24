using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IntAirAct;
using MSEKinect;
using MSELocator;
using System.Windows;


namespace MSEKinect
{
    /// <summary>
    /// CommunicationManager handles all IntAirAct requests for MSEKinect
    /// </summary>
    public class CommunicationManager
    {
        #region Instance Variables

        private IntAirAct.IAIntAirAct intAirAct;
        private PairingRecognizer pairingRecognizer;
        private MSEGestureRecognizer.GestureController gestureController;
        private MSELocator.LocatorInterface locator;
        private PersonManager personManager;

        #endregion


        public CommunicationManager(IntAirAct.IAIntAirAct intAirAct, PairingRecognizer pairingRecognizer, MSEGestureRecognizer.GestureController gestureController, MSELocator.LocatorInterface locator, PersonManager personManager)
        {
            this.intAirAct = intAirAct;
            this.pairingRecognizer = pairingRecognizer;
            this.gestureController = gestureController;
            this.locator = locator;
            this.personManager = personManager;

            // Routes to hook up
            intAirAct.Route(IARoute.Get("/device/{identifier}"), new Action<IARequest, IAResponse>(GetDeviceInformation));
            intAirAct.Route(IARoute.Put("/device/{identifier}"), new Action<IARequest, IAResponse>(UpdateDeviceOrientation));
            intAirAct.Route(IARoute.Put("/device/pairWith"), new Action<IARequest, IAResponse>(UpdateDevicePairingState));
            intAirAct.Route(IARoute.Get("/device/{identifier}/intersections"), new Action<IARequest, IAResponse>(GetDevicesInView));

            intAirAct.Route(IARoute.Get("/device/{identifier}/offsetAngle"), new Action<IARequest, IAResponse>(GetOffsetAngle));

             
        }


        /// <summary>
        /// Handle a request for information about a device
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void GetDeviceInformation(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];

            // Find the associated device in the Current Devices 
            Device device = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));

            // Respond with the device
            response.SetBodyWith(device);

        }

        /// <summary>
        /// Handle a request with updated information for a device.
        /// Presently, only used to update device location
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void UpdateDeviceOrientation(IARequest request, IAResponse response)
        {

            Device requestDevice = request.BodyAs<Device>();

            String name = request.Parameters["identifier"];
            Device localDevice = locator.Devices.Find(d => d.Identifier.Equals(name));

            if (localDevice != null)
            {
                localDevice.Orientation = requestDevice.Orientation;
                response.StatusCode = 201; // created
            }
            else
            {
                response.StatusCode = 404; // not found
            }
        

        }

        void UpdateDevicePairingState(IARequest request, IAResponse response)
        {
            pairingRecognizer.DevicePairAttempt(request.Origin.Name);
        }

        void GetDevicesInView(IARequest request, IAResponse response)
        {
            // Find the observing device
            String deviceIdentifier = request.Parameters["identifier"];
            Device observingDevice = locator.Devices.Find(d => d.Identifier == deviceIdentifier);

            // Device Does Not Exist
            if (observingDevice == null)
            {
                response.StatusCode = 404; // not found
                return;
            }
            // Device Exists
            else
            {
                //Compute the list of intersecting devices, respond with the list
                List<Device> intersectingDevices = locator.GetDevicesInView(observingDevice);
                response.SetBodyWith(intersectingDevices);
            }

        }

        void GetOffsetAngle(IARequest request, IAResponse response)
        {

            // Find the observing device
            String deviceIdentifier = request.Parameters["identifier"];
            Device requestingDevice = locator.Devices.Find(d => d.Identifier == deviceIdentifier);

            // Device Does Not Exist
            if (requestingDevice == null)
            {
                response.StatusCode = 404; // not found
                return;
            }
            // Device Exists
            else
            {
                if (requestingDevice.Location.HasValue && personManager.Tracker.Location.HasValue)
                {
                    Point requestingDeviceLocation = requestingDevice.Location.Value;
                    Point offsetLocation = personManager.Tracker.Location.Value;

                    double angle = Util.NormalizeAngle(Util.AngleBetweenPoints(requestingDeviceLocation, offsetLocation));
                    response.SetBodyWith(angle);
                }
                else
                {
                    // Device doesn't have location 
                    response.StatusCode = 400; 
                }

                
                response.SetBodyWith(90.0f);
            }



        }


    }
}
