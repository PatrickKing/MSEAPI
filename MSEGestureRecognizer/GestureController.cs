using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace MSEGestureRecognizer
{

    public class GestureController
    {

        /// <summary>
        /// The list of all gestures we are currently looking for
        /// </summary>
        //private List<Gesture> gestures = new List<Gesture>();

        /// <summary>
        /// A list of gesture groups, one group per skeleton
        /// </summary>
        private List<GestureGroup> gestureGroups;


        /// <summary>
        /// Initializes a new instance of the <see cref="GestureController"/> class.
        /// </summary>
        public GestureController()
        {
            gestureGroups = new List<GestureGroup>();
            for (int i = 0; i < 6; i++)
            {
                // For each of the six possible skeletons, we create a gesture group, and add the gestures we will check for.
                GestureGroup gestureGroup = new GestureGroup();

                // Define the gestures
                // Each gesture monitors one hand, and each can trigger a GestureRecognized event independently. 
                IRelativeGestureSegment[] waveRightSegments = new IRelativeGestureSegment[4];
                WaveRightSegment1 waveRightSegment1 = new WaveRightSegment1();
                WaveRightSegment2 waveRightSegment2 = new WaveRightSegment2();
                waveRightSegments[0] = waveRightSegment1;
                waveRightSegments[1] = waveRightSegment2;
                waveRightSegments[2] = waveRightSegment1;
                waveRightSegments[3] = waveRightSegment2;
                Gesture gesture = new Gesture(GestureType.WaveRight, waveRightSegments);
                gesture.GestureRecognized += OnGestureRecognized;
                gestureGroup.gestures.Add(gesture);


                IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[4];
                WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
                WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
                waveLeftSegments[0] = waveLeftSegment1;
                waveLeftSegments[1] = waveLeftSegment2;
                waveLeftSegments[2] = waveLeftSegment1;
                waveLeftSegments[3] = waveLeftSegment2;
                gesture = new Gesture(GestureType.WaveLeft, waveLeftSegments);
                gesture.GestureRecognized += OnGestureRecognized;
                gestureGroup.gestures.Add(gesture);

                gestureGroups.Add(gestureGroup);
            }


        }

        /// <summary>
        /// Occurs when [gesture recognised].
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        /// <summary>
        /// Updates all gestures.
        /// </summary>
        /// <param name="data">The skeleton data.</param>
        public void UpdateAllGestureGroups(Skeleton[] data)
        {
            if (data == null)
                return;

            // Update each gesture, in each gesture group
            for (int i = 0; i < data.Length; i++ )
            {
                foreach (Gesture gesture in gestureGroups.ElementAt(i).gestures)
                {
                    gesture.UpdateGesture(data[i]);
                }
            }
        }



        /// <summary>
        /// Handles the GestureRecognized event of the g control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KinectSkeltonTracker.GestureEventArgs"/> instance containing the event data.</param>
        //private void Gesture_GestureRecognized(object sender, GestureEventArgs e)
        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            if (this.GestureRecognized != null)
            {
                this.GestureRecognized(this, e);
            }

        }
    }
}
