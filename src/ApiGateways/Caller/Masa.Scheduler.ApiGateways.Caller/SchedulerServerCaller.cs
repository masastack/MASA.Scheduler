// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller;

public class SchedulerServerCaller : HttpClientCallerBase
{
    SchedulerJobService? _schedulerJobService;
    AuthService? _authService;
    PmService? _pmService;
    SchedulerTaskService? _schedulerTaskService;
    SchedulerResourceService? _schedulerResourceService;
    OssService? _ossService;
    SchedulerServerManagerService? _schedulerServerManagerService;
    
    public SchedulerJobService SchedulerJobService => _schedulerJobService ??= new(CallerProvider);

    public AuthService AuthService => _authService ??= new(CallerProvider);

    public PmService PmService => _pmService ??= new(CallerProvider);

    public SchedulerTaskService SchedulerTaskService => _schedulerTaskService ??= new(CallerProvider);

    public SchedulerResourceService SchedulerResourceService => _schedulerResourceService ??= new(CallerProvider);

    public OssService OssService => _ossService ??= new(CallerProvider);

    public SchedulerServerManagerService SchedulerServerManagerService => _schedulerServerManagerService ??= new(CallerProvider);

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
