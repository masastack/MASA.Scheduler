// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Tsc.Trace;

public partial class ServiceErrors
{
    [CascadingParameter]
    public SearchData SearchData { get; set; } = default!;

    //跟相关的接口关联查询使用
    [Parameter]
    public string? SpanId { get; set; }

    [Parameter]
    public bool ShowChart { get; set; } = true;

    [Parameter]
    public MetricTypes MetricType { get; set; }

    private List<DataTableHeader<ErrorMessageDto>> headers => new()
    {
        new() { Text = I18n.T("Tsc.Error.List.Type"), Value = nameof(ErrorMessageDto.Type)},
        new() { Text = I18n.T("Tsc.Error.List.Message"), Value = nameof(ErrorMessageDto.Message) },
        new() { Text = I18n.T("Tsc.Error.List.LastTime"), Value = nameof(ErrorMessageDto.LastTime) },
        new() { Text = I18n.T("Tsc.Error.List.Total"), Value = nameof(ErrorMessageDto.Total)}
    };

    private int defaultSize = 20;
    private int total = 0;
    private int page = 1;
    private List<ErrorMessageDto> data = new();
    private bool isTableLoading = false;
    private string sortFiled = nameof(ErrorMessageDto.Total);
    private bool sortBy = true;
    private string lastKey = string.Empty, lastSpanId = string.Empty;
    private ChartData chart = new();

    private async Task OnTableOptionsChanged(DataOptions sort)
    {
        if (sort.SortBy.Any())
            sortFiled = sort.SortBy[0];
        else
            sortFiled = default!;
        if (sort.SortDesc.Any())
            sortBy = sort.SortDesc[0];
        else
            sortBy = default;
        await LoadASync();
        StateHasChanged();
    }

    protected override async Task OnParametersSetAsync()
    {
        var key = Encrypt(JsonSerializer.Serialize(SearchData));
        if (lastKey != key || SpanId != null && SpanId != lastSpanId || SpanId == null && lastSpanId.Length > 0)
        {
            lastKey = key;
            lastSpanId = SpanId ?? string.Empty;
            await LoadASync();
            StateHasChanged();
            await LoadChartDataAsync();
        }
        await base.OnParametersSetAsync();
    }

    private StringNumber GetHeight()
    {
        return ShowChart ? "calc(100vh - 620px)" : "calc(100vh - 420px)";
    }

    private async Task OnPageChange((int page, int pageSize) pageData)
    {
        page = pageData.page;
        defaultSize = pageData.pageSize;
        await LoadPageDataAsync();
    }

    private async Task LoadASync()
    {
        if (isTableLoading)
        {
            return;
        }
        await LoadPageDataAsync();
    }

    private async Task LoadChartDataAsync()
    {
        if (!ShowChart)
            return;
        List<ChartLineCountDto> result = default!;
        if (!string.IsNullOrEmpty(SearchData.Service))
        {
            var query = new ApmEndpointRequestDto
            {
                Page = page,
                PageSize = defaultSize,
                Start = SearchData.Start,
                End = SearchData.End,
                OrderField = sortFiled,
                Service = SearchData.Service,
                Endpoint = SearchData.Endpoint!,
                Env = SearchData.Environment,
                IsDesc = sortBy
            };
            result = await TscClient.ApmService.GetErrorChartAsync(query);
        }
        chart.Data = ConvertLatencyChartData(result, lineName: I18n.Apm("Chart.ErrorCount")).Json;
        chart.ChartLoading = false;
    }

    private async Task LoadPageDataAsync()
    {
        if (isTableLoading) return;
        isTableLoading = true;
        if (string.IsNullOrEmpty(SearchData.Service) && string.IsNullOrEmpty(SearchData.TraceId))
        {
            total = 0;
            data = new();
        }
        else
        {
            var query = new ApmErrorRequestDto
            {
                Page = page,
                PageSize = defaultSize,
                Start = SearchData.Start,
                End = SearchData.End,
                OrderField = sortFiled,
                TraceId = SearchData.TraceId!,
                Service = SearchData.Service,
                Env = SearchData.Environment,
                IsDesc = sortBy,
                TextField = StorageConst.Current.SpanId!,
                TextValue = lastSpanId,
                Filter = SearchData.EnableExceptError
            };
            var result = await TscClient.ApmService.GetErrorsPageAsync(CurrentTeamId, query, SearchData.Project, SearchData.ServiceType, string.IsNullOrEmpty(SearchData.Service));
            data = result?.Result?.ToList() ?? new();
            total = (int)(result?.Total ?? 0);
        }
        isTableLoading = false;
    }

    private EChartType ConvertLatencyChartData(List<ChartLineCountDto> data, string? lineColor = default, string? areaLineColor = default, string? unit = default, string? lineName = null)
    {
        var chart = EChartConst.Line;
        chart.SetValue("tooltip", new { trigger = "axis" });
        if (!string.IsNullOrEmpty(lineName))
        {
            chart.SetValue("legend", new { data = new string[] { $"{lineName}" }, bottom = "2%" });
        }

        chart.SetValue("yAxis", new object[] {
            new {type="value",axisLabel=new{formatter=$"{{value}} {unit}" } }
        });
        chart.SetValue("grid", new { top = "10%", left = "2%", right = "5%", bottom = "15%", containLabel = true });
        if (data != null && data.Any())
        {
            chart.SetValue("xAxis", new object[] {
                new { type="category",boundaryGap=false,data=data?.Select(item=>item.Currents.First().Time.ToDateTime(CurrentTimeZone).Format()) }
            });
            chart.SetValue($"series[0]", new { name = $"{lineName}", type = "line", smooth = true, areaStyle = new { }, lineStyle = new { width = 1 }, symbol = "none", data = data?.Select(item => item.Currents.First().Value) });
        }

        return chart;
    }

    bool showDetail = false;

    private void Show(string? type = default, string? message = default)
    {
        SearchData.ExceptionType = type!;
        SearchData.ExceptionMsg = message!;
        //if (!string.IsNullOrEmpty(SearchData.TraceId))
        //    SearchData.TraceId = SearchData.TraceId;
        //else
        //    SearchData.TraceId = string.Empty;
        showDetail = true;
        StateHasChanged();
    }
}
