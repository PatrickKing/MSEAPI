using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;
using IntAirAct;
using ZeroConf;
using MSEGestureRecognizer;


namespace MSEKinect
{
    //TODO Refactor into seperate classes for device-detection and person-detection
    public class PersonManager
    {
        private static TraceSource logger = new TraceSource("MSEKinect");

        KinectSensor ks;
        GestureController gestureController;
        Room room;

        public PersonManager(Room room, GestureController gc)
        {
            this.gestureController = gc;
            this.room = room;
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
            ks.ElevationAngle = 0;

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



        //TODO Add documentation for this method
        //TODO Consider writing exceptions for certain values here 
        List<Person> ProcessPersonsOnFrame(List<Person> updatedPersons, List<Person> currentPersons, List<Device> currentConnectedDevices)
        {
            

            //Assertion that both parameters are valid 
            Debug.Assert(!(updatedPersons == null), "Invalid Parameter: Updated Persons Cannot Be Null");
            Debug.Assert(!(currentPersons == null), "Invalid Parameter: Current Persosn Cannot Be Null");  

            //Determine Persons Missing from the System
            var missing = from cp in currentPersons
                          where !updatedPersons.Contains(cp)
                          select cp;

            List<Person> missingPersons = missing.ToList<Person>();

            //TODO Reconsider this too 
            if (missingPersons.Count > 0)
            {
                ProcessMissingPersons(missingPersons, currentConnectedDevices);
            }

            //Determine Persons newly Added to the System
            var added = from up in updatedPersons
                        where !currentPersons.Contains(up)
                        select up;

            List<Person> addedPersons = added.ToList<Person>();

            //TODO Reconsider How This Event Is Called
            if (addedPersons.Count > 0)
            {
                ProcessAddedPersons(addedPersons);
            }

            //Update the List of Current Persons
            List<Person> processedPersons = new List<Person>();
            processedPersons.AddRange(currentPersons);
            processedPersons.AddRange(addedPersons);
            processedPersons.RemoveAll(person => missingPersons.Contains(person));

            return processedPersons; 
        }


        /// <summary>
        /// Removes the Held-Device relationship and the Held-By-Person relationship for devices which have been disconnected from the system. 
        /// </summary>
        /// <param name="missingPersons"> List of Persons disconnected from the system </param>
        /// <param name="currentConnectedDevices"> List of devices currently connected to the system </param>
        internal void ProcessMissingPersons(List<Person> missingPersons, List<Device> currentConnectedDevices)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, "Processing Missing Persons");

           //For each Person in the list, remove it's held-device referece and the circular reference held by the device
            foreach (Person p in missingPersons)
            {
                //Remove Held-By-Person Identifier
                Device d = currentConnectedDevices.Find(device => device.Identifier.Equals(p.HeldDeviceIdentifier));

                if (d != null)
                {
                    d.HeldByPersonIdentifier = null;
                    d.PairingState = PairingState.NotPaired; 
                }

                //Remove Held-Device Identifier
                p.HeldDeviceIdentifier = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addedPersons"></param>
        private void ProcessAddedPersons(List<Person> addedPersons)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, "Processing Added Persons"); 
        }

        //TODO Add documentation explaining how this works
        void ks_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //Checks if the stream is enabled for cases (though unlikely) when stream isn't enabled
            if (!((KinectSensor)sender).SkeletonStream.IsEnabled)
                return;

            //Process the skeleton frame
            else
            {
                //Capture the updates persons from the skeleton object
                List<Person> updatedPersons = GetPersons(GetSkeletons(e));

                //Process and handle the update to Persons
                room.CurrentPersons = ProcessPersonsOnFrame(updatedPersons, room.CurrentPersons, room.CurrentDevices);

                gestureController.UpdateAllGestures(GetSkeletons(e));
            }
        }

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


        /// <summary>
        /// Method that returns a list of people from the skeletons tracked by the Kinect
        /// </summary>
        /// <param name="updateSkeletons">A list of updated skeletons tracked by the kinect</param>
        /// <returns>List of Person objects, who are tracked by the Kinect</returns>
        List<Person> GetPersons(List<Skeleton> updateSkeletons)
        {
            List<Person> persons = new List<Person>();

            //Checks if the updatedSkeletons are null
            if (updateSkeletons == null)
            {
                return persons; 
            }

            else
            {
                //Iterate through each Skeleton to create a Person Object
                foreach (Skeleton skeleton in updateSkeletons)
                {
                    Person person = new Person
                    {
                        Location = new Location {X = skeleton.Position.X, Y = skeleton.Position.Y},
                        Orientation = null,
                        Identifier = skeleton.TrackingId.ToString()
                    }; 

                    persons.Add(person);
                }
                return persons;
            }
        }


        public void StopPersonManager()
        {
            ks.Stop();
        }
    }
}
