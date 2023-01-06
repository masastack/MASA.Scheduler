// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.SchedulerResources;

public class AppResourceViewModel
{
    public string Name { get; set; } = string.Empty;

    public string Identity { get; set; } = string.Empty;

    public List<SchedulerResourceDto> Resources { get; set; } = new();
}
