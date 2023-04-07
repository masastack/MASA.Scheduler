// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules.ViewModel;

public class CheckFrequencyViewModel
{
    public AlarmCheckFrequencyType Type { get; set; }

    public TimeIntervalViewModel FixedInterval { get; set; } = new TimeIntervalViewModel();


    public string CronExpression { get; set; } = string.Empty;
}
