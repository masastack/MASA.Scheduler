// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.EntityFrameworkCore.Repositories;

public class SchedulerTaskRepository : Repository<SchedulerDbContext, SchedulerTask, Guid>, ISchedulerTaskRepository
{
    public SchedulerTaskRepository(SchedulerDbContext context, IUnitOfWork unitOfWork)
        : base(context, unitOfWork)
    {

    }

    public IQueryable<SchedulerTask> AsQueryable()
    {
        return Context.Set<SchedulerTask>().AsQueryable();
    }

    public async Task<bool> AnyAsync(Expression<Func<SchedulerTask, bool>> predicate)
    {
        return await Context.Set<SchedulerTask>().AnyAsync(predicate);
    }
}
