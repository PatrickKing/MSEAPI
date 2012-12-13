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




        }
        #endregion

        #region Route Handlers

        #region Pairing
        void UpdateDevicePairingState(IARequest request, IAResponse response)
        {
            pairingRecognizer.DevicePairAttempt(request.Origin.Name);
        }
        #endregion

        #region Device Properties

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
                    // TODO use the paired person's location, and calculate the angle more precisely.
                    //Point requestingDeviceLocation = requestingDevice.Location.Value;
                    //Point offsetLocation = personManager.Tracker.Location.Value;

                    //double angle = Util.AngleBetweenPoints(requestingDeviceLocation, offsetLocation);
                    //response.SetBodyWith(angle);
                }
                else
                {
                    // Device doesn't have location 
                    response.StatusCode = 400;
                }

                // When the device requests the offset angle, it is facing the tracker, so its orientation is approximately turned around by 180 degrees
                response.SetBodyWith(Util.NormalizeAngle(personManager.Tracker.Orientation.Value - 180));
            }
        }


        /// <summary>
        /// Handle a request with updated information for a device.
        /// Presently, only used to update device location
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void UpdateDeviceOrientation(IARequest request, IAResponse response)
        {
            string result = request.BodyAsString();
            //TODO: Handle parse failure gracefully 
            float newOrientation = float.Parse(result);
            Console.WriteLine(newOrientation);

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

        public void UpdateDeviceLocation(IARequest request, IAResponse response)
        {
            Point result = request.BodyAs<IntermediatePoint>();

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
            }
            else
            {
                // Respond with the device
                response.SetBodyWith(GetIntermediateDevice(device));
            }
        }

        /// <summary>
        /// Return All Devices known to the Locator 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void GetDevices(IARequest request, IAResponse response)
        {
            List<IntermediateDevice> intermediateDevices = GetIntermediateDevicesList(locator.Devices);

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

            Device nearestDevice = locator.GetNearestDeviceInView(observer);
            if (nearestDevice == null)
            {
                response.StatusCode = 404;
            }
            else
            {
                response.SetBodyWith(GetIntermediateDevice(nearestDevice));
            }
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

            List<Device> devicesInView = locator.GetDevicesInView(observer);
            if (devicesInView.Count == 0)
            {
                response.StatusCode = 404;
            }
            else
            {
                List<IntermediateDevice> intDevices = GetIntermediateDevicesList(devicesInView);
                response.SetBodyWith(intDevices);
            }
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
            }
            else
            {
                // Respond with the device
                response.SetBodyWith(GetIntermediateDevice(nearestDevice));
            }
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
            if (devicesInView.Count == 0)
            {
                response.StatusCode = 404;
            }
            else
            {
                // Respond with the device
                response.SetBodyWith(GetIntermediateDevicesList(devicesInView));
            }

        }


        #endregion

        #endregion

        #region Utility Functions

        // For transmission, we create objects with an anonymous type where the instance variable names precisely match the ones on iOS.
        // ie, identifier instead of Identifier
        // This makes deserialization on the client easier.
        List<IntermediateDevice> GetIntermediateDevicesList(List<Device> devices)
        {
            List<IntermediateDevice> intermediateDevices = new List<IntermediateDevice>();
            foreach (Device device in devices)
            {
                IntermediateDevice intermediateDevice = new IntermediateDevice
                {
                    orientation = device.Orientation,
                    identifier = device.Identifier,
                    location = device.Location
                };

            }

            return intermediateDevices; 
        }


        public IntermediateDevice GetIntermediateDevice(Device device)
        {
            if (device == null)
            {
                return null;
            }
            IntermediateDevice intermediateDevice = new IntermediateDevice();

            intermediateDevice.identifier = device.Identifier;
            intermediateDevice.orientation = device.Orientation;
            intermediateDevice.location = device.Location;
            return intermediateDevice;
        }

        #endregion

    }
}
