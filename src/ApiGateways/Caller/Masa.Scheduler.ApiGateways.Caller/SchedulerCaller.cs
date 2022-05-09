// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller
{
    public class SchedulerCaller : HttpClientCallerBase
    {
        JobService? _jobService;
        AuthService? _authService;

        public JobService JobService => _jobService ?? (_jobService = new(CallerProvider));

        public AuthService AuthService => _authService ?? (_authService = new(CallerProvider));

        public SchedulerCaller(IServiceProvider serviceProvider, SchedulerApiOptions options) : base(serviceProvider)
        {
            Name = nameof(SchedulerCaller);
            BaseAddress = options.SchedulerServerBaseAddress;
        }

        protected override string BaseAddress { get; set; }
        public override string Name { get; set; }

        protected override IHttpClientBuilder UseHttpClient()
        {
            return base.UseHttpClient().AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
        }
    }
}
