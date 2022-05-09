// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Team
{
    public partial class Project
    {
        private StringNumber _curTab = 0;
        private bool _teamDetailDisabled;

        [Parameter]
        public string TeamId { get; set; } = string.Empty;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return base.OnAfterRenderAsync(firstRender);
        }
    }
}
