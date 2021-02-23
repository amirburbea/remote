﻿using System.Threading.Tasks;

namespace Remote.Neeo.Devices.Discovery
{
    /// <summary>
    /// Attempt to register a device adapter given a set of <paramref name="credentials" />.
    /// </summary>
    /// <param name="credentials"></param>
    /// <returns><see cref="Task"/> to indicate completion.</returns>
    public delegate Task CredentialsProcessor(Credentials credentials);
}