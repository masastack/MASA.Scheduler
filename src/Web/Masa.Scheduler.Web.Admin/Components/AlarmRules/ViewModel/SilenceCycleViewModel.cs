// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules.ViewModel;

public class SilenceCycleViewModel
{
    public SilenceCycleType Type { get; set; }

    public TimeIntervalViewModel TimeInterval { get; set; } = new TimeIntervalViewModel();

    public int SilenceCycleValue { get; set; }
}
