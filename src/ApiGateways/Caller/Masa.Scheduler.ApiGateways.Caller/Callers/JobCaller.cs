// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace MASA.Scheduler.ApiGateways.Caller.Callers
{
    public class JobCaller : HttpClientCallerBase
    {
        protected override string BaseAddress { get; set; } = "http://localhost:16002";

        public JobCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(JobCaller);
        }

        public async Task<List<JobDto>> GetListAsync()
        {
            var result = await CallerProvider.GetAsync<List<JobDto>>($"job/list");
            return result ?? new List<JobDto>();
        }
    }
}