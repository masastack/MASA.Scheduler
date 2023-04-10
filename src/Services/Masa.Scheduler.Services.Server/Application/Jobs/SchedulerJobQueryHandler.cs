﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class SchedulerJobQueryHandler
{
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly SchedulerDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;
    private readonly IAuthClient _authClient;

    public SchedulerJobQueryHandler(ISchedulerJobRepository schedulerJobRepository, SchedulerDbContext dbContext, IMapper mapper, IEventBus eventBus, IAuthClient authClient)
    {
        _schedulerJobRepository = schedulerJobRepository;
        _dbContext = dbContext;
        _mapper = mapper;
        _eventBus = eventBus;
        _authClient = authClient;
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
            case JobQueryTimeTypes.ModificationTime:
                condition = condition.And(request.QueryStartTime.HasValue, job => job.ModificationTime >= request.QueryStartTime);
                condition = condition.And(request.QueryEndTime.HasValue, job => job.ModificationTime < request.QueryEndTime);
                break;
            default:
                condition = condition.And(request.QueryStartTime.HasValue, job => job.CreationTime >= request.QueryStartTime);
                condition = condition.And(request.QueryEndTime.HasValue, job => job.CreationTime < request.QueryEndTime);
                break;
        }

        condition = condition.And(request.JobType != 0, job => job.JobType == request.JobType);

        condition = condition.And(!string.IsNullOrEmpty(request.Origin), job => job.Origin == request.Origin);

        condition = condition.And(job => job.BelongProjectIdentity == request.BelongProjectIdentity);

        var skip = (request.Page - 1) * request.PageSize;

        var dbQuery = _dbContext.Jobs.Where(condition);

        var originList = await _dbContext.Jobs.Where(p => !string.IsNullOrWhiteSpace(p.Origin)).Select(p => p.Origin).Distinct().ToListAsync();

        var total = await dbQuery.CountAsync();

        var result = await dbQuery.OrderByDescending(p => p.ModificationTime).ThenByDescending(p => p.CreationTime).Skip(skip).Take(request.PageSize).ToListAsync();

        var jobDtos = _mapper.Map<List<SchedulerJobDto>>(result);

        if (jobDtos.Any())
        {
            var ownerIds = jobDtos.Select(p => p.OwnerId).Distinct().ToList();
            var modifierIds = jobDtos.Where(x => x.Modifier != default).Select(x => x.Modifier).Distinct().ToList();
            var creatorIds = jobDtos.Where(x => x.Creator != default).Select(x => x.Modifier).Distinct().ToList();
            var userIds = ownerIds.Union(modifierIds).Union(creatorIds).ToArray();
            var userInfos = await _authClient.UserService.GetListByIdsAsync(userIds);

            foreach (var item in jobDtos)
            {
                var user = userInfos.FirstOrDefault(u => u.Id == item.OwnerId);

                if (user != null)
                {
                    item.UserName = user.StaffDislpayName ?? user.DisplayName;
                    item.Avator = user.Avatar;
                }

                item.CreatorName = userInfos.FirstOrDefault(x => x.Id == item.Creator)?.StaffDislpayName ?? string.Empty;
                item.ModifierName = userInfos.FirstOrDefault(x => x.Id == item.Modifier)?.StaffDislpayName ?? string.Empty;
            }
        }

        var totalPages = (int)Math.Ceiling(total / (decimal)request.PageSize);

        query.Result = new(total, totalPages, jobDtos, originList);
    }

    [EventHandler]
    public async Task SchedulerJobQueryByIdentityHandleAsync(SchedulerJobQueryByIdentity query)
    {
        if (string.IsNullOrEmpty(query.Request.JobIdentity) || string.IsNullOrWhiteSpace(query.Request.ProjectIdentity))
        {
            throw new UserFriendlyException("Parameter: JobIdentity and ProjectIdentity cannot be null");
        }

        var job = await _schedulerJobRepository.FindAsync(p => p.JobIdentity == query.Request.JobIdentity && p.JobIdentity == query.Request.ProjectIdentity);

        query.Result = job == null ? null : _mapper.Map<SchedulerJobDto>(job);
    }
}
