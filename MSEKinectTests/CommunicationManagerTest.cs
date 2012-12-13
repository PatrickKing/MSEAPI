using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSEKinect;
using IntAirAct;
using MSEAPI_SharedNetworking;

namespace MSEKinectTests
{
    [TestClass]
    //TODO: Find a way to factor out what's in common between this class and the MSEAPI-CS-Tests main class
    public class CommunicationManagerTest
    {
        #region Constants and Instance Variables

        // The test client and server IntAirAct instances use some fixed arbitrary ports, so that they can uniquely identify each other.
        static ushort ServerPort = 4000;
        static ushort ClientPort = 5000;

        IAIntAirAct Client;
        MSEKinectManager Server;

        bool clientConnected;
        bool serverConnected;
        bool doneWaitingForResponse;

        #endregion

        #region Setup, Teardown, Helper methods

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

        /// <summary>
        /// Wait for both client and server to detect each other before proceeding
        /// </summary>
        public void WaitForConnections()
        {
            int attempts = 0;

            while (!(clientConnected && serverConnected))
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


        //[TestMethod]
        //public void TestTemplate()
        //{
        //    Setup();

        //    Teardown();
        //}

        #region Pairing Route Tests


        [TestMethod]
        public void SuccessfulPairingTest()
        {
            Setup();



            Teardown();
        }

        [TestMethod]
        public void FailedPairingTest()
        {
            Setup();

            Teardown();
        }


        #endregion

        #region Device Property Route Tests
        #endregion

        #region Locator Route Tests
        #endregion

    }
}
