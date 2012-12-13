using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using MSEAPI_SharedNetworking;
using MSEAPI_CS.Models;
using IntAirAct;

namespace MSEAPI_CS
{
    public class MSEMultiSurface
    {
        #region Instance Variables
        public ArrayList recievedImageHandlers;
        public ArrayList recievedDataHandlers;

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
        #endregion

        #region Constructor, Start and Stop

        public MSEMultiSurface()
        {
            //Setup Logging
            logger = new TraceSource("MSEMultiSurface");

            this.isRunning = false;

            //Initialize Our Arrays of Handlers
            //this.recievedImageHandlers


            //Setup IntAirAct
            this.intAirAct = IAIntAirAct.New();

#if DEBUG
            this.IntAirAct.Port = 12345;
#endif

            //Setup Pairing Recognizer




            //Setup Locator





            //Setup MSE Standard Routes
            //this.setupRoutes();
            //this.setupGestureHandlers();

        }

        public void Start()
        {
            if (this.isRunning)
            {
                return;
            }
            else
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

        #region Data, Image and Object route handling

        public void setupRoutes()
        {
            //Setup Received Data Handlers
            this.setupDataRoute();
            this.setupImageRoute();
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
        #endregion



        /// <summary>
        /// Notify the server of the device's current Location
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


        #region Locator route handling


        public void locate(MSEDevice device, MSESingleDeviceHandler success, MSEErrorHandler failure) {
            IARequest deviceRequest = new IARequest(Routes.GetDeviceInfoRoute);
            deviceRequest.Parameters["identifier"] = device.Identifier;

            IEnumerable devicesSupportingRoutes = this.intAirAct.DevicesSupportingRoute(Routes.GetDeviceInfoRoute);

            foreach (IADevice iaDevice in devicesSupportingRoutes)
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
