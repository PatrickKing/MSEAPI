using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    }
}
