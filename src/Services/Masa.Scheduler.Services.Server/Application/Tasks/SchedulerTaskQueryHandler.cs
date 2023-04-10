// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Tasks;

public class SchedulerTaskQueryHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;
    private readonly SchedulerDbContext _dbContext;

    public SchedulerTaskQueryHandler(ISchedulerTaskRepository schedulerTaskRepository, IMapper mapper, IEventBus eventBus, SchedulerDbContext dbContext)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _mapper = mapper;
        _eventBus = eventBus;
        _dbContext = dbContext;
    }

    [EventHandler]
    public async Task SchedulerTaskListQuery(SchedulerTaskQuery query)
    {
        var request = query.Request;

        Expression<Func<SchedulerTask, bool>> condition = t => t.JobId == query.Request.JobId;

        condition = condition.And(request.FilterStatus != 0, t => t.TaskStatus == request.FilterStatus);

        switch (request.QueryTimeType)
        {
            case JobQueryTimeTypes.ScheduleTime:
                condition = condition.And(request.QueryStartTime.HasValue, t => t.SchedulerTime >= request.QueryStartTime);
                condition = condition.And(request.QueryEndTime.HasValue, t => t.SchedulerTime < request.QueryEndTime);
                break;
            case JobQueryTimeTypes.RunStartTime:
                condition = condition.And(request.QueryStartTime.HasValue, t => t.TaskRunStartTime >= request.QueryStartTime);
                condition = condition.And(request.QueryEndTime.HasValue, t => t.TaskRunStartTime < request.QueryEndTime);
                break;
            case JobQueryTimeTypes.RunEndTime:
                condition = condition.And(request.QueryStartTime.HasValue, t => t.TaskRunEndTime >= request.QueryStartTime);
                condition = condition.And(request.QueryEndTime.HasValue, t => t.TaskRunEndTime < request.QueryEndTime);
                break;
        }

        condition = condition.And(!string.IsNullOrEmpty(request.Origin), t => t.Origin == request.Origin);

        var skip = (request.Page - 1) * request.PageSize;

        var dbQuery = _dbContext.Tasks.Where(condition);

        var originList = await _dbContext.Tasks.Where(p => !string.IsNullOrWhiteSpace(p.Origin)).Select(p => p.Origin).Distinct().ToListAsync();

        var total = await dbQuery.CountAsync();

        var result = await dbQuery.OrderByDescending(p => p.SchedulerTime).OrderByDescending(p => p.CreationTime).Skip(skip).Take(request.PageSize).ToListAsync();

        var taskDtos = _mapper.Map<List<SchedulerTaskDto>>(result);

        if (taskDtos.Any())
        {
            var userIds = taskDtos.Select(p => p.OperatorId).Distinct().ToList();

            var userQuery = new UserQuery() { UserIds = userIds };

            await _eventBus.PublishAsync(userQuery);

            foreach (var item in taskDtos)
            {
                var user = userQuery.Result.FirstOrDefault(u => u.Id == item.OperatorId);

                if (user != null)
                {
                    item.OperatorName = user.Name;
                }
            }
        }

        var totalPages = (int)Math.Ceiling(total / (decimal)request.PageSize);

        query.Result = new(total, totalPages, taskDtos, originList);
    }
}
