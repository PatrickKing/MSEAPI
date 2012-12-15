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
    //TODO: Find a way to factor out what's in common between this class and the MSEAPI-CS-Tests main class

    [TestClass]
    public class CommunicationManagerTest
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

        #region Setup, Teardown, Helper methods

        public void Setup()
        {
            Server = new MSEKinectManager(RequireKinect: false);
            Client = IAIntAirAct.New();
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

        //    Server.Start();
        //    Client.Start();
        //    WaitForConnections();

        //    Teardown();
        //}

        #region Pairing Route Tests




        /// <summary>
        /// So, as written, this test can never succeed.
        /// IntAirAct
        /// </summary>
        [TestMethod]
        public void SuccessfulPairingTest()
        {
            Setup();

            // Setup the 'became paired' route on the client.
            Client.Route(Routes.BecomePairedRoute, delegate(IARequest request, IAResponse response)
            {
                // In response to receiving 'became paired', we test some properties on the server

                // Find the mock pairable person we created, test their pairing state
                PairablePerson person = (PairablePerson) Server.Locator.Persons.Find(x => x.Identifier.Equals("Bob"));
                Assert.AreEqual(PairingState.Paired, person.PairingState);

                // Find the Client's IADevice on the server, test its pariing state
                PairableDevice device = (PairableDevice)Server.Locator.Devices.Find(x => x.Identifier.Equals(Client.OwnDevice.Name));
                Assert.AreEqual(PairingState.Paired, device.PairingState);

                // Test that the two were paired with each other
                Assert.AreEqual(person.HeldDeviceIdentifier, device.Identifier);
                Assert.AreEqual(device.HeldByPersonIdentifier, person.Identifier);

                doneWaitingForResponse = true;
            });

            Server.Start();
            Client.Start();
            WaitForConnections();

            // Create a person on the server, who is attempting to pair their device
            // NB: Setting PairingAttempt on a person begins a 3 second timer, after which it resets to NotPaired
            // The test should always complete before then, though
            Server.Locator.Persons.Add(new PairablePerson()
            {
                Identifier = "Bob",
                Location = new System.Windows.Point(1, 1),
                PairingState = PairingState.PairingAttempt
            });

            // Notify the server that the client wants to be paired
            IARequest pairingRequest = new IARequest(Routes.RequestPairingRoute);
            pairingRequest.Origin = Client.OwnDevice;
            Client.SendRequest(pairingRequest, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception exception)
            {
                if (exception != null)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                }
            });

            WaitForResponse();
            Teardown();
        }



        #endregion

        #region Device Property Route Tests
        #endregion

        #region Locator Route Tests
        #endregion

    }
}
