// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Repositories;

public class SchedulerTaskRepository : Repository<SchedulerDbContext, SchedulerTask, Guid>, ISchedulerTaskRepository
{
    public SchedulerTaskRepository(SchedulerDbContext context, IUnitOfWork unitOfWork)
        : base(context, unitOfWork)
    {

    }
}
