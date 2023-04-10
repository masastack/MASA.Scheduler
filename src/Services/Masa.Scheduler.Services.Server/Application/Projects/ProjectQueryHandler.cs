// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Projects;

public class ProjectQueryHandler
{
    private readonly IPmClient _pmClient;
    private readonly IMapper _mapper;
    private readonly IMultiEnvironmentUserContext _userContext;
    private readonly IWebHostEnvironment _environment;

    public ProjectQueryHandler(IPmClient pmClient, IMapper mapper, IMultiEnvironmentUserContext userContext, IWebHostEnvironment environment)
    {
        _pmClient = pmClient;
        _mapper = mapper;
        _userContext = userContext;
        _environment = environment;
    }

    [EventHandler]
    public async Task ProjectListHandleAsync(ProjectQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Environment))
        {
            query.Environment = _userContext.Environment ?? _environment.EnvironmentName;
        }

        var projectList = await _pmClient.ProjectService.GetProjectAppsAsync(query.Environment);

        if (query.TeamId.HasValue)
        {
            projectList = projectList.FindAll(p => p.TeamId == query.TeamId.Value);
        }

        query.Result = projectList.Select(p => new ProjectDto()
        {
            Name = p.Name,
            Id = p.Id,
            Identity = p.Identity,
            TeamId = p.TeamId,
            ProjectApps = p.Apps.DistinctBy(p => p.Identity).Select(app => new ProjectAppDto() { Id = app.Id, Identity = app.Identity, Name = app.Name, ProjectId = app.ProjectId, Type = Enum.Parse<ProjectAppTypes>(app.Type.ToString())}).ToList(),
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
