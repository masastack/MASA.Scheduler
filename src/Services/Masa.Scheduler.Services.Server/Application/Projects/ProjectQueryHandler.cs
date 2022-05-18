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
    public async Task TeamListHandleAsync(ProjectQuery query)
    {
        var projectList = await _pmClient.ProjectService.GetProjectAppsAsync("development");

        query.Result = projectList.FindAll(p => p.TeamId == query.TeamId).Select(p => new ProjectDto()
        {
            Name = p.Name,
            Id = p.Id,
            Identity = p.Identity
        }).ToList();
    }
}
