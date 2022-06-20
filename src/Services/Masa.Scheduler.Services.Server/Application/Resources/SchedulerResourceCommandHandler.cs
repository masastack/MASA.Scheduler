// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Resources;

public class SchedulerResourceCommandHandler
{
    private IRepository<SchedulerResource> _repository;
    private IMapper _mapper;

    public SchedulerResourceCommandHandler(IRepository<SchedulerResource> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [EventHandler]
    public async Task AddHandleAsync(AddSchedulerResourceCommand command)
    {
        var resource = _mapper.Map<SchedulerResource>(command.Request.Data);

        await _repository.AddAsync(resource);
    }

    [EventHandler]
    public async Task UpdateHandleAsync(UpdateSchedulerResourceCommand command)
    {
        var resourceDto = command.Request.Data;

        var resource = await _repository.FindAsync(r => r.Id == resourceDto.Id);

        if (resource is null)
        {
            throw new UserFriendlyException("Current resource does not exist");
        }

        resource.UpdateResouce(resourceDto.Name, resourceDto.Description, resourceDto.Version, resourceDto.FilePath);

        await _repository.UpdateAsync(resource);
    }

    [EventHandler]
    public async Task RemoveHandleAsync(RemoveSchedulerResourceCommand command)
    {
        var resource = await _repository.FindAsync(r => r.Id == command.ResourceId);

        if (resource is null)
        {
            throw new UserFriendlyException($"Resouce id {command.ResourceId}, not found");
        }

        await _repository.RemoveAsync(resource);
    }
}
