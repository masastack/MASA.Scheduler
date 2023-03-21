// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules;

public class AlarmRuleUpsertViewModel
{
    public AlarmRuleType Type { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string ProjectIdentity { get; set; } = string.Empty;

    public string AppIdentity { get; set; } = string.Empty;

    public int Step { get; set; } = 1;

    public string ChartYAxisUnit { get; set; } = string.Empty;

    public CheckFrequencyModel CheckFrequency { get; set; } = new();

    public string WhereExpression { get; set; } = string.Empty;

    public string QueryStr { get; set; } = "Query";

    public int ContinuousTriggerThreshold { get; set; }

    public SilenceCycleModel SilenceCycle { get; set; } = new();

    public bool IsEnabled { get; set; }

    public List<LogMonitorItemModel> LogMonitorItems { get; set; } = new();

    public List<MetricMonitorItemModel> MetricMonitorItems { get; set; } = new();

    public List<AlarmRuleItemModel> Items { get; set; } = new();
}