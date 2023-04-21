// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Extensions.TracingAop;

public class TaskHttpClientTracingInterceptor : HttpClientTracingInterceptorBase
{
    public override void OnHttpResponseMessage(Activity activity, HttpResponseMessage responseMessage)
    {
    }

    public override void OnHttpRequestMessage(Activity activity, HttpRequestMessage requestMessage)
    {
        var queryString = requestMessage.RequestUri!.Query.Trim('?');
        if (queryString.Contains("traceId"))
        {
            var traceIdQuery = queryString.Split("&").FirstOrDefault(e => e.Contains("traceId"));
            var spanIdQuery = queryString.Split("&").FirstOrDefault(e => e.Contains("spanId"));
            var traceId = traceIdQuery!.Split('=')[1];
            var spanId = spanIdQuery!.Split('=')[1];

            if (traceId.IsNullOrEmpty() == false && spanId.IsNullOrEmpty() == false)
            {
                activity.SetParentId(ActivityTraceId.CreateFromString(traceId), ActivitySpanId.CreateFromString(spanId));
            }
        }

    }

    public override void OnException(Activity activity, Exception exception)
    {

    }
}
