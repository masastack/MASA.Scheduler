// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller;

public class SchedulerServerCaller : HttpClientCallerBase
{
    SchedulerJobService? _schedulerJobService;
    AuthService? _authService;
    PMService? _pmService;
    SchedulerTaskService? _schedulerTaskService;


    public SchedulerJobService SchedulerJobService => _schedulerJobService ?? (_schedulerJobService = new(CallerProvider));

    public AuthService AuthService => _authService ?? (_authService = new(CallerProvider));

    public PMService PMService => _pmService ?? (_pmService = new(CallerProvider));

    public SchedulerTaskService SchedulerTaskService => _schedulerTaskService ?? (_schedulerTaskService = new(CallerProvider));

    public SchedulerServerCaller(IServiceProvider serviceProvider, SchedulerApiOptions options) : base(serviceProvider)
    {
        Name = nameof(SchedulerServerCaller);
        BaseAddress = options.SchedulerServerBaseAddress;
    }

    protected override string BaseAddress { get; set; }
    public override string Name { get; set; }

    protected override IHttpClientBuilder UseHttpClient()
    {
        return base.UseHttpClient().AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
    }
}
