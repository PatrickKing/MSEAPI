﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace MSEGestureRecognizer
{
    public class WaveLeftSegment1 : IRelativeGestureSegment
    {
        //private static TraceSource logger = new TraceSource("MSEGestureRecognizer");

        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            //logger.TraceEvent(TraceEventType.Verbose, 0, skeleton.Joints[JointType.HandLeft].Position.Y);

            // hand above elbow
            if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y + Gesture.HAND_ELBOW_DISTANCE_THRESHOLD)
            {
                // hand right of elbow
                if (skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ElbowLeft].Position.X)
                {
          //          logger.TraceEvent(TraceEventType.Verbose, 0, "Recognized WaveLeft 1");
                    return GesturePartResult.Succeed;
                }

                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                return GesturePartResult.Pausing;
            }

            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }

    public class WaveLeftSegment2 : IRelativeGestureSegment
    {
        //private static TraceSource logger = new TraceSource("MSEGestureRecognizer");

        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {

            // hand above elbow
            if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y + Gesture.HAND_ELBOW_DISTANCE_THRESHOLD)
            {
                // hand right of elbow
                if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ElbowLeft].Position.X)
                {
          //          logger.TraceEvent(TraceEventType.Verbose, 0, "Recognized WaveLeft 2");
                    return GesturePartResult.Succeed;
                }

                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                return GesturePartResult.Pausing;
            }

            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }

}
