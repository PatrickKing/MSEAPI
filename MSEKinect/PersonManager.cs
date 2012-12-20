using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;
using System.Timers;
using IntAirAct;
using MSEGestureRecognizer;
using MSELocator;

using System.Windows;

using MSEAPI_SharedNetworking;

namespace MSEKinect
{
    public class PersonManager
    {

        #region Constants
        // Distance in meters on either side on both axis that if a new skeleton appears in this area and a device has been unpaired within the timer, it'll automatically pair this skeleton/device
        const double SAVINGDISTANCE = 0.25;



        #endregion



        #region Instance Variables

        private static TraceSource logger = new TraceSource("MSEKinect");
        KinectSensor ks;
        GestureController gestureController;
        LocatorInterface locator;
        IAIntAirAct intAirAct;

        Tracker tracker;
        public Tracker Tracker 
        { 
            get { return tracker; } 
            private set 
            { 
                tracker = value;
                TrackerSet(this, tracker);
            }
        }

        #endregion

        #region Events

        public delegate void PersonChangedEventSignature(PersonManager sender, PairablePerson person);
        public event PersonChangedEventSignature PersonAdded;
        public event PersonChangedEventSignature PersonRemoved;

        public delegate void TrackerChangedEventSignature(PersonManager sender, Tracker tracker);
        public event TrackerChangedEventSignature TrackerSet;

        #endregion

        #region Constructor, Start and Stop

        public PersonManager(LocatorInterface locator, GestureController gc, IAIntAirAct intAirAct)
        {
            this.gestureController = gc;
            this.locator = locator;
            this.intAirAct = intAirAct;

            tracker = new Tracker() { Location = new Point(0, 0), Orientation = 0, Identifier = "MSEKinect" };
            locator.Trackers.Add(tracker);
        }


        public void StartPersonManager()
        {
            // If there is a Kinect connected, get the Kinect

            if (KinectSensor.KinectSensors.Count > 0)
            {
                ks = KinectSensor.KinectSensors[0];
                ks.Start();



                // Sets the initial elevation angle of the connect to 0 degrees
                // This seemed to be causing the app to hang, in particular with the Kinect that was dropped
                //ks.ElevationAngle = 0;

                // Set smoothing parameters for when Kinect is tracking a skeleton
                TransformSmoothParameters parameters = new TransformSmoothParameters()
                {
                    Smoothing = 0.7f,
                    Correction = 0.3f,
                    Prediction = 0.4f,
                    JitterRadius = 1.0f,
                    MaxDeviationRadius = 0.5f,
                };

                ks.SkeletonStream.Enable(parameters);
                ks.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(ks_SkeletonFrameReady);
            }
        }

        public void StopPersonManager()
        {
            if (ks != null)
            {
                ks.Stop();
            }
        }

        #endregion

        #region Handling Skeleton Frames from Kinect

        //TODO Add documentation explaining how this works
        void ks_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //Checks if the stream is enabled for cases (though unlikely) when stream isn't enabled
            if (!((KinectSensor)sender).SkeletonStream.IsEnabled)
                return;



            //Process the skeleton frame
            else
            {
                UpdatePersonsAndDevices(GetTrackedSkeletonsAndPositions(e));
                gestureController.UpdateAllGestureGroups(GetAllSkeletonData(e));
            }


        }


        /// <summary>
        /// Single function to update location, and eventually orientation, of persons and devices in response to new Kinect frames.
        /// </summary>
        /// <param name="skeletons"></param>
        private void UpdatePersonsAndDevices(List<Skeleton> skeletons)
        {
            //Kinect occasionally returns null for a skeleton frame which leads to a null list<Skeleton>: skip this frame so that we don't drop any People
            if (skeletons == null)
            {
                return;
            }
            //Convert Locator List Types into PairablePerson & PairableDevice
            List<PairablePerson> pairablePersons = locator.Persons.OfType<PairablePerson>().ToList<PairablePerson>();
            List<PairableDevice> pairableDevices = locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>();

            // During shut down, the kinect will return some empty skeleton frames, and null skeleton lists
            if (skeletons == null)
                skeletons = new List<Skeleton>();

            AddNewPeople(skeletons, pairablePersons);
            RemoveOldPeople(skeletons, pairablePersons, pairableDevices);
            UpdatePeopleLocations(skeletons, pairablePersons, pairableDevices);

            //Sync up the Locator's Person collection
            locator.Persons = new List<Person>(pairablePersons);
        }

