// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.SignalRClients;

public class MasaSignalRClient : IDisposable
{
    private string _defaultSignalRServiceUrl = string.Empty;

    public MasaSignalRClient(MasaSignalROptions options)
    {
        _defaultSignalRServiceUrl = options.SignalRServiceUrl;
    }

    public HubConnection? HubConnection { get; set; }

    public void Dispose()
    {
        HubConnection?.DisposeAsync();
    }

    public async Task HubConnectionBuilder(string? signalRServiceUrl = null)
    {
        HubConnection = new HubConnectionBuilder()
            .WithUrl(string.IsNullOrEmpty(signalRServiceUrl) ? _defaultSignalRServiceUrl : signalRServiceUrl)
            .Build();
        await HubConnection.StartAsync();
    }
}

