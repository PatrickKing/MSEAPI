using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSEKinect;
using IntAirAct;
using MSEAPI_SharedNetworking;
using System.Windows;

namespace MSEKinectTests
{
    //TODO: Find a way to factor out what's in common between this class and the MSEAPI-CS-Tests main class


    /// <summary>
    /// Integration tests for the network communications on MSEKinect. 
    /// NB: Run these tests WITHOUT a Kinect attached! 
    /// 
    /// NB: These tests depend on the networking libraries, running them in a row can cause failures due to being set up and shut down repeatedly, apparently. 
    /// I don't know of any good fix for this, except for re-running tests that fail until it is obivous that the failure is not timing or networking cleanup related.
    /// </summary>
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
            Client = IAIntAirAct.New();
            Server = new MSEKinectManager(RequireKinect: false);
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

            // Wait a few seconds at the end of each test, to allow networking dependencies to clean up
            System.Threading.Thread.Sleep(2000);
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
                if (attempts > 250)
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

        [TestMethod]
        public void SuccessfulPairingTest()
        {
            Setup();

            // We would like to be able to test the round trip communication, sending 'request pairing' on the client to the server, to sending
            // 'become paired' from server to client. 
            // But internally, tinyIOC registers things by type, so only the first instance of intairact's server adapter gets retrieved. 
            // All routes are handled by the same IAA instance, which is obviously a problem when we have two in the system that we would like to have talk to each other.

            // So, at present this part of the test cannot work
            //Client.Route(Routes.BecomePairedRoute, delegate(IARequest request, IAResponse response)
            //{
            //    doneWaitingForResponse = true;
            //});

            Server.Start();
            Client.Start();
            WaitForConnections();

            // Create a person on the server, who is attempting to pair their device
            // NB: Setting PairingAttempt on a Person begins a 3 second timer, after which their pairing state resets to NotPaired
            // The test should always complete before then, but if mysterious failures start appearing, it could be time related
            Server.Locator.Persons.Add(new PairablePerson()
            {
                Identifier = "Bob",
                Location = new System.Windows.Point(1, 1),
                PairingState = PairingState.PairingAttempt
            });

            // Notify the server that the client wants to be paired
            IARequest pairingRequest = new IARequest(Routes.RequestPairingRoute);
            pairingRequest.Origin = Client.OwnDevice;
            pairingRequest.Parameters["identifier"] = Client.OwnDevice.Name; 
            Client.SendRequest(pairingRequest, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception exception)
            {
                if (exception != null)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    Assert.Fail();
                }

                // In response to the return of the request, we test some properties on the server
                
                // Find the mock pairable person we created, test their pairing state
                PairablePerson person = (PairablePerson)Server.Locator.Persons.Find(x => x.Identifier.Equals("Bob"));
                Assert.AreEqual(PairingState.Paired, person.PairingState);

                // Find the Client's IADevice on the server, test its pariing state
                PairableDevice device = (PairableDevice)Server.Locator.Devices.Find(x => x.Identifier.Equals(Client.OwnDevice.Name));
                Assert.AreEqual(PairingState.Paired, device.PairingState);

                // Test that the two were paired with each other
                Assert.AreEqual(person.HeldDeviceIdentifier, device.Identifier);
                Assert.AreEqual(device.HeldByPersonIdentifier, person.Identifier);

                doneWaitingForResponse = true;
            });

            WaitForResponse();
            Teardown();
        }

        #endregion

        #region Device Property Route Tests

        [TestMethod]
        public void GetOffsetAngleTest()
        {
            Setup();

            // Create a person and device, paired and positioned
            PairablePerson person = new PairablePerson
            {
                PairingState = PairingState.Paired,
                Identifier = "Bob",
                Location = new Point(1, 1),
                HeldDeviceIdentifier = "myPad"
            };

            PairableDevice device = new PairableDevice
            {
                HeldByPersonIdentifier = "Bob",
                Identifier = "myPad",
                Location = new Point(1, 1),
                PairingState = PairingState.Paired
            };

            // Position the tracker
            //Server.PersonManager.Tracker.Location = new Point(0, 2);
            //Server.PersonManager.Tracker.Orientation = 270;
            Server.Locator.Devices.Add(device);
            Server.Locator.Persons.Add(person);

            Server.Start();
            Client.Start();
            WaitForConnections();

            IARequest request = new IARequest(Routes.GetOffsetAngleRoute);
            request.Parameters["identifier"] = "myPad";

            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception exception)
            {
                double offsetOrientation = double.Parse(response.BodyAsString());
                // The angle between the device and the tracker should be 135 degrees
                Assert.AreEqual(135, offsetOrientation, 0.01);
            });


            Teardown();
        }

        [TestMethod]
        public void SetOrientationTest()
        {
            Setup();

            // Set up a device
            PairableDevice device = new PairableDevice
            {
                Identifier = "myPad",
                Orientation = 20.0
            };
            Server.Locator.Devices.Add(device);

            Server.Start();
            Client.Start();
            WaitForConnections();

            // Build a request to set the device's orientation
            IARequest request = new IARequest(Routes.SetOrientationRoute);
            request.Parameters["identifier"] = "myPad";
            double newOrientation = 240.0;
            request.SetBodyWithString(newOrientation.ToString());

            // Send the request, and test
            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception exception)
            {
                Assert.AreEqual(240.0, device.Orientation.Value, 0.01);
            });


            Teardown();
        }




        [TestMethod]
        public void SetLocationTest()
        {
            Setup();

            // Set up a device
            PairableDevice device = new PairableDevice
            {
                Identifier = "myPad",
                Location = new Point(1,1)
            };
            Server.Locator.Devices.Add(device);

            Server.Start();
            Client.Start();
            WaitForConnections();

            // Build a request to set the device's orientation
            IARequest request = new IARequest(Routes.SetOrientationRoute);
            request.Parameters["identifier"] = "myPad";
            Point newLocation = new Point(2,2);
            request.SetBodyWith(new IntermediatePoint(newLocation));

            // Send the request, and test
            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception exception)
            {
                Assert.AreEqual(newLocation, device.Location.Value);
            });

            Teardown();
        }


        #endregion

        #region Locator Route Tests

        [TestMethod]
        public void GetDeviceInfoTest()
        {
            Setup();

            PairableDevice deviceOne = new PairableDevice
            {
                Location = new Point(1,0),
                Orientation = 90,
                Identifier = "deviceOne",
            };

            PairableDevice deviceTwo = new PairableDevice
            {
                Location = new Point(-1,0),
                Identifier = "deviceTwo",
            };

            Server.Locator.Devices.Add(deviceOne);
            Server.Locator.Devices.Add(deviceTwo);


            Server.Start();
            Client.Start();
            WaitForConnections();

            // Successful get device info test
            IARequest request = new IARequest(Routes.GetDeviceInfoRoute);
            request.Parameters["identifier"] = deviceOne.Identifier;

            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception e)
            {
                IntermediateDevice id = response.BodyAs<IntermediateDevice>();

                Assert.AreEqual("deviceOne", id.identifier);
                Assert.AreEqual(new Point(1, 0), id.location.Value);
                Assert.AreEqual(90.0, id.orientation.Value, 0.01);

                doneWaitingForResponse = true;
            });

            WaitForResponse();
            doneWaitingForResponse = false;

            // Unsuccessful get device info test
            // Device two is incomplete, missing orientation, the correct server behaviour is to return http status 404
            request = new IARequest(Routes.GetDeviceInfoRoute);
            request.Parameters["identifier"] = deviceTwo.Identifier;

            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception e)
            {
                Assert.AreEqual(404, response.StatusCode);
                doneWaitingForResponse = true;
            });

            WaitForResponse();

            Teardown();
        }


        [TestMethod]
        public void GetAllDeviceInfoTest()
        {
            Setup();

            PairableDevice deviceOne = new PairableDevice
            {
                Location = new Point(1, 0),
                Orientation = 90,
                Identifier = "deviceOne",
            };

            PairableDevice deviceTwo = new PairableDevice
            {
                Location = new Point(-1, 0),
                Identifier = "deviceTwo",
                Orientation = 20,
            };

            Server.Locator.Devices.Add(deviceOne);
            Server.Locator.Devices.Add(deviceTwo);

            Server.Start();
            Client.Start();
            WaitForConnections();

            IARequest request = new IARequest(Routes.GetAllDeviceInfoRoute);

            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception e)
            {
                List<IntermediateDevice> ids = response.BodyAs<IntermediateDevice>();


                IntermediateDevice intDevice = ids.Find(x => x.identifier.Equals("deviceOne"));
                Assert.AreEqual(deviceOne.Location, intDevice.location);
                Assert.AreEqual(deviceOne.Orientation, intDevice.orientation.Value);
                intDevice = ids.Find(x => x.identifier.Equals("deviceTwo"));
                Assert.AreEqual(deviceTwo.Location, intDevice.location);
                Assert.AreEqual(deviceTwo.Orientation, intDevice.orientation.Value);


                doneWaitingForResponse = true;
            });

            WaitForResponse();

            Teardown();
        }


        [TestMethod]
        public void NearestDeviceInViewTest()
        {
            Setup();

            PairableDevice observer = new PairableDevice
            {
                Location = new Point(0, 0),
                Orientation = 225,
                Identifier = "observer",
            };

            PairableDevice nearest = new PairableDevice
            {
                Location = new Point(-1, -1),
                Identifier = "nearest",
                Orientation = 20,
            };

            PairableDevice furthest = new PairableDevice
            {
                Location = new Point(-2, -2),
                Identifier = "furthest",
                Orientation = 20,
            };

            Server.Locator.Devices.Add(observer);
            Server.Locator.Devices.Add(furthest);
            Server.Locator.Devices.Add(nearest);

            Server.Start();
            Client.Start();
            WaitForConnections();

            IARequest request = new IARequest(Routes.GetNearestDeviceInViewRoute);
            request.Parameters["identifier"] = observer.Identifier;

            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception e)
            {
                IntermediateDevice intDevice = response.BodyAs<IntermediateDevice>();
                Assert.AreEqual(nearest.Identifier, intDevice.identifier);
                Assert.AreEqual(nearest.Location, intDevice.location);
                Assert.AreEqual(nearest.Orientation, intDevice.orientation);

                doneWaitingForResponse = true;
            });

            WaitForResponse();
            Teardown();
        }


        [TestMethod]
        public void AllDevicesInViewTest()
        {
            Setup();
            PairableDevice observer = new PairableDevice
            {
                Location = new Point(0, 0),
                Orientation = 225,
                Identifier = "observer",
            };

            PairableDevice nearest = new PairableDevice
            {
                Location = new Point(-1, -1),
                Identifier = "nearest",
                Orientation = 20,
            };

            PairableDevice furthest = new PairableDevice
            {
                Location = new Point(-2, -2),
                Identifier = "furthest",
                Orientation = 20,
            };

            Server.Locator.Devices.Add(observer);
            Server.Locator.Devices.Add(furthest);
            Server.Locator.Devices.Add(nearest);

            Server.Start();
            Client.Start();
            WaitForConnections();

            IARequest request = new IARequest(Routes.GetAllDevicesInViewRoute);
            request.Parameters["identifier"] = observer.Identifier;

            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception e)
            {
                List<IntermediateDevice> intDevices = response.BodyAs<IntermediateDevice>();

                IntermediateDevice intDevice = intDevices.Find(x => x.identifier == nearest.Identifier);
                Assert.AreEqual(nearest.Location, intDevice.location);
                Assert.AreEqual(nearest.Orientation, intDevice.orientation);

                intDevice = intDevices.Find(x => x.identifier == furthest.Identifier);
                Assert.AreEqual(furthest.Location, intDevice.location);
                Assert.AreEqual(furthest.Orientation, intDevice.orientation);

                doneWaitingForResponse = true;
            });

            WaitForResponse();
            Teardown();
        }


        [TestMethod]
        public void NearestDeviceInRangeTest()
        {
            Setup();

            PairableDevice observer = new PairableDevice
            {
                Location = new Point(10, 10),
                Orientation = 0,
                Identifier = "observer",
            };

            PairableDevice nearest = new PairableDevice
            {
                Location = new Point(0, 5),
                Identifier = "nearest",
                Orientation = 0,
            };

            PairableDevice furthest = new PairableDevice
            {
                Location = new Point(-5, 0),
                Identifier = "furthest",
                Orientation = 0,
            };

            Server.Locator.Devices.Add(observer);
            Server.Locator.Devices.Add(furthest);
            Server.Locator.Devices.Add(nearest);

            Server.Start();
            Client.Start();
            WaitForConnections();

            IARequest request = new IARequest(Routes.GetNearestDeviceInRangeRoute);
            request.Parameters["identifier"] = observer.Identifier;
            request.Parameters["range"] = "100.0";

            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception e)
            {
                IntermediateDevice intDevice = response.BodyAs<IntermediateDevice>();
                Assert.AreEqual(nearest.Identifier, intDevice.identifier);
                Assert.AreEqual(nearest.Location, intDevice.location);
                Assert.AreEqual(nearest.Orientation, intDevice.orientation);

                doneWaitingForResponse = true;
            });

            WaitForResponse();
            Teardown();
        }


        [TestMethod]
        public void AllDevicesWithinRangeTest()
        {
            Setup();

            PairableDevice observer = new PairableDevice
            {
                Location = new Point(10, 10),
                Orientation = 0,
                Identifier = "observer",
            };

            PairableDevice nearest = new PairableDevice
            {
                Location = new Point(0, 5),
                Identifier = "nearest",
                Orientation = 0,
            };

            PairableDevice furthest = new PairableDevice
            {
                Location = new Point(-5, 0),
                Identifier = "furthest",
                Orientation = 0,
            };

            PairableDevice tooFar = new PairableDevice
            {
                Location = new Point(100,100),
                Identifier = "tooFar",
                Orientation = 50,
            };

            Server.Locator.Devices.Add(observer);
            Server.Locator.Devices.Add(furthest);
            Server.Locator.Devices.Add(nearest);
            Server.Locator.Devices.Add(tooFar);

            Server.Start();
            Client.Start();
            WaitForConnections();

            IARequest request = new IARequest(Routes.GetAllDevicesInRangeRoute);
            request.Parameters["identifier"] = observer.Identifier;
            request.Parameters["range"] = "50.0";


            Client.SendRequest(request, Server.IntAirAct.OwnDevice, delegate(IAResponse response, Exception e)
            {
                List<IntermediateDevice> intDevices = response.BodyAs<IntermediateDevice>();

                IntermediateDevice intDevice = intDevices.Find(x => x.identifier == nearest.Identifier);
                Assert.AreEqual(nearest.Location, intDevice.location);
                Assert.AreEqual(nearest.Orientation, intDevice.orientation);

                intDevice = intDevices.Find(x => x.identifier == furthest.Identifier);
                Assert.AreEqual(furthest.Location, intDevice.location);
                Assert.AreEqual(furthest.Orientation, intDevice.orientation);

                // The 'tooFar' device should not be returned
                Assert.AreEqual(2, intDevices.Count);

                doneWaitingForResponse = true;
            });

            WaitForResponse();
            Teardown();
        }


        #endregion

    }
}
