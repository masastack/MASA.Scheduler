// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Tsc.Trace;

public partial class TimeLine
{
    [Inject]
    IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public List<TraceResponseDto> Data { get; set; } = default!;

    [Parameter]
    public List<ChartPointDto> Errors { get; set; } = default!;

    [Parameter]
    public double Percentile { get; set; }

    [Parameter]
    public int Page { get; set; } = 1;

    [Parameter]
    public int Total { get; set; } = 1;

    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    [Parameter]
    public EventCallback<string> OnSpanIdChanged { get; set; }

    [Parameter]
    public bool IsMaui { get; set; }

    [Parameter]
    public string? Service { get; set; }

    [Parameter]
    public string RoutePath { get; set; } = default!;

    private string? lastKey = default;
    private bool loading = true;
    private int totalDuration = 0;
    private int stepDuration = 0;
    private int lastDuration;
    private List<TreeLineDto> timeLines = new();
    DateTime start, end;
    bool showTimeLine = true;
    int[] errorStatus = Array.Empty<int>();

    bool showTraceDetail = false;
    TreeLineDto? currentTimeLine = null;
    TreeLineDto defaultTimeLine = default!;
    List<string> services = new();
    List<string> selectedServices = new();
    string? traceLinkUrl = default, spanLinkUrl = default;
    private string? urlService, UrlEndpoint, urlMethod;

    private static List<string> colors = new() {
        "rgb(84, 179, 153)",
        "#5b9bd5","#ed7d31","#70ad47","#ffc000","#4472c4","#91d024","#b235e6","#02ae75",
        "#a565ef","#628cee","#eb9358","#bb60b2","#433e7c","#f47a75","#009db2","#024b51","#0780cf","#765005"
    };

