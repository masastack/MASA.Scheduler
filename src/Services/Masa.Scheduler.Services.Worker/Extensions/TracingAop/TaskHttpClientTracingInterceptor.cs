// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Extensions.TracingAop;

public class TaskHttpClientTracingInterceptor : HttpClientTracingInterceptorBase
{
    public override void OnHttpResponseMessage(Activity activity, HttpResponseMessage responseMessage)
    {
        //var queryString = responseMessage.RequestMessage!.RequestUri!.Query.Trim('?');
        //if (queryString.Contains("taskId"))
        //{
        //    var taskIdQuery = queryString.Split("&").FirstOrDefault(e => e.Contains("taskId"));
        //    if (taskIdQuery != null && Guid.TryParse(taskIdQuery.Split('=')[1], out Guid taskId))
        //    {
        //        var eventBus = ServiceProvider!.GetRequiredService<IEventBus>();
        //        eventBus.PublishAsync(new SetHttpTaskTracingIntegrationEvent { TaskId = taskId, TraceId = activity?.TraceId.ToString() }).ConfigureAwait(false).GetAwaiter();
        //    }
        //}
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

            activity.SetParentId(ActivityTraceId.CreateFromString(traceId), ActivitySpanId.CreateFromString(spanId));
        }

    }

    public override void OnException(Activity activity, Exception exception)
    {

    }
}
