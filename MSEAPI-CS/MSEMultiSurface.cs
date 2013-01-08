using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using MSEAPI_SharedNetworking;
using MSEAPI_CS.Models;
using IntAirAct;

namespace MSEAPI_CS
{
    public class MSEMultiSurface
    {
        #region Instance Variables

        public List<MSEReceivedImageHandler> receivedImageHandlers;
        //public List<MSEReceivedDataHandler> receivedDataHandlers;
        public List<MSEReceivedDictionaryHandler> receivedDictionaryHandlers;

        public bool isRunning;

        private IAIntAirAct intAirAct;
        public IAIntAirAct IntAirAct
        {
            get
            {
                return intAirAct;
            }
        }

        private static TraceSource logger;

        #endregion

        #region Properties

        public string OwnIdentifier
        {
            get
            {
                if (intAirAct.OwnDevice == null)
                {
                    return null;
                }
                else
                {
                    return intAirAct.OwnDevice.Name;
                }
            }
        }


        #endregion

        #region Delegates

        public delegate void MSESingleDeviceHandler(MSEDevice device);
        public delegate void MSEDeviceCollectionHandler(List<MSEDevice> devices);
        public delegate void MSEErrorHandler(Exception error);
        public delegate void MSESuccessHandler();

        public delegate void MSEReceivedImageHandler(Image image, String imageName, MSEDevice originDevice, MSEGesture originGesture);
        //public delegate void MSEReceivedDataHandler(Data data, MSEDevice originDevice, MSEGesture originGesture);
        public delegate void MSEReceivedDictionaryHandler(Dictionary<string, string> dictionary, String dictionaryType, MSEDevice originDevice, MSEGesture originGesture);
        
        #endregion

        #region Constructor, Start and Stop

        public MSEMultiSurface()
        {
            //Setup Logging
            logger = new TraceSource("MSEMultiSurface");

            this.isRunning = false;

            //Initialize Our Arrays of Handlers
            this.receivedImageHandlers = new List<MSEReceivedImageHandler>();
            //this.receivedDataHandlers = new List<MSEReceivedDataHandler>();
            this.receivedDictionaryHandlers = new List<MSEReceivedDictionaryHandler>();


            //Setup IntAirAct
            this.intAirAct = IAIntAirAct.New();


            //Setup MSE Standard Routes
            //this.setupRoutes();
            
        }

        public void Start()
        {
            if (!this.isRunning)
            {
                this.intAirAct.Start();
                this.isRunning = true;
            }
        }

        /// <summary>
        /// Stops the Multi-surface Environment
        /// </summary>
        public void Stop()
        {
            if (this.isRunning)
            {
                this.isRunning = false;
                this.intAirAct.Stop();
            }
        }

        #endregion

        #region Data, Image and Dictionary route handling

        public void setupRoutes()
        {
            //Setup Received Data Handlers
            this.setupDataRoute();
            this.setupImageRoute();
            this.setupDictionaryRoute();
        }


        public void setupDataRoute()
        {
            //Setup Data Route
            this.intAirAct.Route(Routes.DataRoute, delegate(IARequest request, IAResponse response)
            {
                //Retrieves the data from the request
                byte[] data = request.Body;

                //Retrieves gesture from request
                //MSEGesture gesture = this.gestureFromMetadata(request.Metadata);

                //Error Handling


                //Run Handlers
            });

            
        }

        public void setupImageRoute()
        {
            //Setup Image Route
            this.intAirAct.Route(Routes.ImageRoute, delegate(IARequest request, IAResponse response)
            {
                //Retrieve the name of the image
                string imageName = request.Parameters["imagename"];

                //Retrieve gesture from request
                //MSEGesture gesture = this.gestureFromMetadata(request.Metadata);

                //Retrieve the image


                //Error handling

                //Run Handlers

                

            });
        }

        public void setupDictionaryRoute()
        {
            this.IntAirAct.Route(Routes.DictionaryRoute, delegate(IARequest request, IAResponse response)
            {
                //Retrieve the dictionary data
                String dictionaryType = request.Parameters["dictionarytype"];
                Dictionary<string, string> dictionary = request.BodyAs<Dictionary<string, string>>();

                //Retrieve Device from Request
                MSEDevice originDevice = new MSEDevice();
                originDevice.Identifier = request.Origin.Name;
                originDevice.setupNetworkDevice(this.IntAirAct);

                //It's possible for a device to post a message and then immediately become disconnected, if this happens we warn and do not provide the device to the handlers
                if (originDevice.NetworkDevice == null)
                {
                    logger.TraceEvent(TraceEventType.Error, 0, "MSE Error - A device (" + request.Origin.Name +") has sent a dictionary but is not now visible to MSE");
                    originDevice = null;
                }

                //Run Handlers
                foreach (MSEReceivedDictionaryHandler handler in receivedDictionaryHandlers)
                {
                    handler(dictionary, dictionaryType, originDevice, null);
                }

            });

        }
        #endregion

