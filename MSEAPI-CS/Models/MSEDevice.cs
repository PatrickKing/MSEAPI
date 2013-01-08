using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using IntAirAct;
using MSEAPI_SharedNetworking;

namespace MSEAPI_CS.Models
{
    public class MSEDevice
    {
        #region Properties

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

                #endregion

        #region Constructors and Converters

        public MSEDevice()
        {
        }

        public MSEDevice(IntermediateDevice intermediateDevice)
        {
            this.Identifier = intermediateDevice.identifier;
            this.Orientation = intermediateDevice.orientation;
            this.Location = intermediateDevice.location;
        }

        public static List<MSEDevice> MseDevicesFromIntermediateDevices(List<IntermediateDevice> intermediateDevices)
        {
            List<MSEDevice> devices = new List<MSEDevice>();
            foreach (IntermediateDevice intermediateDevice in intermediateDevices)
            {
                devices.Add(new MSEDevice(intermediateDevice));
            }
            return devices;
        }


        #endregion

        public bool setupNetworkDevice(IAIntAirAct ia)
        {
            IADevice potentialNetworkDevice = ia.DeviceWithName(this.Identifier);

            if (potentialNetworkDevice == null)
                return false;

            this.NetworkDevice = potentialNetworkDevice;
            return true;
        }
    }
}
