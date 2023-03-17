// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class UpsertAlarmRuleDomainEventHandler
{
    private readonly IAlertClient _alertClient;

    public UpsertAlarmRuleDomainEventHandler(IAlertClient alertClient)
    {
        _alertClient = alertClient;
    }

    [EventHandler]
    public async Task HandleEventAsync(UpsertAlarmRuleDomainEvent eto)
    {
        
    }
}
