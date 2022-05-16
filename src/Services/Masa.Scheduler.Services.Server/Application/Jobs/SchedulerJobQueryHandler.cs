// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class SchedulerJobQueryHandler
{
    readonly ISchedulerJobRepository _schedulerJobRepository;
    readonly SchedulerDbContext _dbContext;

    public SchedulerJobQueryHandler(ISchedulerJobRepository schedulerJobRepository, SchedulerDbContext dbContext)
    {
        _schedulerJobRepository = schedulerJobRepository;
        _dbContext = dbContext;
    }

    [EventHandler]
    public async Task JobListHandleAsync(SchedulerJobQuery query)
    {
        var request = query.Request;

        Expression<Func<SchedulerJob, bool>> condition = job => true;

        if (request.IsCreatedByManual)
        {
            condition.And(job => job.Origin == string.Empty);
        }
        else
        {
            condition.And(job => job.Origin != string.Empty);
        }

        if (request.FilterStatus != 0)
        {
            condition = condition.And(job => job.LastRunStatus == request.FilterStatus);
        }

        if (!string.IsNullOrEmpty(request.JobName))
        {
            condition = condition.And(job => job.Name.Contains(request.JobName));
        }

        switch (request.QueryTimeType)
        {
            case JobQueryTimeTypes.ScheduleTime:
                if (request.QueryStartTime.HasValue)
                {
                    condition = condition.And(job => job.LastScheduleTime >= request.QueryStartTime);
                }
                if (request.QueryEndTime.HasValue)
                {
                    condition = condition.And(job => job.LastScheduleTime < request.QueryEndTime);
                }
                break;
            case JobQueryTimeTypes.RunStartTime:
                if (request.QueryStartTime.HasValue)
                {
                    condition = condition.And(job => job.LastRunStartTime >= request.QueryStartTime);
                }
                if (request.QueryEndTime.HasValue)
                {
                    condition = condition.And(job => job.LastRunStartTime < request.QueryEndTime);
                }
                break;
            case JobQueryTimeTypes.RunEndTime:
                if (request.QueryStartTime.HasValue)
                {
                    condition = condition.And(job => job.LastRunEndTime >= request.QueryStartTime);
                }
                if (request.QueryEndTime.HasValue)
                {
                    condition = condition.And(job => job.LastRunEndTime < request.QueryEndTime);
                }
                break;
        }

        if (request.JobType != 0)
        {
            condition = condition.And(job => job.JobType == request.JobType);
        }

        if (!string.IsNullOrEmpty(request.Origin))
        {
            condition = condition.And(job => job.Origin == request.Origin);
        }

        var jobs = await _schedulerJobRepository.GetPaginatedListAsync(condition, new PaginatedOptions()
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Sorting = new Dictionary<string, bool>()
            {
                [nameof(SchedulerJob.ModificationTime)] = true,
                [nameof(SchedulerJob.CreationTime)] = true,
            }
        });

        query.Result = new(jobs.Total, jobs.TotalPages ,jobs.Result);
    }
}