        private void UpdatePeopleLocations(List<Skeleton> skeletons, List<PairablePerson> pairablePersons, List<PairableDevice> pairableDevices)
        {
            // PairablePersons and Skeletons now contain only corresponding elements, except for people who are occluded, they don't have a matching skeleton
            // Update each Person
            foreach (PairablePerson person in pairablePersons)
            {
                // If the person is occluded, we don't have a skeleton to use for position updates right now
                if (person.PairingState == PairingState.PairedButOccluded)
                {
                    continue;
                }

                Skeleton skeleton = skeletons.Find(x => x.TrackingId.ToString().Equals(person.Identifier));

                // The Kinect looks down the Z axis in its coordinate space, left right movement happens on the X axis, and vertical movement on the Y axis
                // To translate this into the tracker's coordinate space, where it is at 0,0 and looks down the X axis, we pass in the Z and X components of 
                // the skeleton's position. See Tracker for more details.
                tracker.UpdatePositionForPerson(person, new Vector(skeleton.Position.Z, skeleton.Position.X));

                // TODO: Also update Person's orientation.

                // If the Person has a paired device, infer that the device is located where the person is, and update its location too
                if (person.PairingState == PairingState.Paired && person.HeldDeviceIdentifier != null)
                {
                    Device device = pairableDevices.Find(x => x.Identifier.Equals(person.HeldDeviceIdentifier));
                    device.Location = person.Location;
                }


            }
        }

        private void RemoveOldPeople(List<Skeleton> skeletons, List<PairablePerson> pairablePersons, List<PairableDevice> pairableDevices)
        {
            // For any Persons that have left the scene, remove their PairablePerson from , and if it was paired, unhook their paired device
            List<PairablePerson> vanishedPersons = new List<PairablePerson>();
            foreach (PairablePerson person in pairablePersons)
            {
                if (skeletons.Find(x => x.TrackingId.ToString().Equals(person.Identifier)) == null)
                {
                    //Remove Held-By-Person Identifier
                    PairableDevice device = pairableDevices.Find(x => x.Identifier.Equals(person.HeldDeviceIdentifier));

                    // If the person was not paired to a device, we can remove them immediately
                    if (device == null)
                    {
                        RemovePerson(vanishedPersons, person);

                    }
                    // If the person was paired, then we allow a grace period for them to reappear, to avoid immediately unpairing them
                    else
                    {


                        if (person.PairingState == PairingState.Paired)
                        {
                            person.PairingState = PairingState.PairedButOccluded;
                            person.Identifier = null;
                        }
                        // The person will remain with PairingState == PairedButOccluded for a few seconds, after which it will mark itself NotPaired
                        // If this happens, we remove the person for good, and unpair their device
                        else if (person.PairingState == PairingState.NotPaired)
                        {
                            device.HeldByPersonIdentifier = null;
                            device.PairingState = PairingState.NotPaired;

                            // Dispatch a message to the device
                            IARequest request = new IARequest(Routes.BecomeUnpairedRoute);
                            // Find the IntAirAct device matching the current device.
                            IADevice iaDevice = intAirAct.Devices.Find(d => d.Name == device.Identifier);
                            intAirAct.SendRequest(request, iaDevice);
                            System.Diagnostics.Debug.WriteLine(iaDevice.Name + " " + iaDevice.Host);

                            RemovePerson(vanishedPersons, person);
                        }

                    }

                }
            }
            foreach (PairablePerson person in vanishedPersons)
            {
                pairablePersons.Remove(person);
            }
        }

        private void RemovePerson(List<PairablePerson> vanishedPersons, PairablePerson person)
        {
            //Remove Held-Device Identifier
            person.HeldDeviceIdentifier = null;
            vanishedPersons.Add(person);
            if (PersonRemoved != null)
                PersonRemoved(this, person);
        }


