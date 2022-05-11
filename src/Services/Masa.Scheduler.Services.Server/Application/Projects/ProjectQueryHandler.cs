// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.


namespace Masa.Scheduler.Services.Server.Application.Projects;

public class ProjectQueryHandler
{
    [EventHandler]
    public Task TeamListHandleAsync(ProjectQuery query)
    {
        var projectList = new List<ProjectModel>()
        {
            new ProjectModel()
            {
                Id = 1,
                Name = "Masa Auth",
                Description = "Masa Auth",
                TeamId = new Guid("22A8CA67-F5C2-44A7-2129-08DA1DE0EBC7"),
            },
            new ProjectModel()
            {
                Id = 2,
                Name = "Masa Project",
                Description = "Masa Project",
                TeamId = new Guid("22A8CA67-F5C2-44A7-2129-08DA1DE0EBC7"),
            },
            new ProjectModel()
            {
                Id= 3,
                Name = "IoT",
                Description ="IoT",
                TeamId = new Guid("D0643F37-8202-45FF-D53C-08DA1D3F3845"),
            },
            new ProjectModel()
            {
                Id= 4,
                Name = "SEC",
                Description ="SEC",
                TeamId = new Guid("3119137C-DB47-4523-C509-08DA1EC03F6F"),
            },
        };

        query.Result = projectList.FindAll(p => p.TeamId == query.TeamId);

        return Task.CompletedTask;

    }
}
