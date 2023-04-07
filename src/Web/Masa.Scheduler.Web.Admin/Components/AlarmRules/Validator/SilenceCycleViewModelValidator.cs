// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules.Validator;

public class SilenceCycleViewModelValidator : AbstractValidator<SilenceCycleViewModel>
{
    public SilenceCycleViewModelValidator(I18n i18n)
    {
        RuleFor(x => x.TimeInterval).SetValidator(new TimeIntervalViewModelValidator(i18n)).When(x => x.Type == SilenceCycleType.Time);
    }
}
