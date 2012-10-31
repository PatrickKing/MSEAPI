using MSEKinect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using MSELocator;
using System.Windows;

namespace MSEKinectTests
{
    
    
    /// <summary>
    ///This is a test class for KinectCameraTest and is intended
    ///to contain all KinectCameraTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PersonManagerTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region Process Persons Tests

        internal PairablePerson CreateTestPerson(int newId, int? holdsId = null)
        {
            PairablePerson person = new PairablePerson
            {
                Identifier = newId.ToString(),
                HeldDeviceIdentifier = holdsId.ToString() ?? null,
                Location = null,
                Orientation = null,
                PairingState = PairingState.NotPaired
            };

            return person;
        }

        internal PairableDevice CreateTestDevice(int newId, int? heldById = null)
        {
            PairableDevice device = new PairableDevice
            {
                Identifier = newId.ToString(),
                HeldByPersonIdentifier = heldById.ToString() ?? null,
                PairingState = PairingState.NotPaired 
            };

            return device;
        }


        /// <summary>
        ///A happy path (standard) test for the Person Process functionality. In this test, we simulate a single person leaving the room.
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MSEKinect.dll")]
        public void ProcessPersonsOnFrameStandardTest()
        {
            
            PersonManager target = new PersonManager(null, null);

            //Empty list of devices
            List<PairableDevice> currentDevices = new List<PairableDevice>();

            //Current Persons = {1,2}
            List<PairablePerson> currentPersons = new List<PairablePerson>();
            currentPersons.Add(CreateTestPerson(1));
            currentPersons.Add(CreateTestPerson(2));

            //Update Persons = {2, 3}
            List<PairablePerson> updatedPersons = new List<PairablePerson>();
            updatedPersons.Add(CreateTestPerson(2));
            updatedPersons.Add(CreateTestPerson(3));

            List<PairablePerson> expected = updatedPersons; //The returned list should always be the same as the updated list

            //(!) Run Person Processing
            List<Person> actual = target.ProcessPersonsOnFrame(updatedPersons, currentPersons, currentDevices);

            bool listIsEqual = actual.SequenceEqual(expected);
            Assert.IsTrue(listIsEqual);
        }

        /// <summary>
        ///A test for the sitation where no person is being tracked and a person then becomes tracked in the system
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MSEKinect.dll")]
        public void ProcessPersonsOnFrameTestEmptyToOne()
        {
            PersonManager target = new PersonManager(null, null);

            //Empty list of devices
            List<PairableDevice> currentDevices = new List<PairableDevice>();

            //Current Persons Is Empty
            List<PairablePerson> currentPersons = new List<PairablePerson>();

            //Update Persons = {1}
            List<PairablePerson> updatedPersons = new List<PairablePerson>();
            updatedPersons.Add(CreateTestPerson(1));

            List<PairablePerson> expected = updatedPersons;

            //(!) Run PairablePerson Processing
            List<Person> actual = target.ProcessPersonsOnFrame(updatedPersons, currentPersons, currentDevices);

            bool listIsEqual = actual.SequenceEqual(expected);
            Assert.IsTrue(listIsEqual);
        }

        /// <summary>
        ///A test for ProcessPersonsOnFrame
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MSEKinect.dll")]
        public void ProcessPersonsOnFrameTestEmptyToEmpty()
        {
            PersonManager target = new PersonManager(null, null);

            //Empty list of devices
            List<PairableDevice> currentDevices = new List<PairableDevice>();

            //Current Persons Is Empty
            List<PairablePerson> currentPersons = new List<PairablePerson>();

            //Update Persons Is Empty
            List<PairablePerson> updatedPersons = new List<PairablePerson>();

            List<PairablePerson> expected = updatedPersons;

            //(!) Run PairablePerson Processing
            List<Person> actual = target.ProcessPersonsOnFrame(updatedPersons, currentPersons, currentDevices);

            bool listIsEqual = actual.SequenceEqual(expected);
            Assert.IsTrue(listIsEqual);
        }

        #endregion

