// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class SchedulerJobQueryHandler
{
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly SchedulerDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;

    public SchedulerJobQueryHandler(ISchedulerJobRepository schedulerJobRepository, SchedulerDbContext dbContext, IMapper mapper, IEventBus eventBus)
    {
        _schedulerJobRepository = schedulerJobRepository;
        _dbContext = dbContext;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    [EventHandler]
    public async Task JobListHandleAsync(SchedulerJobQuery query)
    {
        var request = query.Request;

        Expression<Func<SchedulerJob, bool>> condition = job => true;

        if (request.IsCreatedByManual)
        {
            condition = condition.And(job => job.Origin == string.Empty);
        }
        else
        {
            condition = condition.And(job => job.Origin != string.Empty);
        }

        condition = condition.And(request.FilterStatus != 0, job => job.LastRunStatus == request.FilterStatus);

        condition = condition.And(!string.IsNullOrEmpty(request.JobName), job => job.Name.Contains(request.JobName));

        switch (request.QueryTimeType)
        {
            case JobQueryTimeTypes.ScheduleTime:
                condition = condition.And(request.QueryStartTime.HasValue, job => job.LastScheduleTime >= request.QueryStartTime);
                condition = condition.And(request.QueryEndTime.HasValue, job => job.LastScheduleTime < request.QueryEndTime);
                break;
            case JobQueryTimeTypes.RunStartTime:
                condition = condition.And(request.QueryStartTime.HasValue, job => job.LastRunStartTime >= request.QueryStartTime);
                condition = condition.And(request.QueryEndTime.HasValue, job => job.LastRunStartTime < request.QueryEndTime);
                break;
            case JobQueryTimeTypes.RunEndTime:
                condition = condition.And(request.QueryStartTime.HasValue, job => job.LastRunEndTime >= request.QueryStartTime);
                condition = condition.And(request.QueryEndTime.HasValue, job => job.LastRunEndTime < request.QueryEndTime);
                break;
        }

        condition = condition.And(request.JobType != 0, job => job.JobType == request.JobType);

        condition = condition.And(!string.IsNullOrEmpty(request.Origin), job => job.Origin == request.Origin);

        condition = condition.And(job => job.BelongProjectIdentity == request.BelongProjectIdentity);

        var skip = (request.Page - 1) * request.PageSize;

        var dbQuery = _dbContext.Jobs.Where(condition);

        var result = await dbQuery
            .Select(j => new
            {
                job = j,
                SortPriority =
                    j.LastRunStatus == TaskRunStatus.Failure ? 6 :
                    j.LastRunStatus == TaskRunStatus.WaitToRetry ? 5 :
                    j.LastRunStatus == TaskRunStatus.Timeout ? 4 :
                    j.LastRunStatus == TaskRunStatus.TimeoutSuccess ? 3 :
                    j.LastRunStatus == TaskRunStatus.Idle ? 2 :
                    j.LastRunStatus == TaskRunStatus.Running ? 1 : 0
            })
            .OrderByDescending(p => p.job.Enabled)
            .ThenByDescending(p => p.SortPriority)
            .ThenByDescending(p => p.job.ModificationTime).Skip(skip).Take(request.PageSize).ToListAsync();

        var total = await dbQuery.CountAsync();

        var jobList = result.Select(p => p.job).ToList();

        var jobDtos = _mapper.Map<List<SchedulerJobDto>>(jobList);

        if (jobDtos.Any())
        {
            var userIds = jobDtos.Select(p => p.OwnerId).Distinct().ToList();

            var userQuery = new UserQuery() { UserIds = userIds };

            await _eventBus.PublishAsync(userQuery);

            foreach (var item in jobDtos)
            {
                var user = userQuery.Result.FirstOrDefault(u => u.Id == item.OwnerId);

                if (user != null)
                {
                    item.UserName = user.Name;
                    item.Avator = user.Avatar;
                }
            }
        }

        var totalPages = (int)Math.Ceiling(total / (decimal)request.PageSize);

        query.Result = new(total, totalPages, jobDtos);
    }
}
