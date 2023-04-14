// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules.Validator;

public class AlarmRuleUpsertViewModelValidator : AbstractValidator<AlarmRuleUpsertViewModel>
{
    public AlarmRuleUpsertViewModelValidator(I18n i18n)
    {
        RuleFor(x => x.ProjectIdentity).Required()
            .When(x => x.Type == AlarmRuleType.Log).When(x => x.Step == 1);
        RuleFor(x => x.AppIdentity).Required()
            .When(x => x.Type == AlarmRuleType.Log).When(x => x.Step == 1);

        RuleFor(x => x.CheckFrequency).SetValidator(new CheckFrequencyViewModelValidator(i18n))
            .When(x => x.Type == AlarmRuleType.Log && x.Step == 2 || x.Type == AlarmRuleType.Metric && x.Step == 1);
        RuleFor(x => x.LogMonitorItems).Required()
            .ForEach(x => x.SetValidator(new LogMonitorItemViewModelValidator(i18n)))
            .When(x => x.Type == AlarmRuleType.Log && x.Step == 2);
        RuleFor(x => x.DisplayName).Required()
            .Matches(RegularHelper.CHINESE_LETTER_NUMBER_SYMBOL).WithMessage(i18n.T("AlarmRuleBlock.AlarmRuleNameFormat.Verification"))
            .Length(2, 50)
            .When(x => x.Type == AlarmRuleType.Log && x.Step == 3 || x.Type == AlarmRuleType.Metric && x.Step == 2);
        RuleFor(x => x.Items).Required()
            .ForEach(x => x.SetValidator(new AlarmRuleItemViewModelValidator(i18n)))
            .When(x => x.Type == AlarmRuleType.Log && x.Step == 3 || x.Type == AlarmRuleType.Metric && x.Step == 2);
        RuleFor(x => x.SilenceCycle).SetValidator(new SilenceCycleViewModelValidator(i18n)).When(x => x.Type == AlarmRuleType.Log && x.Step == 3 || x.Type == AlarmRuleType.Metric && x.Step == 2);
    }
}