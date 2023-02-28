// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler;

public static class DnsHelper
{
    public static string GetHostName(ILogger? logger=null)
    {
        var hostName = Dns.GetHostName();
        try
        {
            var host = Dns.GetHostEntryAsync(hostName).GetAwaiter().GetResult();
            hostName = host.HostName;
        }
        catch (Exception e)
        {
            logger?.LogError(e, e.Message);
        }

        return hostName;
    }
}