// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class SchedulerJobQueryHandler
{
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly SchedulerDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;
    private readonly IMemoryCacheClient _memoryCacheClient;

    public SchedulerJobQueryHandler(ISchedulerJobRepository schedulerJobRepository, SchedulerDbContext dbContext, IMapper mapper, IEventBus eventBus, IMemoryCacheClient memoryCacheClient)
    {
        _schedulerJobRepository = schedulerJobRepository;
        _dbContext = dbContext;
        _mapper = mapper;
        _eventBus = eventBus;
        _memoryCacheClient = memoryCacheClient;
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

        var originList = await dbQuery.Where(p => !string.IsNullOrWhiteSpace(p.Origin)).Select(p => p.Origin).Distinct().ToListAsync();

        var total = await dbQuery.CountAsync();

        var result = await dbQuery.OrderByDescending(p => p.ModificationTime).ThenByDescending(p => p.CreationTime).Skip(skip).Take(request.PageSize).ToListAsync();

        var jobDtos = _mapper.Map<List<SchedulerJobDto>>(result);

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

        query.Result = new(total, totalPages, jobDtos, originList);
    }
}
