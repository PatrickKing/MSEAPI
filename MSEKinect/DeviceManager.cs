using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IntAirAct;
using System.Diagnostics;
using MSELocator;
using System.Windows;
using MSEAPI_SharedNetworking;

namespace MSEKinect
{
	public class DeviceManager
	{

		#region Instance Variables
		private static TraceSource logger = new TraceSource("MSEKinect");

		private IAIntAirAct ia;
		public IAIntAirAct IntAirAct
		{
			get { return ia; }

		}
		LocatorInterface locator;
		#endregion

		#region Events
		
		public delegate void DeviceChangedEventSignature(DeviceManager sender, PairableDevice device);
		public event DeviceChangedEventSignature DeviceAdded;
		public event DeviceChangedEventSignature DeviceRemoved;
		#endregion

		#region Constructor, Start and Stop

		public DeviceManager(LocatorInterface locator, IAIntAirAct intAirAct)
		{
			this.locator = locator;
			this.ia = intAirAct;
		}

		public void StartDeviceManager()
		{
			ia.DeviceFound += DeviceFound;
			ia.DeviceLost += DeviceLost;
			
		}

		public void StopDeviceManager()
		{
			ia.DeviceFound -= DeviceFound;
			ia.DeviceLost -= DeviceLost;
		}

		#endregion

		#region Device discovery and loss event handlers

		public void DeviceFound(IADevice iaDevice, bool ownDevice)
		{
			PairableDevice pairableDevice = new PairableDevice
			{
				Identifier = iaDevice.Name,
				PairingState = PairingState.NotPaired
			};

            FindDeviceWidthAndHeight(iaDevice); 


            if (iaDevice.SupportedRoutes.Contains(Routes.GetLocationRoute))
            {
                IARequest request = new IARequest(Routes.GetLocationRoute);
                IntAirAct.SendRequest(request, iaDevice, delegate(IAResponse response, Exception error)
                {
                    if (response == null || response.StatusCode == 404)
                    {
                        // Device has no location

                    }
                    else if (response.StatusCode == 200)
                    {
                        IntermediatePoint intermediatePoint = response.BodyAs<IntermediatePoint>();
                        Point result = intermediatePoint.ToPoint();

                        Device localDevice = locator.Devices.Find(d => d.Identifier.Equals(iaDevice.Name));

                        if (localDevice != null)
                        {
                            localDevice.Location = result;
                            response.StatusCode = 201; // created
                        }
                        else
                        {
                            response.StatusCode = 404; // not found
                        }

                    }
                });
            }

			locator.Devices.Add(pairableDevice);
			
			if (DeviceAdded != null)
				DeviceAdded(this, pairableDevice);
			
		}

        private void FindDeviceWidthAndHeight(IADevice iaDevice)
        {
            //Does the device contain and width and height route 
            if (iaDevice.SupportedRoutes.Contains(Routes.GetWidthAndHeightRoute))
            {
                IARequest request = new IARequest(Routes.GetWidthAndHeightRoute);
                IntAirAct.SendRequest(request, iaDevice, delegate(IAResponse response, Exception error)
                {
                    if (response == null || response.StatusCode == 404)
                    {
                        logger.TraceEvent(TraceEventType.Error, 100, "All devices should provide a width and height"); 
                    }
                    else if (response.StatusCode == 200)
                    {
                        Dictionary<String,double> dictionary = response.BodyAs<Dictionary<String,double>>();

                        Device localDevice = locator.Devices.Find(d => d.Identifier.Equals(iaDevice.Name));

                        //Device still exists, set width and height for device
                        if (localDevice != null)
                        {
                            localDevice.Height = dictionary["height"];
                            localDevice.Width = dictionary["width"];
                            
                        }
                        //Otherwise, device has disappeared, no action required
                        else
                        {
                            
                        }

                    }
                });

            }

        }

		public void DeviceLost(IADevice iaDevice)
		{
			List<PairablePerson> pairablePersons = locator.Persons.OfType<PairablePerson>().ToList<PairablePerson>();
			List<PairableDevice> pairableDevices = locator.Devices.OfType<PairableDevice>().ToList<PairableDevice>();

			// Find and remove the pairable device from the Locator's list of devices
			PairableDevice pairableDevice = pairableDevices.Find(d => d.Identifier.Equals(iaDevice.Name));
			locator.Devices.Remove(pairableDevice);


			// If the device was paired, we mark its corresponding person unpaired.
			PairablePerson person = pairablePersons.Find(p => p.Identifier.Equals(pairableDevice.HeldByPersonIdentifier));
			if (person != null)
			{
				//Remove the pairing information associated with the Perpon
				person.HeldDeviceIdentifier = null;
				person.PairingState = PairingState.NotPaired;
			}
			pairableDevice.HeldByPersonIdentifier = null;

			if (pairableDevice == null)
			{
				System.Diagnostics.Debug.WriteLine("ERROR: tried to remove nonexistent pairable device.");
			}

			if (DeviceRemoved != null)
				DeviceRemoved(this, pairableDevice);
		}

		#endregion








	}
}
