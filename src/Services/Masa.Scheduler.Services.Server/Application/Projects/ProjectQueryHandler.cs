// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Projects;

public class ProjectQueryHandler
{
    private readonly IPmClient _pmClient;
    private readonly IMapper _mapper;
    private readonly IMultiEnvironmentUserContext _userContext;

    public ProjectQueryHandler(IPmClient pmClient, IMapper mapper, IMultiEnvironmentUserContext userContext)
    {
        _pmClient = pmClient;
        _mapper = mapper;
        _userContext = userContext;
    }

    [EventHandler]
    public async Task ProjectListHandleAsync(ProjectQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Environment))
        {
            query.Environment = _userContext.Environment ?? "development";
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
            ProjectApps = p.Apps.DistinctBy(p => p.Identity).Where(p => p.Type == BuildingBlocks.BasicAbility.Pm.Enum.AppTypes.Job).Select(app => new ProjectAppDto() { Id = app.Id, Identity = app.Identity, Name = app.Name, ProjectId = app.ProjectId }).ToList(),
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
