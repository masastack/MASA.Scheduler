// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules;

public class LogMonitorItemViewModelValidator : AbstractValidator<LogMonitorItemModel>
{
    public LogMonitorItemViewModelValidator(I18n i18n)
    {
        RuleFor(x => x.Field).Required();
        RuleFor(x => x.Alias).Required();
    }
}