// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Extensions.TracingAop;

public interface IHttpClientTracingInterceptor : ISingletonDependency
{
    public void OnHttpResponseMessage(Activity activity, HttpResponseMessage responseMessage);

    public void OnHttpRequestMessage(Activity activity, HttpRequestMessage requestMessage);

}
