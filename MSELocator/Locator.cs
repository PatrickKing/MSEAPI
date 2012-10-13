using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSELocator
{
    public class Locator : MSELocatorInterface
    {
        private List<Person> _persons;
        public List<Person> Persons
        {
            get { return _persons; }
            set { _persons = value; }
        }

        private List<Device> _devices;
        public List<Device> Devices
        {
            get { return _devices; }
            set { _devices = value; }
        }

        public Locator()
        {
            Persons = new List<Person>();
            Devices = new List<Device>();
        }
        
    }
}