    string ShowTimeLineIcon
    {
        get
        {
            return $"fa:fas fa-angles-{(showTimeLine ? "down" : "up")}";
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        var str = $"{JsonSerializer.Serialize(Data)}";
        var key = Encrypt(str);
        if (lastKey != key)
        {
            loading = true;
            lastKey = key;
            await CaculateTimelines(Data?.ToList());
            loading = false;
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var uri = NavigationManager.ToAbsoluteUri(CurrentUrl);
        var values = HttpUtility.ParseQueryString(uri.Query);
        urlService = values.Get("service")!;
        UrlEndpoint = values.Get("endpoint")!;
        urlMethod = values.Get("method")!;
    }  

    protected override async Task OnInitializedAsync()
    {
        var statusCodes = await TscClient.ApmService.GetStatusCodesAsync();
        if (statusCodes != null && statusCodes.Any())
        {
            errorStatus = statusCodes.Where(item => !string.IsNullOrEmpty(item)).Select(code => Convert.ToInt32(code)).ToArray();
        }
        else
        {
            errorStatus = Array.Empty<int>();
        }
        await base.OnInitializedAsync();
    }

    private async Task CaculateTimelines(List<TraceResponseDto>? traces)
    {
        if (string.IsNullOrEmpty(urlService) && !string.IsNullOrEmpty(Service))
            urlService = Service;
        traceLinkUrl = default;
        spanLinkUrl = default;
        timeLines.Clear();
        services.Clear();
        selectedServices.Clear();
        if (traces == null || !traces.Any())
            return;

        var spanIds = new List<string>();
        var index = 0;
        do
        {
            if (!spanIds.Contains(traces[index].SpanId))
            {
                spanIds.Add(traces[index].SpanId);
                index++;
            }
            else
                traces.RemoveAt(index);
        }
        while (traces.Count - index >= 1);
        //长连接，只取前100个
        int limit = traces.Count;
        if (limit - 100 >= 0)
        {
            limit = 100;
        }

        start = traces.Min(item => item.Timestamp);
        end = traces.Max(item => item.EndTimestamp);
        CaculateTotalTime(start, end);

        do
        {
            SetRootTimeLine(traces);
        }
        while (traces.Count > 0);
        //SetInitialValues();
        defaultTimeLine = GetDefaultLine()!;
        if (OnSpanIdChanged.HasDelegate)
            await OnSpanIdChanged.InvokeAsync(defaultTimeLine?.Trace?.SpanId);
        timeLines = timeLines.OrderBy(item => item.Trace.Timestamp).ToList();
        services = services.Distinct().ToList();
        selectedServices.Add(defaultTimeLine!.ServiceName!);
        traceLinkUrl = GetUrl(defaultTimeLine);
    }

    private TreeLineDto? GetDefaultLine()
    {
        if (IsMaui)
            return GetMauiDefaultLines(timeLines);
        return GetDefaultLine(timeLines);
    }

    private TreeLineDto? GetDefaultLine(List<TreeLineDto>? data)
    {
        if (data == null || !data.Any())
            return default;
        foreach (var item in data)
        {
            if (IsTarget(item))
                return item;
            var find = GetDefaultLine(item.Children);
            if (find != null) return find;
        }
        return default;
    }

    private bool IsTarget(TreeLineDto item)
    {
        if ((item.Trace.Kind == "SPAN_KIND_SERVER" || item.Trace.Kind == "Server")
               && item.Trace.Resource.TryGetValue("telemetry.sdk.version", out var sdkVersion))
        {
            return true;
        }
        return false;
    }

    private static bool IsNullOrEquals(Dictionary<string, object> values, string key, string? urlValue = default)
    {
        if (string.IsNullOrEmpty(urlValue)) return true;
        return values.TryGetValue(key, out var value) && string.Equals(urlValue, value.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }

    private TreeLineDto? GetMauiDefaultLines(List<TreeLineDto>? data)
    {
        if (!IsMaui || data == null || !data.Any() || string.IsNullOrEmpty(RoutePath))
            return default;
        foreach (var item in data)
        {
            if (item.Trace.Resource.TryGetValue("telemetry.sdk.version", out var sdkVersion))
            {
                if (string.Equals(sdkVersion.ToString(), OpenTelemetrySdks.OpenTelemetrySdk1_5_1_Lonsid) || string.Equals(sdkVersion.ToString(), OpenTelemetrySdks.OpenTelemetrySdk1_5_1))
                {
                    if (item.Trace.Attributes.TryGetValue("http.target", out var target) && string.Equals(RoutePath, target.ToString()!, StringComparison.CurrentCultureIgnoreCase))
                        return item;
                }
            }
            var find = GetMauiDefaultLines(item.Children);
            if (find != null) return find;
        }
        return default;
    }

    private void SetRootTimeLine(List<TraceResponseDto> traces)
    {
        var roots = GetEmptyParentNodes(traces!);
        if (!roots.Any())
        {
            roots = GetInteruptRootNodes(traces!);
            if (!roots.Any())
                roots = traces.OrderBy(t => t.Timestamp).ToList();
        }
        services.AddRange(traces.Select(item => item.Resource["service.name"].ToString())!);

        foreach (var item in roots)
        {
            traces.Remove(item);
            var timeLine = new TreeLineDto
            {
                ParentSpanId = item.ParentSpanId,
                SpanId = item.SpanId,
                Trace = item,
                Children = GetChildren(item.SpanId, traces)
            };
            timeLine.SetValue(item, start, end, totalDuration, errorStatus);
            var spanError = Errors?.FirstOrDefault(t => t.X == item.SpanId);
            if (spanError != null)
            {
                timeLine.ErrorCount = Convert.ToInt32(spanError.Y);
            }
            timeLines.Add(timeLine);
        }
    }

    private List<TreeLineDto> GetChildren(string spanId, List<TraceResponseDto> traces)
    {
        var children = traces.Where(item => !string.IsNullOrEmpty(item.ParentSpanId) && item.ParentSpanId == spanId).ToList();
        if (!children.Any()) return default!;
        var result = new List<TreeLineDto>();
        foreach (var item in children)
        {
            traces.Remove(item);
            var timeLine = new TreeLineDto
            {
                ParentSpanId = item.ParentSpanId,
                SpanId = item.SpanId,
                Trace = item,
                Children = GetChildren(item.SpanId, traces)
            };
            timeLine.SetValue(item, start, end, totalDuration, errorStatus);
            result.Add(timeLine);
        }
        return result;
    }

    private async Task LoadPageAsync(int page)
    {
        Page = page;
        loading = true;
        if (PageChanged.HasDelegate)
            await PageChanged.InvokeAsync(Page);
    }

    /// <summary>
    /// 完整的parentId为空的roots
    /// </summary>
    /// <param name="traces"></param>
    /// <returns></returns>
    private List<TraceResponseDto> GetEmptyParentNodes(List<TraceResponseDto> traces)
    {
        return traces.Where(item => string.IsNullOrEmpty(item.ParentSpanId)).OrderBy(t => t.Timestamp).ToList();
    }

    private void ShowOrHideLine(TreeLineDto item)
    {
        showTimeLine = true;
        item.Show = !item.Show;
    }

    private void ShowOrHideAll()
    {
        showTimeLine = !showTimeLine;
        //StateHasChanged();
    }

    private void ShowTraceDetail(TreeLineDto current)
    {
        spanLinkUrl = GetUrl(current, true);
        currentTimeLine = current;
        showTraceDetail = true;
    }

    /// <summary>
    /// 截断的trace,取最外面的作为roots
    /// </summary>
    /// <param name="traces"></param>
    /// <returns></returns>
    private List<TraceResponseDto> GetInteruptRootNodes(List<TraceResponseDto> traces)
    {
        //var parentIds = traces.Select(item => item.ParentSpanId).Distinct().ToList();
        var allSpanIds = traces.Select(item => item.SpanId).Distinct().ToList();
        var roots = traces.Where(item => !allSpanIds.Contains(item.ParentSpanId));
        return roots.OrderBy(t => t.Timestamp).ToList();
    }

    private void CaculateTotalTime(DateTime start, DateTime end)
    {
        totalDuration = (int)Math.Floor((end - start).TotalMilliseconds);
        stepDuration = totalDuration / 8;
        lastDuration = totalDuration;
    }

    private string GetUrl(TreeLineDto? current, bool isSpan = false, string baseUrl = "/apm/logs")
    {
        if (current == null)
            return string.Empty;
        string spanId = current.Trace.SpanId;
        return $"{baseUrl}{GetUrlParam(service: current.ServiceName, env: current.Trace.Resource.TryGetValue("service.namespace", out var env) ? env.ToString() : "", start: start.AddSeconds(-1), end: end.AddSeconds(1), traceId: current.Trace.TraceId, spanId: isSpan ? spanId : default)}";
    }

    private async Task OpenLogAsync(TreeLineDto item)
    {
        var url = GetUrl(item, true, "/apm/errors");
        await JSRuntime.InvokeVoidAsync("open", url, "_blank");
    }

    private async Task OpenTraceLogAsync()
    {
        await JSRuntime.InvokeVoidAsync("open", traceLinkUrl, "_blank");
    }

    private string GetServiceStyle(string service)
    {
        return selectedServices.Contains(service) ? "background-color: rgb(211, 218, 230)" : "";
    }

    private void OnServiceSelected(string service)
    {
        if (service == urlService)
            return;

        if (selectedServices.Contains(service))
            selectedServices.Remove(service);
        else
            selectedServices.Add(service);

        StateHasChanged();
    }
}