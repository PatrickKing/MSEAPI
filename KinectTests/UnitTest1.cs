using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IntAirAct;
using MSEKinect;
using MSEAPI_SharedNetworking;

namespace KinectTests
{
    [TestClass]
    public class UnitTest1
    {
        #region Instance Variables

        // The test client and server IntAirAct instances use some fixed arbitrary ports, so that they can uniquely identify each other.
        static ushort ServerPort = 4000;
        static ushort ClientPort = 5000;

        IAIntAirAct Client;
        MSEKinectManager Server;

        bool clientConnected;
        bool serverConnected;
        bool doneWaitingForResponse;

        #endregion

        IAIntAirAct ia;
        MSEKinectManager mse;

        [TestMethod]
        public void TestMethod1()
        {
            ia = IAIntAirAct.New();
            ia.Start();
            ia.Stop();

            mse = new MSEKinectManager();

            //Setup();
            //Teardown();

        }


        public void Setup()
        {
            //client = IAIntAirAct.New();
            Client = IAIntAirAct.New();
            Server = new MSEKinectManager();
            clientConnected = false;
            serverConnected = false;
            doneWaitingForResponse = false;

            Client.Port = ClientPort;
            Server.IntAirAct.Port = ServerPort;

            // Increment the port numbers, so that if the current test run crashes, we don't try to use unreclaimed ports on the next test
            ClientPort++;
            ServerPort++;

            // In the tests, we wait on clientConnected and serverConnected, to be certain that each IntAirAct instance has registered the other
            // We use known ports to 'uniquely' identify instances during the test, since other IntAirAct devices may exist on the network during the test
            Client.DeviceFound += delegate(IADevice device, bool ownDevice)
            {
                if (device.Port == Server.IntAirAct.Port)
                    clientConnected = true;
            };

            Server.IntAirAct.DeviceFound += delegate(IADevice device, bool ownDevice)
            {
                if (device.Port == Client.Port)
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

    }
}
