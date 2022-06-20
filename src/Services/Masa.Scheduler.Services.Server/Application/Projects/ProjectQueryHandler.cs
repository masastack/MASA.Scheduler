// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Projects;

public class ProjectQueryHandler
{
    private readonly IPmClient _pmClient;
    
    public ProjectQueryHandler(IPmClient pmClient)
    {
        _pmClient = pmClient;
    }

    [EventHandler]
    public async Task ProjectListHandleAsync(ProjectQuery query)
    {
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
            ProjectApps = p.Apps.DistinctBy(p=> p.Identity).Select(app => new ProjectAppDto() { Id = app.Id, Identity = app.Identity, Name = app.Name, ProjectId = app.ProjectId }).ToList(),
        }).ToList();
    }
}
