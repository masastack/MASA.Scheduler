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
        await ValidateVersionAsync(command.Request.Data.JobAppIdentity, command.Request.Data.Version, null);

        var resource = _mapper.Map<SchedulerResource>(command.Request.Data);

        await _repository.AddAsync(resource);
    }

    [EventHandler]
    public async Task UpdateHandleAsync(UpdateSchedulerResourceCommand command)
    {
        var resourceDto = command.Request.Data;

        await ValidateVersionAsync(resourceDto.JobAppIdentity, resourceDto.Version, resourceDto.Id);

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
        var request = command.request;

        if (request.ResourceId == Guid.Empty && string.IsNullOrEmpty(request.JobAppIdentity))
        {
            return;
        }

        Expression<Func<SchedulerResource, bool>> condition;

        if (request.ResourceId != Guid.Empty)
        {
            condition = r => r.Id == request.ResourceId;
        }
        else
        {
            condition = r => r.JobAppIdentity == request.JobAppIdentity;
        }

        var resourceList = await _repository.GetListAsync(condition);

        if (!resourceList.Any())
        {
            throw new UserFriendlyException($"Resouce not found, id: {request.ResourceId}, jobAppIdentity: {request.JobAppIdentity}");
        }

        await _repository.RemoveRangeAsync(resourceList);
    }

    private async Task ValidateVersionAsync(string jobAppIdentity, string version, Guid? id)
    {
        if (await _repository.FindAsync(x => x.JobAppIdentity == jobAppIdentity && x.Version == version && x.Id != id) != null)
        {
            throw new UserFriendlyException(errorCode: UserFriendlyExceptionCodes.VERSION_CANNOT_BE_DUPLICATE);
        }
    }
}