        #region Add Handler Methods

        public void addReceivedImageHandler(MSEReceivedImageHandler handler)
        {
            this.receivedImageHandlers.Add(handler);
        }

 /*       public void addReceivedDataHandler(MSEReceivedDataHandler handler)
        {
            this.receivedDataHandlers.Add(handler);
        }*/

        public void addReceivedDictionaryHandler(MSEReceivedDictionaryHandler handler)
        {
            this.receivedDictionaryHandlers.Add(handler);
        }

        #endregion

        #region Sending Messages
      //  public void sendImage(Image image, String name, MSEDevice targetDevice,

        #endregion


        #region Server Updates


        /// <summary>
        /// Notify the server of the device's current Location. Intended for use with stationary devices, since mobile devices can't
        /// determine their own location in the room.
        /// </summary>
        /// <param name="device">The Identifier and Location properties of this MSEDeice will be used for the update.</param>
        /// <param name="success"></param>
        /// <param name="failure"></param>
        public void UpdateDeviceLocation(MSEDevice device, MSESuccessHandler success, MSEErrorHandler failure)
        {
            IARequest updateRequest = new IARequest(Routes.SetLocationRoute);
            updateRequest.SetBodyWith(new IntermediatePoint(device.Location.Value));
            updateRequest.Parameters["identifier"] = device.Identifier;

            IEnumerable devicesSupportingRoutes = this.intAirAct.DevicesSupportingRoute(Routes.SetLocationRoute);
            foreach (IADevice iaDevice in devicesSupportingRoutes)
            {
                this.intAirAct.SendRequest(updateRequest, iaDevice, delegate(IAResponse response, Exception exception)
                {
                    if (exception != null)
                    {
                        failure(exception);
                        return;
                    }
                    else
                    {
                        success();
                    }
                });

                // Break, so that we only send the update to one server
                // How our system should function if there are multiple servers is undefined ... 
                break;
            }

        }

        public void UpdateDeviceOrientation(MSEDevice device, MSESuccessHandler success, MSEErrorHandler failure)
        {
            IARequest request = new IARequest(Routes.SetOrientationRoute);
            request.SetBodyWithString(device.Orientation.Value.ToString());
            request.Parameters["identifier"] = device.Identifier;

            IEnumerable devicesSupportingRoutes = this.intAirAct.DevicesSupportingRoute(Routes.SetOrientationRoute);
            foreach (IADevice iaDevice in devicesSupportingRoutes)
            {
                this.intAirAct.SendRequest(request, iaDevice, delegate(IAResponse response, Exception exception)
                {
                    if (exception != null)
                    {
                        failure(exception);
                        return;
                    }
                    else
                    {
                        success();
                    }
                });

                break;
            }
        }



        #endregion


        #region Locator route handling


        public void locate(MSEDevice device, MSESingleDeviceHandler success, MSEErrorHandler failure) {
            IARequest deviceRequest = new IARequest(Routes.GetDeviceInfoRoute);
            deviceRequest.Parameters["identifier"] = device.Identifier;

            IEnumerable devicesSupportingRoute = this.intAirAct.DevicesSupportingRoute(Routes.GetDeviceInfoRoute);

            foreach (IADevice iaDevice in devicesSupportingRoute)
            {
                this.intAirAct.SendRequest(deviceRequest, iaDevice, delegate(IAResponse response, Exception exception) {

                    String json = response.BodyAsString();

                    if (exception != null)
                    {
                        failure(exception);
                        return;
                    }
                    else
                    {
                        MSEDevice returnDevice = new MSEDevice();
                        IntermediateDevice intermediateDevice = response.BodyAs<IntermediateDevice>();

                        if (intermediateDevice == null)
                        {
                            success(null);
                            return;
                        }

                        returnDevice.Location = intermediateDevice.location;
                        returnDevice.Identifier = intermediateDevice.identifier;
                        returnDevice.Orientation = intermediateDevice.orientation;

                        if (returnDevice.Identifier == OwnIdentifier)
                        {
                            returnDevice.NetworkDevice = intAirAct.OwnDevice;
                        }
                        else
                        {
                            returnDevice.NetworkDevice = this.intAirAct.Devices.Find(d => d.Name.Equals(returnDevice.Identifier));
                        }
                        returnDevice.LastUpdated = DateTime.Now;

                        if (returnDevice.NetworkDevice == null)
                        {
                            success(null);
                        }
                        else
                        {
                            success(returnDevice);
                        }
                    }
                });

                // Break, so that we only send the query to one server
                // How our system should function if there are multiple servers is undefined ... 
                break;
            }
        }

        public void locateAllDevices(MSEDeviceCollectionHandler success, MSEErrorHandler failure)
        {

        }


        #endregion
    }
}
