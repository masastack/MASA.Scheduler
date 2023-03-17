// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.


namespace Masa.Scheduler.ApiGateways.Caller;

public class SchedulerServerCaller : StackHttpClientCaller
{
    SchedulerJobService? _schedulerJobService;
    AuthService? _authService;
    PmService? _pmService;
    SchedulerTaskService? _schedulerTaskService;
    SchedulerResourceService? _schedulerResourceService;
    OssService? _ossService;
    SchedulerServerManagerService? _schedulerServerManagerService;

    public SchedulerJobService SchedulerJobService => _schedulerJobService ??= new(Caller);

    public AuthService AuthService => _authService ??= new(Caller);

    public PmService PmService => _pmService ??= new(Caller);

    public SchedulerTaskService SchedulerTaskService => _schedulerTaskService ??= new(Caller);

    public SchedulerResourceService SchedulerResourceService => _schedulerResourceService ??= new(Caller);

    public OssService OssService => _ossService ??= new(Caller);

    public SchedulerServerManagerService SchedulerServerManagerService => _schedulerServerManagerService ??= new(Caller);

    public SchedulerServerCaller(SchedulerApiOptions options)
    {
        BaseAddress = options.SchedulerServerBaseAddress;
    }

    protected override string BaseAddress { get; set; }
}
