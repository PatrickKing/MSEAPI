using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using IntAirAct;



namespace MSEAPI_SharedNetworking
{
    /// <summary>
    /// CommunicationManager handles all IntAirAct requests for MSEKinect
    /// </summary>
    /// 
    public class IntermediateDevice
    {
        public String identifier;
        public double? orientation;
        public Point? location;

        public bool isComplete
        {
            get
            {
                if (identifier != null &&
                    orientation.HasValue &&
                    location.HasValue)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }  



    }

}
