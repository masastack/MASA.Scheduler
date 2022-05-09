// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Repositories;

public class JobRepository : Repository<SchedulerDbContext, Job, Guid>, IJobRepository
{
    public JobRepository(SchedulerDbContext context, IUnitOfWork unitOfWork)
        : base(context, unitOfWork)
    {
    }
    public async Task<List<Job>> GetListAsync()
    {
        var data = new List<Job>();
        return await Task.FromResult(data);
    }
}
