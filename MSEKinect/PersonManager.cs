using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;
using IntAirAct;
using ZeroConf;
using MSEGestureRecognizer;
using MSELocator;

using System.Windows;

namespace MSEKinect
{
    //TODO Refactor into seperate classes for device-detection and person-detection
    public class PersonManager
    {

        #region Instance Variables

        private static TraceSource logger = new TraceSource("MSEKinect");
        KinectSensor ks;
        GestureController gestureController;
        LocatorInterface locator;
        Tracker tracker;

        #endregion

        #region Constructor, Start and Stop

        public PersonManager(LocatorInterface locator, GestureController gc)
        {
            this.gestureController = gc;
            this.locator = locator;

            tracker = new Tracker() { Location = new Point(0, 0), Orientation = 0, Identifier = "MSEKinect" };
            locator.Trackers.Add(tracker);
        }


        //TODO Rename to StartPersonmM
        public void StartPersonManager()
        {
            // Checks to see how many Kinects are connected to the system. If None then exit.
            if (KinectSensor.KinectSensors.Count == 0)
            {
                logger.TraceEvent(TraceEventType.Error, 0, "There are no Kinects connected");
                return;
            }

            // If there is a Kinect connected, get the Kinect
            ks = KinectSensor.KinectSensors[0];
            ks.Start();
            //Sets the initial elevation angle of the connect to 0 degrees
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

        public void StopPersonManager()
        {
            ks.Stop();
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
                List<Skeleton> skeletons = GetSkeletons(e);
                UpdatePersonsAndDevices(skeletons);
                gestureController.UpdateAllGestures(skeletons);
            }


        }


        /// <summary>
        /// Single function to update location, and eventually orientation, of persons and devices in response to new Kinect frames.
        /// TODO: It might make sense to refactor some parts of this function into sub functions.
        /// </summary>
        /// <param name="skeletons"></param>
        private void UpdatePersonsAndDevices(List<Skeleton> skeletons)
        {
            //Convert Locator List Types into PairablePerson & PairableDevice
            List<PairablePerson> pairablePersons = locator.Persons.OfType<PairablePerson>().ToList<PairablePerson>();
            List<PairableDevice> pairableDevices = locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>();




            // For any skeletons that have just appeared, create a new PairablePerson
            foreach (Skeleton skeleton in skeletons)
            {
                if (pairablePersons.Find(x => x.Identifier.Equals(skeleton.TrackingId.ToString())) == null)
                {
                    PairablePerson person = new PairablePerson
                    {
                        Location = new Point(0, 0),
                        Orientation = 0.0,
                        Identifier = skeleton.TrackingId.ToString(),
                        PairingState = PairingState.NotPaired
                    };
                    pairablePersons.Add(person);
                }
            }

            // For any Persons that have left the scene, remove their PairablePerson from , and if it was paired, unhook their paired device
            List<PairablePerson> vanishedPersons = new List<PairablePerson>();
            foreach (PairablePerson person in pairablePersons)
            {
                if (skeletons.Find(x => x.TrackingId.ToString().Equals(person.Identifier)) == null)
                {
                    //Remove Held-By-Person Identifier
                    PairableDevice device = pairableDevices.Find(x => x.Identifier.Equals(person.HeldDeviceIdentifier));

                    if (device != null)
                    {
                        device.HeldByPersonIdentifier = null;
                        device.PairingState = PairingState.NotPaired;
                    }

                    //Remove Held-Device Identifier
                    person.HeldDeviceIdentifier = null;
                    vanishedPersons.Add(person);
                }
            }
            foreach (PairablePerson person in vanishedPersons)
            {
                pairablePersons.Remove(person);
            }

            // PairablePersons and Skeletons now contain only corresponding elements.
            // Update each Person
            foreach (Skeleton skeleton in skeletons)
            {
                PairablePerson person = pairablePersons.Find(x => x.Identifier.Equals(skeleton.TrackingId.ToString()));

                // The Kinect looks down the Z axis in its coordinate space, left right movement happens on the X axis, and vertical movement on the Y axis
                // To translate this into the tracker's coordinate space, where it is at 0,0 and looks down the X axis, we pass in the Z and X components of 
                // the skeleton's position. See Tracker for more details.
                tracker.UpdatePositionForPerson(person, new Vector(skeleton.Position.Z, skeleton.Position.X));

                // TODO: Also update Person's orientation.

                // If the Person has a paired device, infer that the device is located where the person is, and update its location too
                if (person.PairingState == PairingState.Paired && person.HeldDeviceIdentifier != null)
                {
//                    Device device = pairableDevices.Find(x => x.HeldByPersonIdentifier.Equals(person.Identifier));
                    Device device = pairableDevices.Find(x => x.Identifier.Equals(person.HeldDeviceIdentifier));
                    device.Location = person.Location;
                }

            }

                        //TODO Remove debug junk
            foreach (PairableDevice d in pairableDevices)
            {
                System.Diagnostics.Debug.WriteLine(d.Identifier + " " + d.PairingState.ToString());
            }
            foreach (PairablePerson p in pairablePersons)
            {
                System.Diagnostics.Debug.WriteLine(p.Identifier + " " + p.PairingState.ToString());
            }
            System.Diagnostics.Debug.WriteLine(" === ");


            //Sync up the Locator's Person collection
            locator.Persons = new List<Person>(pairablePersons);
        }

        #endregion

        #region Skeleton Frame Helper

        //TODO: Lost person notification
        //TODO:
        /// <summary>
        /// Copies tracked skeleton information from a frame and returns them as a list of skeletons
        /// </summary>
        /// <param name="e">
        /// Incoming skeletonframe argument
        /// </param>
        /// <returns>Tracked skeletons as a list </returns>
        List<Skeleton> GetSkeletons(SkeletonFrameReadyEventArgs e)
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

        #endregion




    }
}
