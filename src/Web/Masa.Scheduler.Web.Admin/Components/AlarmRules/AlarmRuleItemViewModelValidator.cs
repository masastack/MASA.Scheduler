// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules;

public class AlarmRuleItemViewModelValidator : AbstractValidator<AlarmRuleItemModel>
{
    public AlarmRuleItemViewModelValidator(I18n i18n)
    {
        RuleFor(x => x.Expression).Required();
        RuleFor(x => x.AlertSeverity).IsInEnum();
    }
}