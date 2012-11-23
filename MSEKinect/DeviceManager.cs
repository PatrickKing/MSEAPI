using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IntAirAct;
using System.Diagnostics;
using MSELocator;


namespace MSEKinect
{
	public class DeviceManager
	{

		#region Instance Variables
		private int dictionaryResets;
		private static TraceSource logger = new TraceSource("MSEKinect");

		IAIntAirAct ia;
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
			dictionaryResets = 0;
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
			locator.Devices.Add(pairableDevice);
			
			if (DeviceAdded != null)
				DeviceAdded(this, pairableDevice);
			
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
