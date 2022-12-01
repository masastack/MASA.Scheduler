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
    TokenProvider _tokenProvider;

    public SchedulerJobService SchedulerJobService => _schedulerJobService ??= new(Caller);

    public AuthService AuthService => _authService ??= new(Caller);

    public PmService PmService => _pmService ??= new(Caller);

    public SchedulerTaskService SchedulerTaskService => _schedulerTaskService ??= new(Caller);

    public SchedulerResourceService SchedulerResourceService => _schedulerResourceService ??= new(Caller);

    public OssService OssService => _ossService ??= new(Caller);

    public SchedulerServerManagerService SchedulerServerManagerService => _schedulerServerManagerService ??= new(Caller);

    public SchedulerServerCaller(
        IServiceProvider serviceProvider,
        TokenProvider tokenProvider,
        SchedulerApiOptions options) : base(serviceProvider)
    {
        Name = nameof(SchedulerServerCaller);
        BaseAddress = options.SchedulerServerBaseAddress;
        _tokenProvider = tokenProvider;
    }

    protected override string BaseAddress { get; set; }

    public override string? Name { get; set; }

    protected override async Task ConfigHttpRequestMessageAsync(HttpRequestMessage requestMessage)
    {
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
        await base.ConfigHttpRequestMessageAsync(requestMessage);
    }
}
