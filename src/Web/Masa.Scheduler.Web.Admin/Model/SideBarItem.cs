// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Model;

public class SideBarItem
{
    public StringNumber Identity { get; set; } = default!;

    public string Title { get; set; } = string.Empty;

    public bool IsProject { get; set; }

    public bool Expanded { get; set; }

    public List<SideBarItem> Children { get; set; } = new();
}

