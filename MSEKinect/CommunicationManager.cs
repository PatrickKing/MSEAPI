using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IntAirAct;
using MSEKinect;
using MSELocator;
using System.Windows;
using System.Collections;


namespace MSEKinect
{
    /// <summary>
    /// CommunicationManager handles all IntAirAct requests for MSEKinect
    /// </summary>
    /// 
    public class IntermediateDevice
    {
        private String identifer;
        private double? orientation;
        private Point? location;

        public IntermediateDevice(Device device)
        {
            this.identifer = device.Identifier;
            this.orientation = device.Orientation;
            this.location = device.Location;
        }

    }
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

            // Routes used only for the initial application

            //to be deprecated ... 
            intAirAct.Route(IARoute.Get("/device/{identifier}/intersections"), new Action<IARequest, IAResponse>(GetDevicesInViewOld));


            intAirAct.Route(IARoute.Put("/devices/{identifier}/orientation"), new Action<IARequest, IAResponse>(UpdateDeviceOrientation));
            intAirAct.Route(IARoute.Put("/device/pairWith"), new Action<IARequest, IAResponse>(UpdateDevicePairingState));
            intAirAct.Route(IARoute.Get("/device/{identifier}/offsetAngle"), new Action<IARequest, IAResponse>(GetOffsetAngle));


            // Routes used in the API proper
            intAirAct.Route(IARoute.Get("/device/{identifier}"), new Action<IARequest, IAResponse>(GetDevice));
            intAirAct.Route(IARoute.Get("/devices"), new Action<IARequest, IAResponse>(GetDevices));
            intAirAct.Route(IARoute.Get("/device/view/{identifier}"), new Action<IARequest, IAResponse>(GetNearestDeviceInView));
            intAirAct.Route(IARoute.Get("/devices/view/{identifier}"), new Action<IARequest, IAResponse>(GetDevicesInView));
            intAirAct.Route(IARoute.Get("/device/range/{identifier}/{range}"), new Action<IARequest, IAResponse>(GetNearestDeviceInRange));
            intAirAct.Route(IARoute.Get("/devices/range/{identifier}/{range}"), new Action<IARequest, IAResponse>(GetDevicesInRange));




        }


        #region Route Handlers

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

            // Respond with the device
            response.SetBodyWith(new IntermediateDevice(device)); 

        }

        /// <summary>
        /// Handle a request with updated information for a device.
        /// Presently, only used to update device location
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void UpdateDeviceOrientation(IARequest request, IAResponse response)
        {
            float newOrientation = request.BodyAs<float>();

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

        void UpdateDevicePairingState(IARequest request, IAResponse response)
        {
            pairingRecognizer.DevicePairAttempt(request.Origin.Name);
        }

        void GetDevicesInViewOld(IARequest request, IAResponse response)
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

                    double angle = Util.AngleBetweenPoints(requestingDeviceLocation, offsetLocation);
                    response.SetBodyWith(angle);
                }
                else
                {
                    // Device doesn't have location 
                    response.StatusCode = 400; 
                }

                // When the device requests the offset angle, it is facing the tracker, so its orientation is approximately turned around by 180 degrees
                // TODO use the paired person's location, and calculate the angle more precisely.
                response.SetBodyWith(Util.NormalizeAngle(personManager.Tracker.Orientation.Value - 180));
            }



        }
        #endregion


        //Return All Devices known to Locator 
        void GetDevices(IARequest request, IAResponse response)
        {

            response.SetBodyWith(IntermediateDevicesForDevices(locator.Devices)); 
        }



        void GetNearestDeviceInView(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];

            // Find the associated device in the Current Devices 
            Device device = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            Device nearestDevice = locator.GetNearestDeviceInView(device);

            // Respond with the device
            response.SetBodyWith(new IntermediateDevice(nearestDevice)); 

        }

        void GetDevicesInView(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];

            // Find the associated device in the Current Devices 
            Device device = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            List<Device> devicesInView = locator.GetDevicesInView(device);

            // Respond with the device
            response.SetBodyWith(IntermediateDevicesForDevices(devicesInView)); 

        }

        void GetNearestDeviceInRange(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];
            double range = Double.Parse(request.Parameters["range"]);

            // Find the associated device in the Current Devices 
            Device device = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            Device nearestDevice = locator.GetNearestDeviceWithinRange(device, range);

            // Respond with the device
            response.SetBodyWith(new IntermediateDevice(nearestDevice)); 

        }

        void GetDevicesInRange(IARequest request, IAResponse response)
        {
            String deviceIdentifier = request.Parameters["identifier"];
            double range = Double.Parse(request.Parameters["range"]);

            // Find the associated device in the Current Devices 
            Device device = locator.Devices.Find(d => d.Identifier.Equals(deviceIdentifier));
            List<Device> devicesInView = locator.GetDevicesWithinRange(device, range);

            // Respond with the device
            response.SetBodyWith(IntermediateDevicesForDevices(devicesInView)); 
        }

        #region Utility Functions
        // For transmission, we create objects with an anonymous type where the instance variable names precisely match the ones on iOS.
        // ie, identifier instead of Identifier
        // This makes deserialization on the client easier.
        List<IntermediateDevice> IntermediateDevicesForDevices(List<Device> devices)
        {
            List<IntermediateDevice> intermediateDevices = (from device in devices
                                                            select new IntermediateDevice(device)
                                                            ).ToList<IntermediateDevice>();
            return intermediateDevices; 
        }


        #endregion



    }
}
