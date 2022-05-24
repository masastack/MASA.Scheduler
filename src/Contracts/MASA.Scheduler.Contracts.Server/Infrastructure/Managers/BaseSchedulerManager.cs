// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Managers;

public abstract class BaseSchedulerManager<T, TOnlineEvent, TMonitorEvent> where T : BaseServiceModel, new() where TOnlineEvent : OnlineIntegrationEvent, new() where TMonitorEvent : OnlineIntegrationEvent, new()
{
    private readonly static List<T> serviceList = new();
    private readonly IDistributedCacheClientFactory _cacheClientFactory;
    private readonly IDistributedCacheClient _redisCacheClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly IIntegrationEventBus _eventBus;
    private static List<string> _addressList = new();
    protected readonly IHttpClientFactory _httpClientFactory;

    public BaseSchedulerManager(IDistributedCacheClientFactory cacheClientFactory, IDistributedCacheClient redisCacheClient, IServiceProvider serviceProvider, IIntegrationEventBus eventBus, IHttpClientFactory httpClientFactory)
    {
        _cacheClientFactory = cacheClientFactory;
        _redisCacheClient = redisCacheClient;
        _serviceProvider = serviceProvider;
        _eventBus = eventBus;
        _httpClientFactory = httpClientFactory;
    }

    public static List<string> AddressList => _addressList;

    protected IIntegrationEventBus EventBus => _eventBus;

    protected IServiceProvider ServiceProvider => _serviceProvider;

    protected IDistributedCacheClient RedisCacheClient => _redisCacheClient;

    protected IDistributedCacheClientFactory CacheClientFactory => _cacheClientFactory;

    public static List<T> ServiceList => serviceList;

    protected abstract string HeartbeatApi { get; set; }

    protected abstract ILogger<BaseSchedulerManager<T, TOnlineEvent, TMonitorEvent>> Logger { get; }

    public virtual async Task StartManagerAsync()
    {
        var _server = ServiceProvider.GetService<IServer>()!;

        var addressFeature = _server.Features.Get<IServerAddressesFeature>()!;

        while (true)
        {
            if (addressFeature.Addresses.Any())
            {
                _addressList = addressFeature.Addresses.ToList();

                await OnManagerStartAsync();

                await RegisterHeartbeat();

                return;
            }

            await Task.Delay(500);
        }
    }

    private Task RegisterHeartbeat()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Heartbeat();
                await Task.Delay(10000);
            }
        });

        return Task.CompletedTask;
    }

    private async Task Heartbeat()
    {
        var checkList = serviceList.FindAll(p => p.Status != ServiceStatuses.Stopped);

        if(!checkList.Any())
        {
            return;
        }

        var client = _httpClientFactory.CreateClient();

        foreach (var item in checkList)
        {
            try
            {
                _ = await client.GetAsync(item.GetServiceUrl() + item.HeartbeatApi);
                item.Status = ServiceStatuses.Normal;
                item.NotResponseCount = 0;
                item.LastResponseTime = DateTimeOffset.Now;
                Logger.LogInformation($"Heartbeat request success, {item.GetServiceUrl()}");
            }
            catch (Exception ex)
            {
                var message = $"Heartbeat request error, ServiceUrl: {item.GetServiceUrl()}";
                Logger.LogError(ex, message);

                item.NotResponseCount++;

                if (item.NotResponseCount >= 3)
                {
                    item.Status = ServiceStatuses.Stopped;
                }
                else
                {
                    item.Status = ServiceStatuses.Error;
                }
            }
        }

        serviceList.RemoveAll(p => p.Status == ServiceStatuses.Stopped);
    }

    public virtual async Task OnManagerStartAsync()
    {
        await Online(false);
    }

    public async virtual Task Online(bool isResponse = false)
    {
        if (!_addressList.Any())
        {
            return;
        }

        var httpAddress = _addressList.FirstOrDefault(address => address.StartsWith("http://"));

        var httpsAddress = _addressList.FirstOrDefault(address => address.StartsWith("https://"));

        var @event = new TOnlineEvent();

        @event.HttpPort = GetAddressPort(httpAddress);

        @event.HttpsPort = GetAddressPort(httpsAddress);

        @event.HttpHost = GetAddresssHost(httpAddress);

        @event.HttpsHost = GetAddresssHost(httpsAddress);

        @event.IsResponse = isResponse;

        @event.HeartbeatApi = HeartbeatApi;

        await _eventBus.PublishAsync(@event);

        await _eventBus.CommitAsync();
    }

    public async virtual Task MonitorHandler(TMonitorEvent @event)
    {
        if (string.IsNullOrEmpty(@event.HttpHost) || (@event.HttpPort == 0 && @event.HttpsPort == 0))
        {
            return;
        }

        var service = ServiceList.FirstOrDefault(w => w.HttpHost == @event.HttpHost && (w.HttpPort == @event.HttpPort || w.HttpsPort == @event.HttpsPort));

        if (service == null)
        {
            var model = new T()
            {
                HttpHost = @event.HttpHost,
                HttpsHost = @event.HttpsHost,
                HttpPort = @event.HttpPort,
                HttpsPort = @event.HttpsPort,
                Status = ServiceStatuses.Normal,
                HeartbeatApi = @event.HeartbeatApi,
            };
            
            model.CallerClient = CreateClient(model);

            serviceList.Add(model);
        }
        else
        {
            service.HttpPort = @event.HttpPort;
            service.HttpsPort = @event.HttpsPort;
            service.HttpHost = @event.HttpHost;
            service.HttpsHost = @event.HttpsHost;
            service.Status = ServiceStatuses.Normal;
            service.HeartbeatApi = @event.HeartbeatApi;
            service.CallerClient = CreateClient(service);
        }

        if (!@event.IsResponse)
        {
            await Online(true);
        }
    }

    private HttpClient CreateClient(T model)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(model.GetServiceUrl());
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    public virtual Task<IIntegrationEventBus> GetIntegrationEventBus()
    {
        return Task.FromResult(_serviceProvider.GetRequiredService<IIntegrationEventBus>());
    }

    private static async Task<string> GetCurrentIp()
    {
        var hostName = System.Net.Dns.GetHostName();

        var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);

        foreach (var ip in ips)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return string.Empty;
    }

    private int GetAddressPort(string? address)
    {
        if (!string.IsNullOrEmpty(address))
        {
            var uri = new Uri(address);

            return uri.Port;
        }

        return 0;
    }

    private string GetAddresssHost(string? address)
    {
        if (!string.IsNullOrEmpty(address))
        {
            var uri = new Uri(address);
            return uri.Host;
        }

        return string.Empty;
    }
}

