// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules.Validator;

public class TimeIntervalViewModelValidator : AbstractValidator<TimeIntervalViewModel>
{
    public TimeIntervalViewModelValidator(I18n i18n)
    {
        RuleFor(x => x.IntervalTimeType).IsInEnum();
        RuleFor(x => x.IntervalTime).InclusiveBetween(1, 59)
            .When(x => x.IntervalTimeType == TimeType.Minute);
        RuleFor(x => x.IntervalTime).InclusiveBetween(1, 23)
            .When(x => x.IntervalTimeType == TimeType.Hour);
        RuleFor(x => x.IntervalTime).InclusiveBetween(1, 31)
            .When(x => x.IntervalTimeType == TimeType.Day);
    }
}
