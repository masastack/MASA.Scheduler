// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class SchedulerJobCommandHandler
{
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly IMapper _mapper;
    private readonly SchedulerJobDomainService _schedulerJobDomainService;
    private readonly IEventBus _eventBus;

    public SchedulerJobCommandHandler(ISchedulerJobRepository schedulerJobRepository, IMapper mapper, SchedulerJobDomainService schedulerJobDomainService, IEventBus eventBus)
    {
        _schedulerJobRepository = schedulerJobRepository;
        _mapper = mapper;
        _schedulerJobDomainService = schedulerJobDomainService;
        _eventBus = eventBus;
    }

    [EventHandler]
    public async Task AddHandleAsync(AddSchedulerJobCommand command)
    {
        command.Request.Data.UpdateExpiredStrategyTime = DateTimeOffset.Now;

        var job = _mapper.Map<SchedulerJob>(command.Request.Data);

        if (!string.IsNullOrWhiteSpace(job.JobIdentity))
        {
            var sameJobIdentity = await _schedulerJobRepository.FindAsync(p => p.JobIdentity == job.JobIdentity && p.BelongProjectIdentity == job.BelongProjectIdentity);
            if(sameJobIdentity != null)
            {
                throw new UserFriendlyException("JobIdentity already exists");
            }
        }

        var result = await _schedulerJobRepository.AddAsync(job);

        await _schedulerJobRepository.UnitOfWork.SaveChangesAsync();

        await _schedulerJobRepository.UnitOfWork.CommitAsync();

        var dto = _mapper.Map<SchedulerJobDto>(result);

        command.Result = dto;

        var request = new UpdateCronJobRequest()
        {
            JobId = job.Id,
            CronExpression = job.CronExpression,
            Enabled = job.Enabled,
            ScheduleType = job.ScheduleType
        };

        await _schedulerJobDomainService.UpdateCronJobAsync(request);
    }

    [EventHandler]
    public async Task UpdateHandleAsync(UpdateSchedulerJobCommand command)
    {
        var jobDto = command.Request.Data;

        var job = await _schedulerJobRepository.FindAsync(job => job.Id == jobDto.Id);

        if(job is null)
        {
            throw new UserFriendlyException($"The current job does not exist");
        }

        if(job.ScheduleExpiredStrategy != jobDto.ScheduleExpiredStrategy || job.CronExpression != jobDto.CronExpression)
        {
            jobDto.UpdateExpiredStrategyTime = DateTimeOffset.Now;
        }

        job.UpdateJob(jobDto);

        await _schedulerJobRepository.UpdateAsync(job);

        var request = new UpdateCronJobRequest()
        {
            JobId = job.Id,
            CronExpression = job.CronExpression,
            Enabled = job.Enabled,
            ScheduleType = job.ScheduleType
        };

        await _schedulerJobDomainService.UpdateCronJobAsync(request);
    }

    [EventHandler]
    public async Task RemoveHandleAsync(RemoveSchedulerJobCommand command)
    {
        await _schedulerJobDomainService.RemoveSchedulerJobAsync(command.Request);
    }

    [EventHandler]
    public async Task ChangeEnabledStatusHandleAsync(ChangeEnableStatusSchedulerJobCommand command)
    {
        var job = await _schedulerJobRepository.FindAsync(command.Request.JobId);

        if (job is null)
        {
            throw new UserFriendlyException($"Job id {command.Request.JobId}, not found");
        }

        job.ChangeEnableStatus(command.Request.Enabled);

        await _schedulerJobRepository.UpdateAsync(job);

        var jobDto = _mapper.Map<SchedulerJobDto>(job);

        var request = new UpdateCronJobRequest()
        {
            JobId = job.Id,
            CronExpression = job.CronExpression,
            Enabled = job.Enabled,
            ScheduleType = job.ScheduleType
        };

        await _schedulerJobDomainService.UpdateCronJobAsync(request);
    }

    [EventHandler]
    public async Task StartJobHandleAsync(StartSchedulerJobCommand command)
    {
        command.Request.ExcuteTime = DateTimeOffset.Now;
        await _schedulerJobDomainService.StartJobAsync(command.Request);
    }

    [EventHandler]
    public async Task AddSchedulerJobBySdkAsync(AddSchedulerJobBySdkCommand command)
    {
        var request = command.Request;

        switch (request.JobType)
        {
            case JobTypes.JobApp:
                if (request.JobAppConfig == null)
                {
                    throw new UserFriendlyException($"JobAppconfig cannot null");
                }
                break;
            case JobTypes.Http:
                if (request.HttpConfig == null)
                {
                    throw new UserFriendlyException($"HttpConfig cannot null");
                }
                break;
            case JobTypes.DaprServiceInvocation:
                if (request.DaprServiceInvocationConfig == null)
                {
                    throw new UserFriendlyException($"DaprServiceInvocationConfig cannot null");
                }
                break;
        }

        var projectDetailsQuery = new ProjectDetailsQuery()
        {
            ProjectIdentity = request.ProjectIdentity
        };

        await _eventBus.PublishAsync(projectDetailsQuery);

        if(projectDetailsQuery.Result == null)
        {
            throw new UserFriendlyException($"ProjectDetails not found, ProjectIdentity: {request.ProjectIdentity}");
        }

        var schedulerJobDto = _mapper.Map<SchedulerJobDto>(request);

        schedulerJobDto.BelongProjectIdentity = request.ProjectIdentity;

        schedulerJobDto.BelongTeamId = projectDetailsQuery.Result.TeamId;

        schedulerJobDto.Origin = projectDetailsQuery.Result.Name;

        if(request.OperatorId != Guid.Empty)
        {
            var query = new UserQuery();
            query.UserIds.Add(request.OperatorId);

            await _eventBus.PublishAsync(query);

            var owner = query.Result.FirstOrDefault();

            if (owner != null)
            {
                schedulerJobDto.Owner = owner.Name;
            }
        }

        schedulerJobDto.OwnerId = request.OperatorId;

        schedulerJobDto.ScheduleType = string.IsNullOrWhiteSpace(schedulerJobDto.CronExpression) ? ScheduleTypes.ManualRun : ScheduleTypes.Cron;

        schedulerJobDto.FailedStrategy = schedulerJobDto.FailedRetryCount == 0 ? FailedStrategyTypes.Manual : FailedStrategyTypes.Auto;

        schedulerJobDto.RoutingStrategy = RoutingStrategyTypes.RoundRobin;

        schedulerJobDto.Enabled = true;

        schedulerJobDto.HttpConfig ??= new();

        schedulerJobDto.JobAppConfig ??= new();

        schedulerJobDto.DaprServiceInvocationConfig ??= new();

        var addCommand = new AddSchedulerJobCommand(new AddSchedulerJobRequest()
        {
            Data = schedulerJobDto,
        });

        await _eventBus.PublishAsync(addCommand);

        command.Result = addCommand.Result;
    }

    [EventHandler]
    public async Task UpsertAlarmRuleHandleAsync(UpsertAlarmRuleCommand command)
    {
        var job = await _schedulerJobRepository.FindAsync(job => job.Id == command.JobId);
        MasaArgumentException.ThrowIfNull(job);

        job.SetAlarmRuleId(command.AlarmRuleId);

        await _schedulerJobRepository.UpdateAsync(job);
    }
}
