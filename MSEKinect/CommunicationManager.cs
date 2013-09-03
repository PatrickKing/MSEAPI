using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IntAirAct;
using MSEKinect;
using MSELocator;
using System.Windows;
using System.Collections;
using MSEAPI_SharedNetworking;

using Newtonsoft.Json; 

namespace MSEKinect
{
    public class CommunicationManager
    {

        #region Instance Variables

        private IntAirAct.IAIntAirAct intAirAct;
        private PairingRecognizer pairingRecognizer;
        private MSELocator.LocatorInterface locator;
        private PersonManager personManager;

        #endregion

        #region Constructor 
        public CommunicationManager(IntAirAct.IAIntAirAct intAirAct, PairingRecognizer pairingRecognizer, MSELocator.LocatorInterface locator, PersonManager personManager)
        {
            this.intAirAct = intAirAct;
            this.pairingRecognizer = pairingRecognizer;
            this.locator = locator;
            this.personManager = personManager;

            // Pairing
            intAirAct.Route(Routes.RequestPairingRoute, new Action<IARequest, IAResponse>(UpdateDevicePairingState));

            // Properties of Devices
            intAirAct.Route(Routes.GetOffsetAngleRoute, new Action<IARequest, IAResponse>(GetOffsetAngle));
            intAirAct.Route(Routes.SetOrientationRoute, new Action<IARequest, IAResponse>(UpdateDeviceOrientation));
            intAirAct.Route(Routes.SetLocationRoute, new Action<IARequest, IAResponse>(UpdateDeviceLocation));

            // Locating Devices
            intAirAct.Route(Routes.GetDeviceInfoRoute, new Action<IARequest, IAResponse>(GetDevice));
            intAirAct.Route(Routes.GetAllDeviceInfoRoute, new Action<IARequest, IAResponse>(GetDevices));
            intAirAct.Route(Routes.GetNearestDeviceInViewRoute, new Action<IARequest, IAResponse>(GetNearestDeviceInView));
            intAirAct.Route(Routes.GetAllDevicesInViewRoute, new Action<IARequest, IAResponse>(GetDevicesInView));
            intAirAct.Route(Routes.GetNearestDeviceInRangeRoute, new Action<IARequest, IAResponse>(GetNearestDeviceInRange));
            intAirAct.Route(Routes.GetAllDevicesInRangeRoute, new Action<IARequest, IAResponse>(GetDevicesInRange));
            
            /* Depricated!!!
            intAirAct.Route(Routes.GetAllDevicesWithIntersectionPointsRoute, new Action<IARequest, IAResponse>(GetDevicesWithIntersectionPoint));
             */




        }
        #endregion

        #region Route Handlers

        #region Pairing
        void UpdateDevicePairingState(IARequest request, IAResponse response)
        {
            Console.WriteLine(request.Parameters["identifier"]);
            pairingRecognizer.DevicePairAttempt(request.Parameters["identifier"]);
        }
        #endregion

        #region Device Properties

        void GetOffsetAngle(IARequest request, IAResponse response)
        {
            // Find the device
            String deviceIdentifier = request.Parameters["identifier"];
            Device requestingDevice = locator.Devices.Find(d => d.Identifier == deviceIdentifier);

            // Device Does Not Exist
            if (requestingDevice == null)
            {
                response.StatusCode = 404; // not found
                return;
            }

            if (requestingDevice.Location.HasValue && locator.Trackers[0].Location.HasValue)
            {
                Point requestingDeviceLocation = requestingDevice.Location.Value;
                Point offsetLocation = locator.Trackers[0].Location.Value;

                double angle = Util.AngleBetweenPoints(requestingDeviceLocation, offsetLocation);
                response.SetBodyWith(angle);

                //response.SetBodyWith(90);
            }
            else
            {
                // Device doesn't have location 
                response.StatusCode = 400;
            }

        
        }

        /// <summary>
        /// Handle a request with updated information for a device.
        /// Presently, only used to update device location
        /// </summary>
        /// <param name="request">IntAirAct Request</param>
        /// <param name="response">IntAirAct Response</param>
        void UpdateDeviceOrientation(IARequest request, IAResponse response)
        {
            string result = request.BodyAsString();
            //TODO: Handle parse failure gracefully 
            float newOrientation = float.Parse(result);

            String name = request.Parameters["identifier"];
            Device localDevice = locator.Devices.Find(d => d.Identifier.Equals(name));

            if (localDevice != null)
            {
                localDevice.Orientation = newOrientation;
                response.StatusCode = 201; // created
            }
            else
            {
                response.StatusCode = 404; // not found
            }
        }

        /// <summary>
        /// Handle a request with updated location for a device.
        /// </summary>
        /// <param name="request">IntAirAct Request</param>
        /// <param name="response">IntAirAct Response</param>
        public void UpdateDeviceLocation(IARequest request, IAResponse response)
        {
            IntermediatePoint intermediatePoint = request.BodyAs<IntermediatePoint>();
            Point result = intermediatePoint.ToPoint();

            String name = request.Parameters["identifier"];
            Device localDevice = locator.Devices.Find(d => d.Identifier.Equals(name));

            if (localDevice != null)
            {
                localDevice.Location = result;
                response.StatusCode = 201; // created
            }
            else
            {
                response.StatusCode = 404; // not found
            }

        }

