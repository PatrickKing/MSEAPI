using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using IntAirAct;

namespace MSEAPI_CS.Models
{
    public class MSEDevice
    {

        public string Identifier
        {
            get;
            set;
        }

        public double? Orientation
        {
            get;
            set;
        }

        public Point? Location
        {
            get;
            set;
        }

        public DateTime LastUpdated
        {
            get;
            set;
        }

        public IADevice NetworkDevice
        {
            get;
            set;
        }




    }
}
