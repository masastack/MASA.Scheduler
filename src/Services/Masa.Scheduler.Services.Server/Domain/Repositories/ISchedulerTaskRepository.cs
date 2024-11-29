// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Repositories;

public interface ISchedulerTaskRepository : IRepository<SchedulerTask, Guid>
{
    IQueryable<SchedulerTask> AsQueryable();

    Task<bool> AnyAsync(Expression<Func<SchedulerTask, bool>> predicate);
}
