using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace MSEGestureRecognizer
{
    public class Gesture
    {

        #region Instance Variables
        /// <summary>
        /// The segments that make up this gesture
        /// </summary>
        private IRelativeGestureSegment[] gestureSegments;

        /// <summary>
        /// The current gesture segment that we are matching against
        /// </summary>
        private int currentGestureSegment = 0;

        /// <summary>
        /// The number of frames for which we have been paused, waiting for the current segment to succeed.
        /// </summary>
        private int frameCount = 0;

        /// <summary>
        /// The type of gesture that this is
        /// </summary>
        private GestureType type;


        /// <summary>
        /// To avoid triggering wave gestures when users hold a device in front of them, we require that their hands be above their elbows,
        /// by the amount of this threshold. This makes it less likely to trigger accidental wave gestures.
        /// This may need to be bumped downward for people with short forearms =/
        /// </summary>
        public const double HAND_ELBOW_DISTANCE_THRESHOLD = 0.1; // meters
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="Gesture"/> class.
        /// </summary>
        /// <param name="type">The type of gesture.</param>
        /// <param name="gestureSegments">The gesture parts.</param>
        public Gesture(GestureType type, IRelativeGestureSegment[] gestureSegments)
        {
            this.gestureSegments = gestureSegments;
            this.type = type;
        }

        /// <summary>
        /// Occurs when [gesture recognised].
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        /// <summary>
        /// Uses skeleton data from sequential Kinect frames to detect the segments that make up a gesture.
        /// To be called on each SkeletonFrame from the Kinect.
        /// </summary>
        /// <param name="data">The skeleton data.</param>
        public void UpdateGesture(Skeleton data)
        {            
            if (data == null)
            {
                // We either lost the skeleton, or were never tracking it to begin with. 
                this.Reset();
                return;
            }

            GesturePartResult result = this.gestureSegments[this.currentGestureSegment].CheckGesture(data);

            // If the result is Pausing, the segment can still potentially succeed, 
            // but the joints it is tracking are not in the right position to satisfy it.
            if (result == GesturePartResult.Pausing)
            {
                this.frameCount++;
            }

            // The joints on the skeleton satisfy the gesture segment's criteria
            if (result == GesturePartResult.Succeed)
            {
                // If there are more gesture segments to test, we move on to the next one.
                if (this.currentGestureSegment + 1 < this.gestureSegments.Length)
                {
                    this.currentGestureSegment++;
                    this.frameCount = 0;
                }
                // If this was the last gesture segment in our list, we're done.
                // We fire off a gesture recognized event and reset to detect future gestures. 
                else
                {
                    if (this.GestureRecognized != null)
                    {
                        this.GestureRecognized(this, new GestureEventArgs(this.type, data.TrackingId));
                        this.Reset();
                    }
                }
            }


            // Failure is when the skeleton's joints move in a way that cancels the gesture, or if we have been waiting for too many frames for it to complete.

            // TODO: Using the number of frames as our time out condition could be problematic, because the FPS of the kinect is configurable.
            // It would be best to use some sort of timer instead. This was written with the Kinect configured for 60 FPS.
            if (result == GesturePartResult.Fail || this.frameCount >= 50)
            {
                // We go back to testing the first gesture segment. 
                Reset();
            }

        }



        /// <summary>
        /// Go back to testing the first gesture segment.
        /// </summary>
        public void Reset()
        {
            this.currentGestureSegment = 0;
            this.frameCount = 0;
        }
    }
}
