// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Tsc;

public partial class ErrorDetail
{
    [Parameter]
    public bool Show { get; set; }

    ChartData errorChart = new();
    LogResponseDto? currentLog = null;
    TraceResponseDto? currentTrace = null;
    IJSObjectReference? module = null;

    bool showExceptError = false;

    [Inject]
    IJSRuntime JSRuntime { get; set; } = default!;

    int currentPage = 1;
    int total = 1;

    StringNumber index = 1;

    [Parameter]
    public SearchData SearchData { get; set; } = default!;

    string search = string.Empty;
    IDictionary<string, object>? _dic = null;
    bool loading = true;
    string? lastKey = default, lastType = default, lastMessage = default;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/_content/Masa.Tsc.Web.Admin.Rcl/Pages/Apm/ErrorDetail.razor.js");
        }
        else if (module != null)
        {
            await module.InvokeVoidAsync("autoHeight");
        }
    }

    private async Task OnLoadAsync(SearchData? data = null)
    {
        if (data != null)
        {
            currentPage = 1;
            total = 0;
            SearchData = data;
        }
        if (!SearchData.Loaded)
            return;
        loading = true;
        await ChangeRecordAsync();
        loading = false;
        await LoadChartDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (!Show)
            return;
        var key = Encrypt(JsonSerializer.Serialize(SearchData));
        if (lastKey != key || lastType != SearchData.ExceptionType || lastMessage != SearchData.ExceptionMsg)
        {
            lastKey = key;
            lastType = SearchData.ExceptionType;
            lastMessage = SearchData.ExceptionMsg;
            currentPage = 1;
            currentLog = default!;
            _dic?.Clear();
            total = 1;
            await OnLoadAsync();
        }
    }

    private async Task LoadLogAysnc()
    {
        currentLog = default!;
        _dic = new Dictionary<string, object>();
        var query = new BaseRequestDto
        {
            Service = SearchData.Service!,
            PageSize = 1,
            Page = currentPage,
            Start = SearchData.Start,
            End = SearchData.End,
            TraceId = SearchData.TraceId!,
        };
        var list = new List<FieldConditionDto>();
        if (!string.IsNullOrEmpty(SearchData.Environment))
        {
            list.Add(new FieldConditionDto { Name = StorageConst.Current.Environment, Value = SearchData.Environment, Type = ConditionTypes.Equal });
        }
        if (!string.IsNullOrEmpty(SearchData.ExceptionType))
        {
            list.Add(new FieldConditionDto { Name = StorageConst.Current.ExceptionType, Value = SearchData.ExceptionType, Type = ConditionTypes.Equal });
        }
        if (!string.IsNullOrEmpty(SearchData.ExceptionMsg))
        {
            list.Add(new FieldConditionDto { Name = StorageConst.Current.ExceptionMessage, Value = SearchData.ExceptionMsg, Type = ConditionTypes.Regex });
        }
        if (!string.IsNullOrEmpty(SearchData.TextField) && !string.IsNullOrEmpty(SearchData.TextValue))
        {
            if (SearchData.TextField == StorageConst.Current.ExceptionMessage || SearchData.TextField == StorageConst.Current.Log.Body)
            {
                list.Add(new FieldConditionDto { Name = SearchData.TextField, Value = SearchData.TextValue, Type = ConditionTypes.Regex });
            }
            else if (SearchData.TextField == StorageConst.Current.TraceId || SearchData.TextField == StorageConst.Current.SpanId)
            {
                list.Add(new FieldConditionDto { Name = SearchData.TextField, Value = SearchData.TextValue, Type = ConditionTypes.Equal });
            }
        }
        if (SearchData.EnableExceptError)
            list.Add(new FieldConditionDto { Name = nameof(ApmErrorRequestDto.Filter), Value = true, Type = ConditionTypes.Equal });
        query.Conditions = list;
        int count = 1;
        do
        {
            if (count == 0)
            {
                var message = list.Find(item => item.Name == StorageConst.Current.ExceptionMessage)!;
                message.Name = StorageConst.Current.Log.Body;
            }
            var result = await TscClient.ApmService.GetLogListAsync(CurrentTeamId, query, SearchData.Project, SearchData.ServiceType, ignoreTeam: !string.IsNullOrEmpty(SearchData.TraceId) || string.IsNullOrEmpty(SearchData.Service));
            if (result != null)
            {
                if (currentPage == 1)
                {
                    total = (int)result.Total;
                }

                if (result.Result != null && result.Result.Any())
                {
                    currentLog = result.Result[0];
                    _dic = currentLog.ToDictionary();
                    break;
                }

            }

        } while (count-- > 0);
    }

    private async Task LoadTraceAsync()
    {
        currentTrace = default!;
        if (currentLog == null || string.IsNullOrEmpty(currentLog.SpanId) || !currentLog.Attributes.ContainsKey("RequestPath"))
            return;
        //var result = await ApiCaller.TraceService.GetListAsync(new RequestTraceListDto
        //{
        //    SpanId = currentLog.SpanId,
        //    Page = 1,
        //    PageSize = 1,
        //    Start = Search.Start,
        //    End = Search.End,
        //    Service = Search.Service!,
        //    HasPage = false
        //});
        //if (result == null || result.Result == null || !result.Result.Any())
        //    return;
        //currentTrace = result.Result[0];
    }

    private async Task ChangePageAsync(int page)
    {
        loading = true;
        currentPage = page;
        await ChangeRecordAsync();
        loading = false;
    }

    private async Task ChangeRecordAsync()
    {
        await LoadLogAysnc();
        await LoadTraceAsync();
    }

    private async Task LoadChartDataAsync()
    {
        var query = new ApmEndpointRequestDto
        {
            Start = SearchData.Start,
            End = SearchData.End,
            //Queries = GetText,
            Service = SearchData.Service,
            Endpoint = SearchData.Endpoint!,
            Env = SearchData.Environment,
            ExType = SearchData.ExceptionType,
            ExMessage = SearchData.ExceptionMsg
        };
        var result = await TscClient.ApmService.GetErrorChartAsync(query);
        errorChart.Data = ConvertLatencyChartData(result, lineName: "error count").Json;
        errorChart.ChartLoading = false;
    }

    private EChartType ConvertLatencyChartData(List<ChartLineCountDto> data, string? lineColor = null, string? areaLineColor = null, string? unit = null, string? lineName = null)
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
        //if (data != null && data.Any())
        {
            chart.SetValue("xAxis", new object[] {
                new { type="category",boundaryGap=false,data=data?.Select(item=>item.Currents.First().Time.ToDateTime(CurrentTimeZone).Format()) }
            });
            chart.SetValue($"series[0]", new { name = $"{lineName}", type = "line", smooth = true, areaStyle = new { }, lineStyle = new { width = 1 }, symbol = "none", data = data?.Select(item => item.Currents.First().Value) });
        }

        return chart;
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await base.DisposeAsyncCore();
        if (module != null)
            await module.DisposeAsync();
    }
}
