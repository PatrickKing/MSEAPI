using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MSELocator
{
    public interface LocatorInterface
    {
        List<Person> Persons
        {
            get;
            set;
        }

        List<Device> Devices
        {
            get;
            set;
        }

        List<Tracker> Trackers
        {
            get;
            set;
        }



        #region Query Methods

        List<Device> GetDevicesInView(String identifier);

        List<Device> GetDevicesInView(Device observer);

        Device GetNearestDeviceInView(String identifier);

        Device GetNearestDeviceInView(Device observer);

        List<Device> GetDevicesWithinRange(String identifier, double distance);

        List<Device> GetDevicesWithinRange(Device observer, double distance);

        Device GetNearestDeviceWithinRange(String identifier, double distance);

        Device GetNearestDeviceWithinRange(Device observer, double distance);

        Dictionary<Device, Point> GetDevicesInViewWithIntersectionPoints(Device observer);

        #endregion


    }
}
