using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IntAirAct;
using MSEAPI_SharedNetworking;
using MSEAPI_CS;
using MSEAPI_CS.Models;

namespace MSEAPI_CS_Tests
{
    [TestClass]
    public class MSEAPI_CS_Tests
    {

        #region Constants and Instance Variables

        // The test client and server IntAirAct instances use some fixed arbitrary ports, so that they can uniquely identify each other.
        const int SERVER_PORT = 3474;
        const int CLIENT_PORT = 34789;

        //IAIntAirAct client;
        MSEMultiSurface client;
        IAIntAirAct server;

        bool clientConnected;
        bool serverConnected;
        bool doneWaitingForResponse;

        #endregion

        #region Setup, Teardown, Helper methods

        public void Setup()
        {
            //client = IAIntAirAct.New();
            client = new MSEMultiSurface();
            server = IAIntAirAct.New();
            clientConnected = false;
            serverConnected = false;
            doneWaitingForResponse = false;

            client.IntAirAct.Port = CLIENT_PORT;
            server.Port = SERVER_PORT;

            client.IntAirAct.DeviceFound += delegate(IADevice device, bool ownDevice)
            {
                if (device.Port == SERVER_PORT)
                    clientConnected = true;
            };

            server.DeviceFound += delegate(IADevice device, bool ownDevice)
            {
                if (device.Port == CLIENT_PORT)
                    serverConnected = true;
            };


        }

        public void Teardown()
        {

            client = null;
            server = null;
        }

        // TODO: both of the 'wait' functions need an eventual timeout

        /// <summary>
        /// Wait for both client and server to detect each other before proceeding
        /// </summary>
        public void WaitForConnections()
        {
            while (!(clientConnected && serverConnected))
            {
                System.Threading.Thread.Sleep(100);
            }
        }


        /// <summary>
        /// Busy wait for networking response to get in
        /// </summary>
        public void WaitForResponse()
        {
            while (!doneWaitingForResponse)
            {
                System.Threading.Thread.Sleep(100);
            }
        }


        #endregion


#region Locator method tests

        // TODO: The server side API will change, so that a 'no result' comes back as a 404 error and not a 200 with no message body.
        // This test will be obsolete then.
        [TestMethod]
        public void GetDeviceInfoTestReturningNull()
        {
            Setup();
            server.Route(Routes.GetDeviceInfoRoute, delegate(IARequest request, IAResponse response) 
            {
                response.SetBodyWithString("");
            });

            server.Start();
            client.Start();
            WaitForConnections();

            client.locate(new MSEDevice() { Identifier="foo"}, delegate(MSEDevice successDevice)
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



        [TestMethod]
        public void GetDeviceInfoTest()
        {
            Setup();
            server.Route(Routes.GetDeviceInfoRoute, delegate(IARequest request, IAResponse response) 
            {
                if(request.Parameters["identifier"].Equals("foo"))
                {
                    // This string obtained from the actual MSEAPI JSON serializer. Use real data for all tests! 
                    response.SetBodyWithString("{\"identifier\":\"ASE Lab iPad 3\",\"orientation\":99.55555,\"location\":\"2.22716139629483,3.0686103105545\"}");
                }
            });

            server.Start();
            client.Start();
            WaitForConnections();

            client.locate(new MSEDevice() { Identifier="foo"}, delegate(MSEDevice successDevice)
            {
                Assert.AreEqual(successDevice.Orientation.Value, 99.55555, 0.001, "Orientation not equal");
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


        [TestMethod]
        public void SetDeviceLocationAndOrientationTest()
        {
            Setup();

            Teardown();

        }



    }
}
