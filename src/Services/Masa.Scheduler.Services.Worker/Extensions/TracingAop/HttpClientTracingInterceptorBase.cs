// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Extensions.TracingAop;

public abstract class HttpClientTracingInterceptorBase : IHttpClientTracingInterceptor
{
    private IServiceScope _serviceScope = MasaApp.RootServiceProvider.CreateScope();

    public IServiceProvider? ServiceProvider { get => _serviceScope.ServiceProvider; }

    public abstract void OnException(Activity activity, Exception exception);

    public abstract void OnHttpRequestMessage(Activity activity, HttpRequestMessage requestMessage);

    public abstract void OnHttpResponseMessage(Activity activity, HttpResponseMessage responseMessage);

}
