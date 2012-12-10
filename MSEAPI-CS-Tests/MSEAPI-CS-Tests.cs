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

            while (!clientConnected && !serverConnected)
            {
                System.Threading.Thread.Sleep(100);
            }


            teardown();
        }

        public void setup()
        {
            client = IAIntAirAct.New();
            server = IAIntAirAct.New();
            clientConnected = false;
            serverConnected = false;

            client.DeviceFound += delegate(IADevice device, bool ownDevice)
            {
                if (ownDevice)
                    clientConnected = true;
            };

            server.DeviceFound += delegate(IADevice device, bool ownDevice)
            {
                if (ownDevice)
                    serverConnected = true;
            };


        }

        public void teardown()
        {

            client = null;
            server = null;
        }
    }
}