        #endregion

        #region Locating Devices
        /// <summary>
        /// Handle a request for information about a device
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void GetDevice(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];

            // Find the associated device in the Current Devices 
            Device device = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            if (device == null)
            {
                response.StatusCode = 404;
                return;
            }

            // Get the intermediateDevice for serialization
            IntermediateDevice intermediateDevice = PairableDevice.GetCompleteIntermediateDevice(device);
            if (intermediateDevice == null)
            {
                //TODO: Should this status code be different, to reflect that the device exists but couldn't be returned due to incompleteness?
                response.StatusCode = 404;
                return;
            }

            // Respond with the device
            response.SetBodyWith(intermediateDevice);
        }

        /// <summary>
        /// Return All Devices known to the Locator 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void GetDevices(IARequest request, IAResponse response)
        {
            List<IntermediateDevice> intermediateDevices = PairableDevice.GetCompleteIntermediateDevicesList(locator.Devices);

            if (intermediateDevices.Count == 0)
            {
                response.StatusCode = 404;
            }
            else
            {
                response.SetBodyWith(intermediateDevices);
            }

        }

        void GetNearestDeviceInView(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];

            // Find the associated device in the Current Devices 
            Device observer = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            if (observer == null)
            {
                //TODO: Should we use distinct status codes for distinct failure types here?
                response.StatusCode = 404;
                return;
            }

            // Find the nearest device that we observe
            Device nearestDevice = locator.GetNearestDeviceInView(observer);
            if (nearestDevice == null)
            {
                response.StatusCode = 404;
                return;
            }

            // Prepare the device for serialization
            IntermediateDevice intermediateDevice = PairableDevice.GetCompleteIntermediateDevice(nearestDevice);
            if (intermediateDevice == null)
            {
                response.StatusCode = 404;
                return;
            }

            response.SetBodyWith(intermediateDevice);

        }

        void GetDevicesInView(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];

            // Find the associated device in the Current Devices 
            Device observer = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            if (observer == null)
            {
                response.StatusCode = 404;
                return;
            }        

            // Get the devices in view, and convert them for serialization
            List<Device> devicesInView = locator.GetDevicesInView(observer);
            List<IntermediateDevice> intDevices = PairableDevice.GetCompleteIntermediateDevicesList(devicesInView);
            if (intDevices.Count == 0)
            {
                response.StatusCode = 404;
                return;
            }

            response.SetBodyWith(intDevices);
        }

        void GetNearestDeviceInRange(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];
            double range = Double.Parse(request.Parameters["range"]);

            // Find the associated device in the Current Devices 
            Device observer = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            if (observer == null)
            {
                response.StatusCode = 404;
                return;
            }

            Device nearestDevice = locator.GetNearestDeviceWithinRange(observer, range);
            if (nearestDevice == null)
            {
                response.StatusCode = 404;
                return;
            }

            IntermediateDevice intermediateDevice = PairableDevice.GetCompleteIntermediateDevice(nearestDevice);
            if (intermediateDevice == null)
            {
                response.StatusCode = 404;
                return;
            }

            // Respond with the device
            response.SetBodyWith(intermediateDevice);
        }

        void GetDevicesInRange(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];
            double range = Double.Parse(request.Parameters["range"]);

            // Find the associated device in the Current Devices 
            Device device = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            if (device == null)
            {
                response.StatusCode = 404;
                return;
            }

            List<Device> devicesInView = locator.GetDevicesWithinRange(device, range);
            List<IntermediateDevice> intermediateDevices = PairableDevice.GetCompleteIntermediateDevicesList(devicesInView);
            if (intermediateDevices.Count == 0)
            {
                response.StatusCode = 404;
                return;
            }

            // Respond with the device
            response.SetBodyWith(intermediateDevices);

        }

        /* Depricated!!!
        void GetDevicesWithIntersectionPoint(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];

            // Find the associated device in the Current Devices 
            Device observer = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            if (observer == null)
            {
                response.StatusCode = 404;
                return;
            }

            List<IntermediateDevice> deviceAndIntersections = ConvertToUsePairableDevices(locator.GetDevicesInView(observer));

            if (deviceAndIntersections.Count == 0)
            {
                response.StatusCode = 404;
                return;
            }

            response.SetBodyWith(deviceAndIntersections);
        }
          

        private List<IntermediateDevice> ConvertToUsePairableDevices(List<Device> devices)
        {
            List<IntermediateDevice> convertedList = new List<IntermediateDevice>();

            foreach (Device device in devices)
            {
                //Convert from Device to IntermediateDevice
                //TODO - Fix this atrocity
                IntermediateDevice intermediateDevice = PairableDevice.GetCompleteIntermediateDevice(device);
                intermediateDevice.intersectionPoint = new Point(device.intersectionPoint["x"], device.intersectionPoint["y"]);
                convertedList.Add(intermediateDevice); 
            }

            return convertedList;

        }
        */

        #endregion

        #endregion


    }
}