        /// <summary>
        ///This test tries the standard case. The missing Persons list contains one person, who has a circule reference to the one PairableDevice stored in the currentlyConnectedDevicesList.
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MSEKinect.dll")]
        public void ProcessMissingPersonsStandardTest()
        {
            PersonManager target = new PersonManager(null, null);

            //Missing Persons = { PairablePerson (Id = 1, Holds-PairableDevice = 2) }
            List<PairablePerson> missingPersons = new List<PairablePerson>() 
            {
                CreateTestPerson(1,2)
            };

            //Current Connect Devices = { PairableDevice (Id = 2, Held-By-PairablePerson = 1) } 
            List<PairableDevice> currentConnectedDevices = new List<PairableDevice>() 
            {
                CreateTestDevice(2, 1)
            };

            //(!) Run Missign PairableDevice Processing 
            target.ProcessMissingPersons(missingPersons, currentConnectedDevices);

            List<PairableDevice> devicesWithHeldByIdentifiers = currentConnectedDevices.FindAll(device => device.HeldByPersonIdentifier != null);
            List<PairablePerson> personsWithHoldsDeviceIdentifiers = missingPersons.FindAll(person => person.HeldDeviceIdentifier != null);

            Assert.IsTrue(devicesWithHeldByIdentifiers.Count == 0, "Process Missing Persons Did Not Remove Held-By-PairablePerson Identifier");
            Assert.IsTrue(personsWithHoldsDeviceIdentifiers.Count == 0, "Process Missing Persons Did Not Remove Holds-PairableDevice Identifer");
        }

        /// <summary>
        ///This test tries the case where a missing person has a reference to a device that doesn't exist. For example,
        ///a person leaves and the device was turned off.
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MSEKinect.dll")]
        public void ProcessMissingPersonsTestDeviceDoesntExist()
        {
            PersonManager target = new PersonManager(null, null);

            //Missing person has a reference to a device that doesn't exist

            //Missing Persons = { PairablePerson (Id = 1, Holds-PairableDevice = 3) }
            List<PairablePerson> missingPersons = new List<PairablePerson>() 
            {
                CreateTestPerson(1,3)
            };

            //Current Connect Devices = empty 
            List<PairableDevice> currentConnectedDevices = new List<PairableDevice>();

            //(!) Run Missign PairableDevice Processing 
            target.ProcessMissingPersons(missingPersons, currentConnectedDevices);

            List<PairableDevice> devicesWithHeldByIdentifiers = currentConnectedDevices.FindAll(device => device.HeldByPersonIdentifier != null);
            List<PairablePerson> personsWithHoldsDeviceIdentifiers = missingPersons.FindAll(person => person.HeldDeviceIdentifier != null);

            Assert.IsTrue(devicesWithHeldByIdentifiers.Count == 0, "Process Missing Persons Did Not Remove Held-By-Person Identifier");
            Assert.IsTrue(personsWithHoldsDeviceIdentifiers.Count == 0, "Process Missing Persons Did Not Remove Holds-PairableDevice Identifer");
        }

        /// <summary>
        ///This test tries the case where there are no missing Persons and a device-person relationship already exists
        ///a person leaves and the device was turned off.
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MSEKinect.dll")]
        public void ProcessMissingPersonsTestNoMissingPersons()
        {
            PersonManager target = new PersonManager(null, null);

            //Missing Persons  = empty 
            List<PairablePerson> missingPersons = new List<PairablePerson>();

            //Current Connect Devices = { PairableDevice (Id = 2, Held-By-Person = 1) } 
            List<PairableDevice> currentConnectedDevices = new List<PairableDevice>() 
            {
                CreateTestDevice(2, 1)
            };

            //(!) Run Missign PairableDevice Processing 
            target.ProcessMissingPersons(missingPersons, currentConnectedDevices);

            List<PairableDevice> devicesWithHeldByIdentifiers = currentConnectedDevices.FindAll(device => device.HeldByPersonIdentifier != null);

            Assert.IsTrue(devicesWithHeldByIdentifiers.Count > 0, "Process Missing Persons Did Not Remove Held-By-Person Identifier");
        }

        /// <summary>
        ///This test tries the case where there are no missing persons and no devices paired with a person
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MSEKinect.dll")]
        public void ProcessMissingPersonsTest()
        {
            PersonManager target = new PersonManager(null, null);

            //Missing Persons  = empty 
            List<PairablePerson> missingPersons = new List<PairablePerson>();

            //Current Connect Devices = empty 
            List<PairableDevice> currentConnectedDevices = new List<PairableDevice>();

            target.ProcessMissingPersons(missingPersons, currentConnectedDevices);
            List<PairableDevice> devicesWithHeldByIdentifiers = currentConnectedDevices.FindAll(device => device.HeldByPersonIdentifier != null);
            List<PairablePerson> personsWithHoldsDeviceIdentifiers = missingPersons.FindAll(person => person.HeldDeviceIdentifier != null);

            Assert.IsTrue(devicesWithHeldByIdentifiers.Count == 0, "Process Missing Persons Did Not Remove Held-By-Person Identifier");
            Assert.IsTrue(personsWithHoldsDeviceIdentifiers.Count == 0, "Process Missing Persons Did Not Remove Holds-PairableDevice Identifer");

        }
    }
}
