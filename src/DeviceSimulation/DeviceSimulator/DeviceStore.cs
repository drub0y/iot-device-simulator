using DeviceSimulator.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceSimulator
{
    public class DeviceStore : IDeviceStore
    {
        private readonly List<IDeviceService> _deviceServices;
        public DeviceStore()
        {
            _deviceServices = new List<IDeviceService>();
        }
        public ICollection<IDeviceService> Devices()
        {
            return _deviceServices;
        }
    }
}
