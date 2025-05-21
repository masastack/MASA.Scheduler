// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Model;

public class SearchData
{
    public SearchData()
    {
        ServiceType = AppTypes.Service.ToString();
    }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string? Environment { get; set; }

    public string? ServiceType { get; set; }

    public string? Project { get; set; }

    public string? Service { get; set; }

    public string? Endpoint { get; set; }

    public ApmComparisonTypes ComparisonType { get; set; }

    public string? TextField { get; set; }

    public string? TextValue { get; set; }

    public string? Status { get; set; }

    public string? Method { get; set; }

    public string? ExceptionType { get; set; }

    public string? ExceptionMsg { get; set; }

    public string? TraceId { get; set; }

    public string? SpanId { get; set; }

    public bool EnableExceptError { get; set; } = true;

    /// <summary>
    /// search com
    /// </summary>
    public bool Loaded { get; set; }
}

public enum ApmComparisonTypes
{
    None,

    Day = 1,

    Week = 2
}

public enum MetricTypes
{
    Avg,

    P95,

    P99
}

public class ChartData
{

    public bool HasChart { get; set; } = true;

    public bool ChartLoading { get; set; } = true;

    public bool EmptyChart { get; set; }

    public object Data { get; set; } = default!;
}

public class LatencyTypeChartData
{
    public MetricTypes MetricType { get; set; }

    public ChartData Avg { get; set; } = new();

    public ChartData P95 { get; set; } = new();

    public ChartData P99 { get; set; } = new();

    public ChartData ChartData
    {
        get
        {
            return MetricType switch
            {
                MetricTypes.P95 => P95,
                MetricTypes.P99 => P99,
                _ => Avg,
            };
        }
    }
}

public static class ApmComparisonTypeExtensions
{
    public static ComparisonTypes? ToComparisonType(this ApmComparisonTypes value)
    {
        if ((int)value - ComparisonTypes.DayBefore == 0 || (int)value - ComparisonTypes.WeekBefore == 0)
            return (ComparisonTypes)((int)value);
        return default;
    }
}