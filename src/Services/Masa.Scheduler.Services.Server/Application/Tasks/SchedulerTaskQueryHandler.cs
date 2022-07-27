// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Tasks;

public class SchedulerTaskQueryHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;

    public SchedulerTaskQueryHandler(ISchedulerTaskRepository schedulerTaskRepository, IMapper mapper, IEventBus eventBus)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _mapper = mapper;
        _eventBus = eventBus;
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

        var paginatedResult = await _schedulerTaskRepository.GetPaginatedListAsync(condition, new PaginatedOptions()
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Sorting = new Dictionary<string, bool>()
            {
                [nameof(SchedulerJob.CreationTime)] = true
            }
        });

        var taskDtos = _mapper.Map<List<SchedulerTaskDto>>(paginatedResult.Result);

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

        query.Result = new(paginatedResult.Total, paginatedResult.TotalPages, taskDtos);
    }
}
