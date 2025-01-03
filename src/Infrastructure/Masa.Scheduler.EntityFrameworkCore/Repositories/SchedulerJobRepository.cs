﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.EntityFrameworkCore.Repositories;

public class SchedulerJobRepository : Repository<SchedulerDbContext, SchedulerJob, Guid>, ISchedulerJobRepository
{
    public SchedulerJobRepository(SchedulerDbContext context, IUnitOfWork unitOfWork)
        : base(context, unitOfWork)
    {
    }
}
