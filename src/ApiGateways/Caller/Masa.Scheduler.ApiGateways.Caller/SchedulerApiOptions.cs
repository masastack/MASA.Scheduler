// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Scheduler.ApiGateways.Caller
{
    public class SchedulerApiOptions
    {
        public string SchedulerServerBaseAddress { get; set; }

        public SchedulerApiOptions(string schedulerServiceBaseAddress)
        {
            SchedulerServerBaseAddress = schedulerServiceBaseAddress;
        }
    }
}
