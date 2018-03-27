using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceSimulator.Interfaces
{
    interface IDeviceStore
    {
        ICollection<IDeviceService> Devices();
    }
}
