// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Resources;

public class SchedulerResourceQueryHandler
{
    private IRepository<SchedulerResource> _repository;
    private IMapper _mapper;

    public SchedulerResourceQueryHandler(IRepository<SchedulerResource> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [EventHandler]
    public async Task SchedulerResourceListQueryHandle(SchedulerResourceQuery query)
    {
        var request = query.Request;

        var list = await _repository.GetListAsync(p => p.JobAppId == request.JobAppId, "CreationTime", true);

        var dtoList = _mapper.Map<List<SchedulerResourceDto>>(list);

        query.Result = new SchedulerResourceListResponse() { Data = dtoList };
    }
}
