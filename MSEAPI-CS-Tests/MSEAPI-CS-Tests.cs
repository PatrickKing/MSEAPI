using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IntAirAct;
using MSEAPI_SharedNetworking;
using MSEAPI_CS;
using MSEAPI_CS.Models;
using System.Windows;
using MSEKinect;
using MSELocator;

namespace MSEAPI_CS_Tests
{
    /// <summary>
    /// Integration tests for the MSEAPI WPF Client and MSEAPI Server (KinectManager).
    /// Important! Run these tests without any other MSE instances on the network.
    /// The simplest way: temporarily disable your wireless adapter.
    /// </summary>
    [TestClass]
    public class MSEAPI_CS_Tests
    {
        #region Constants and Instance Variables

        // The test client and server IntAirAct instances use some fixed arbitrary ports, so that they can uniquely identify each other.
        static ushort ServerPort = 4000;
        static ushort ClientPort = 5000;

        MSEMultiSurface Client;
        MSEKinectManager Server;

        bool clientConnected;
        bool serverConnected;
        bool doneWaitingForResponse;

        #endregion

        #region Setup, Teardown, Helper methods

        public void Setup()
        {
            Client = new MSEMultiSurface();
            Server = new MSEKinectManager(false);
            clientConnected = false;
            serverConnected = false;
            doneWaitingForResponse = false;

            Client.IntAirAct.Port = ClientPort;
            Server.IntAirAct.Port = ServerPort;

            // Increment the port numbers, so that if the current test run crashes, we don't try to use unreclaimed ports on the next test
            ClientPort++;
            ServerPort++;

            // In the tests, we wait on clientConnected and serverConnected, to be certain that each IntAirAct instance has registered the other
            // We use known ports to 'uniquely' identify instances during the test, since other IntAirAct devices may exist on the network during the test
            Client.IntAirAct.DeviceFound += delegate(IADevice device, bool ownDevice)
            {
                if (device.Port == Server.IntAirAct.Port)
                    clientConnected = true;
            };

            Server.IntAirAct.DeviceFound += delegate(IADevice device, bool ownDevice)
            {
                if (device.Port == Client.IntAirAct.Port)
                    serverConnected = true;
            };


        }

        public void Teardown()
        {
            Client.Stop();
            Server.Stop();

            Client = null;
            Server = null;
        }

        /// <summary>
        /// Wait for both client and server to detect each other before proceeding
        /// </summary>
        public void WaitForConnections()
        {
            int attempts = 0;

            while (!(clientConnected && serverConnected))
            {
                attempts++;
                //TODO: A test will occasionally fail due to this timeout. Might need to be bumped up. 
                if (attempts > 150)
                {
                    Assert.Fail();
                    break;
                }
                System.Threading.Thread.Sleep(100);
            }
        }


        /// <summary>
        /// Busy wait for networking response to get in
        /// </summary>
        public void WaitForResponse()
        {
            int attempts = 0;
            while (!doneWaitingForResponse)
            {
                attempts++;
                if (attempts > 150)
                {
                    Assert.Fail();
                    break;
                }
                System.Threading.Thread.Sleep(100);
            }
        }


        #endregion

        #region Locator method tests

        // TODO: The server side API will change, so that a 'no result' comes back as a 404 error and not a 200 with no message body.
        // This test will need to be fixed up then.
        [TestMethod]
        public void GetDeviceInfoTestReturningNull()
        {
            Setup();

            Server.Start();
            Client.Start();
            WaitForConnections();

            Client.locate(new MSEDevice() { Identifier="foo"}, delegate(MSEDevice successDevice)
            {
                Assert.IsNull(successDevice);
                doneWaitingForResponse = true;
            },
            delegate(Exception exception)
            {
                Assert.Fail();
            });

            WaitForResponse();
            Teardown();
        }




        // So, this test doesn't work right now, because MSEMultiSurface only returns devices that also have a network device. Options:
        // 1 - Make the client the device we're requesting information about
        // 2 - Make a third intAirAct entity to be the 'device'
        // 3 - Hack a network device into intAirAct's array on the client... 
        [TestMethod]
        public void GetDeviceInfoTest()
        {
            Setup();
            //Server.Route(Routes.GetDeviceInfoRoute, delegate(IARequest request, IAResponse response) 
            //{
            //    if(request.Parameters["identifier"].Equals(Client.OwnIdentifier))
            //    {
            //        // This string derived from the actual MSEAPI JSON serializer. Use real data for all tests! 
            //        response.SetBodyWithString("{\"identifier\":\"" + Client.OwnIdentifier + "\",\"orientation\":99.55555,\"location\":\"2.22716139629483,3.0686103105545\"}");
            //    }
            //});

            Device testDevice = new Device 
            { Identifier = "testDevice", 
                Location = new Point(2.227, 3.0686), 
                Orientation = 99.55555 
            };
            Server.Locator.Devices.Add(testDevice);

            Server.Start();
            Client.Start();
            WaitForConnections();

            Client.locate(new MSEDevice() { Identifier= "testDevice"}, delegate(MSEDevice successDevice)
            {
                Assert.AreEqual(testDevice.Orientation.Value, successDevice.Orientation.Value, 0.001, "Orientation not equal");
                Assert.AreEqual(testDevice.Location.Value, successDevice.Location.Value, "Location not equal");
                Assert.AreEqual(testDevice.Identifier, successDevice.Identifier, "Identifier not equal");

                doneWaitingForResponse = true;
            },
            delegate(Exception exception)
            {
                Assert.Fail();
            });

            WaitForResponse();
            Teardown();

        }
#endregion


        /// <summary>
        /// A test of the client's function for sending device location updates
        /// </summary>
        /// 
        /*
        [TestMethod]
        public void SetDeviceLocationTest()
        {
            Setup();
            Server.Route(Routes.SetLocationRoute, delegate(IARequest request, IAResponse response)
            {
                if (!request.Parameters["identifier"].Equals(Client.OwnIdentifier))
                {
                    response.StatusCode = 404;
                }

                IntermediatePoint updatePoint = request.BodyAs<IntermediatePoint>();
                Assert.AreEqual(updatePoint.X, 10.0, 0.1);
                Assert.AreEqual(updatePoint.Y, 5.0, 0.1);


            });

            Server.Start();
            Client.Start();
            WaitForConnections();

            Client.UpdateDeviceLocation(new MSEDevice() { Identifier = Client.OwnIdentifier, Location = new Point(10, 5) }, delegate()
            {
                doneWaitingForResponse = true;
            },
            delegate(Exception exception)
            {
                Assert.Fail();
            });

            WaitForResponse();

            Teardown();
        }
        */


    }
}
