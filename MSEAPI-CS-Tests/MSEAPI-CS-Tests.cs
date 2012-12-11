using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IntAirAct;
using MSEAPI_SharedNetworking;

namespace MSEAPI_CS_Tests
{
    [TestClass]
    public class MSEAPI_CS_Tests
    {
        const int SERVER_PORT = 3474;
        const int CLIENT_PORT = 34789;


        IAIntAirAct client;
        IAIntAirAct server;
        bool clientConnected;
        bool serverConnected;


        [TestMethod]
        public void GetDeviceInfoTestReturningNull()
        {
            setup();


            server.Route(Routes.GetDeviceInfoRoute, delegate(IARequest request, IAResponse response) {

                request.SetBodyWithString("");
            
            });

            server.Start();
            client.Start();



            teardown();
        }

        public void setup()
        {
            client = IAIntAirAct.New();
            server = IAIntAirAct.New();
            clientConnected = false;
            serverConnected = false;

            client.Port = CLIENT_PORT;
            server.Port = SERVER_PORT;

            client.DeviceFound += delegate(IADevice device, bool ownDevice)
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

        public void teardown()
        {

            client = null;
            server = null;
        }

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

    }
}
