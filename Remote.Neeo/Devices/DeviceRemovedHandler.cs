﻿using System.Threading.Tasks;

namespace Remote.Neeo.Devices
{
    /// <summary>
    /// Callback invoked by the Brain when a device is removed.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    public delegate Task DeviceRemovedHandler(string deviceId);
}