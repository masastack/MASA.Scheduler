// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Shared;

public class TscComponentBase : ProComponentBase
{
    [Inject]
    public ITscClient TscClient { get; set; } = default!;

    protected override void OnInitialized()
    {
        StorageConst.Init(new ClickhouseStorageConst());
        base.OnInitialized();
    }

    [Inject]
    public JsInitVariables JsInitVariables { get; set; } = default!;

    protected ApmSearchComponent apmSearchComponent = default!;

    [Inject]
    public IMultiEnvironmentUserContext UserContext { get; set; } = default!;

    public virtual bool IsNeedRefresh { get; set; }

    public Guid CurrentTeamId { get; set; } = Guid.Empty;

    public TimeZoneInfo CurrentTimeZone { get; set; } = TimeZoneInfo.Utc;

    public static string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return string.Join("", SHA1.HashData(Encoding.UTF8.GetBytes(text)).Select(b => b.ToString("x2")));
    }

    public string CurrentUrl { get; set; } = default!;


    public static string GetUrlParam(string? service = default,
         string? env = default,
         string? endpoint = default,
         DateTime? start = default,
         DateTime? end = default,
         ApmComparisonTypes? comparisonType = default,
         string? exType = default,
         string? exMsg = default,
         string? traceId = default,
         string? spanId = default,
         string? search = default,
         string? method = default,
         string? statusCode = default)
    {
        var text = new StringBuilder();
        if (!string.IsNullOrEmpty(env))
            text.AppendFormat("&env={0}", HttpUtility.UrlEncode(env));
        if (!string.IsNullOrEmpty(service))
            text.AppendFormat("&service={0}", HttpUtility.UrlEncode(service));
        if (!string.IsNullOrEmpty(endpoint))
            text.AppendFormat("&endpoint={0}", HttpUtility.UrlEncode(endpoint));
        if (comparisonType.HasValue)
            text.AppendFormat("&comparison={0}", (int)comparisonType);
        if (start.HasValue && start.Value > DateTime.MinValue)
            text.AppendFormat("&start={0}", HttpUtility.UrlEncode(start.Value.ToString("yyyy-MM-dd HH:mm:ss")));
        if (end.HasValue && end.Value > DateTime.MinValue)
            text.AppendFormat("&end={0}", HttpUtility.UrlEncode(end.Value.ToString("yyyy-MM-dd HH:mm:ss")));
        if (!string.IsNullOrEmpty(exType))
            text.AppendFormat("&ex_type={0}", HttpUtility.UrlEncode(exType));
        if (!string.IsNullOrEmpty(exMsg))
            text.AppendFormat("&ex_msg={0}", HttpUtility.UrlEncode(exMsg).Replace(".", "x2E"));
        if (!string.IsNullOrEmpty(traceId))
            text.AppendFormat("&traceId={0}", HttpUtility.UrlEncode(traceId));
        if (!string.IsNullOrEmpty(spanId))
            text.AppendFormat("&spanId={0}", HttpUtility.UrlEncode(spanId));
        if (!string.IsNullOrEmpty(search))
            text.AppendFormat("&search={0}", HttpUtility.UrlEncode(search));
        if (!string.IsNullOrEmpty(method))
            text.AppendFormat("&method={0}", method);
        if (!string.IsNullOrEmpty(statusCode))
            text.AppendFormat("&status={0}", statusCode);

        if (text.Length > 0)
            text.Remove(0, 1).Insert(0, "?");
        return text.Remove(0, 1).Insert(0, "?").ToString();
    }

    public EnvironmentAppDto? GetService(string? service, Func<string, EnvironmentAppDto?>? func = default)
    {
        if (string.IsNullOrEmpty(service)) return default;
        if (func != null)
        {
            var value = func.Invoke(service);
            if (value != null)
                return value;
        }

        if (apmSearchComponent != null)
            return apmSearchComponent.GetService(service);
        return null;
    }
}