        private void AddNewPeople(List<Skeleton> skeletons, List<PairablePerson> pairablePersons)
        {
            // First, test each new skeleton to see if it matches an occluded person
            foreach (Skeleton skeleton in skeletons)
            {
                //New Skeleton Found
                //if (pairablePersons.Find(x => x.Identifier.Equals(skeleton.TrackingId.ToString())) == null)
                if (pairablePersons.Find(x => skeleton.TrackingId.ToString().Equals(x.Identifier)) == null)
                {
                    foreach (PairablePerson pairablePerson in pairablePersons)
                    {
                        if (pairablePerson.PairingState == PairingState.PairedButOccluded)
                        {
                            Point skeletonInRoomSpace = Tracker.ConvertSkeletonToRoomSpace(new Vector(skeleton.Position.Z, skeleton.Position.X));

                            if (skeletonInRoomSpace.X < (pairablePerson.Location.Value.X + SAVINGDISTANCE) &&
                                skeletonInRoomSpace.X > (pairablePerson.Location.Value.X - SAVINGDISTANCE) &&
                                skeletonInRoomSpace.Y < (pairablePerson.Location.Value.Y + SAVINGDISTANCE) &&
                                skeletonInRoomSpace.Y > (pairablePerson.Location.Value.Y - SAVINGDISTANCE))
                            {
                                // "Repair them"
                                pairablePerson.Identifier = skeleton.TrackingId.ToString();
                                pairablePerson.PairingState = PairingState.Paired;

                                PairableDevice device = (PairableDevice)locator.Devices.Find(x => x.Identifier == pairablePerson.HeldDeviceIdentifier);
                                device.HeldByPersonIdentifier = pairablePerson.Identifier;
                            }
                        }
                    }
                }
            }

            // For any skeletons that weren't matched to an occluded person, we create a new PairablePerson
            foreach (Skeleton skeleton in skeletons)
            {
                //New Skeleton Found
                //if (pairablePersons.Find(x => x.Identifier.Equals(skeleton.TrackingId.ToString())) == null)
                if (pairablePersons.Find(x => skeleton.TrackingId.ToString().Equals(x.Identifier)) == null)
                {
                    PairablePerson person = new PairablePerson
                    {
                        Location = new Point(0, 0),
                        Orientation = 0.0,
                        Identifier = skeleton.TrackingId.ToString(),
                        PairingState = PairingState.NotPaired
                    };
                    pairablePersons.Add(person);

                    if (PersonAdded != null)
                        PersonAdded(this, person);
                }
            }
        }

        #endregion

        #region Skeleton Frame Helper

        //TODO: Lost person notification
        /// <summary>
        /// Copies tracked skeleton information from a frame and returns them as a list of skeletons
        /// Returns only skeletons in full tracking mode... 
        /// </summary>
        /// <param name="e">
        /// Incoming skeletonframe argument
        /// </param>
        /// <returns>Tracked skeletons as a list </returns>
        List<Skeleton> GetTrackedSkeletons(SkeletonFrameReadyEventArgs e)
        {
            //Allocate a maximum of 6 skeletons, as per the maximum allowed by the Kinect
            Skeleton[] allSkeletons = new Skeleton[6];

            using (SkeletonFrame frameData = e.OpenSkeletonFrame())
            {
                //Check that frame is not null
                if (frameData == null)
                {
                    return null;
                }

                //After checking the frame is not null, copy the skeleton data to the empty array of skeletons
                frameData.CopySkeletonDataTo(allSkeletons);

                //Capture only the Skeletons that are marked as Tracked
                List<Skeleton> updatedSkeletons = (from s in allSkeletons
                                                   where s.TrackingState == SkeletonTrackingState.Tracked
                                                   select s).ToList();

                return updatedSkeletons;
            }
        }

        

        /// <summary>
        /// Gets all skeletons seen by the Kinect, whether they are fully tracked (with skeleton data) or only positionally tracked (position only, no joint or bones).
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        List<Skeleton> GetTrackedSkeletonsAndPositions(SkeletonFrameReadyEventArgs e)
        {
            //Allocate a maximum of 6 skeletons, as per the maximum allowed by the Kinect
            Skeleton[] allSkeletons = new Skeleton[6];

            using (SkeletonFrame frameData = e.OpenSkeletonFrame())
            {
                //Check that frame is not null
                if (frameData == null)
                {
                    return null;
                }

                //After checking the frame is not null, copy the skeleton data to the empty array of skeletons
                frameData.CopySkeletonDataTo(allSkeletons);

                //Capture only the Skeletons that are marked as Tracked
                List<Skeleton> updatedSkeletons = (from s in allSkeletons
                                                   where s.TrackingState == SkeletonTrackingState.Tracked || s.TrackingState == SkeletonTrackingState.PositionOnly
                                                   select s).ToList();

                return updatedSkeletons;
            }
        }



        Skeleton[] GetAllSkeletonData(SkeletonFrameReadyEventArgs e)
        {
            //Allocate a maximum of 6 skeletons, as per the maximum allowed by the Kinect
            Skeleton[] allSkeletons = new Skeleton[6];

            using (SkeletonFrame frameData = e.OpenSkeletonFrame())
            {
                //Check that frame is not null
                if (frameData == null)
                {
                    return null;
                }

                //After checking the frame is not null, copy the skeleton data to the empty array of skeletons
                frameData.CopySkeletonDataTo(allSkeletons);

                return allSkeletons;
            }
        }




        #endregion

    }
}
