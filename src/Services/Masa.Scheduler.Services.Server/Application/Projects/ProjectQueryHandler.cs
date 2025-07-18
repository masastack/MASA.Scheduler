﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Projects;

public class ProjectQueryHandler
{
    private readonly IPmClient _pmClient;
    private readonly IMapper _mapper;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;

    public ProjectQueryHandler(IPmClient pmClient, IMapper mapper, IMultiEnvironmentContext multiEnvironmentContext)
    {
        _pmClient = pmClient;
        _mapper = mapper;
        _multiEnvironmentContext = multiEnvironmentContext;
    }

    [EventHandler]
    public async Task ProjectListHandleAsync(ProjectQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Environment))
        {
            query.Environment = _multiEnvironmentContext.CurrentEnvironment;
        }

        var projectList = await _pmClient.ProjectService.GetProjectAppsAsync(query.Environment);

        if (query.TeamId.HasValue)
        {
            projectList = projectList.FindAll(p => p.TeamIds != null && p.TeamIds.Contains(query.TeamId.Value));
        }

        query.Result = projectList.Select(p => new ProjectDto()
        {
            Name = p.Name,
            Id = p.Id,
            Identity = p.Identity,
            TeamIds = p.TeamIds ?? new List<Guid>(),
            ProjectApps = p.Apps.DistinctBy(p => p.Identity).Select(app => new ProjectAppDto() { Id = app.Id, Identity = app.Identity, Name = app.Name, ProjectId = app.ProjectId, Type = Enum.Parse<ProjectAppTypes>(app.Type.ToString()) }).ToList(),
        }).ToList();
    }

    [EventHandler]
    public async Task GetProjectDetailsAsync(ProjectDetailsQuery query)
    {
        var projectDetails = await _pmClient.ProjectService.GetByIdentityAsync(query.ProjectIdentity);

        var dto = _mapper.Map<ProjectDto>(projectDetails);

        query.Result = dto;
    }
}
