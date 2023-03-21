// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules;

public class CheckFrequencyViewModelValidator : AbstractValidator<CheckFrequencyModel>
{
    public CheckFrequencyViewModelValidator(I18n i18n)
    {
        RuleFor(x => x.Type).Required();
        RuleFor(x => x.CronExpression).Required()
            .Must(x => CronExpression.IsValidExpression(x ?? string.Empty))
            .When(x => x.Type == AlarmCheckFrequencyType.Cron);
        RuleFor(x => x.FixedInterval).SetValidator(new TimeIntervalViewModelValidator(i18n))
            .When(x => x.Type == AlarmCheckFrequencyType.FixedInterval);
    }
}