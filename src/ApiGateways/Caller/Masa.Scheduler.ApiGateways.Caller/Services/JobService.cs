// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller.Services
{
    public class JobService : ServiceBase
    {
        protected override string BaseUrl { get; set; }

        public JobService(ICallerProvider callerProvider) : base(callerProvider)
        {
            BaseUrl = "api/job/";
        }

        public async Task<List<JobDto>> GetListAsync()
        {
            var result = await GetAsync<List<JobDto>>($"List");
            return result ?? new List<JobDto>();
        }
    }
}