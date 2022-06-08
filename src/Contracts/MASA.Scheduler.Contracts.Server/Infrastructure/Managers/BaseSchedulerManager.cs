// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Managers;

public abstract class BaseSchedulerManager<T, TOnlineEvent, TMonitorEvent> where T : BaseServiceModel, new() where TOnlineEvent : OnlineIntegrationEvent, new() where TMonitorEvent : OnlineIntegrationEvent, new()
{
    private readonly IDistributedCacheClientFactory _cacheClientFactory;
    private readonly IDistributedCacheClient _redisCacheClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly IIntegrationEventBus _eventBus;
    protected readonly IHttpClientFactory _httpClientFactory;
    private readonly BaseSchedulerManagerData<T> _data;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public BaseSchedulerManager(
        IDistributedCacheClientFactory cacheClientFactory,
        IDistributedCacheClient redisCacheClient,
        IServiceProvider serviceProvider,
        IIntegrationEventBus eventBus,
        IHttpClientFactory httpClientFactory,
        BaseSchedulerManagerData<T> data, IHostApplicationLifetime hostApplicationLifetime)
    {
        _cacheClientFactory = cacheClientFactory;
        _redisCacheClient = redisCacheClient;
        _serviceProvider = serviceProvider;
        _eventBus = eventBus;
        _httpClientFactory = httpClientFactory;
        _data = data;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected IIntegrationEventBus EventBus => _eventBus;

    protected IServiceProvider ServiceProvider => _serviceProvider;

    protected IDistributedCacheClient RedisCacheClient => _redisCacheClient;

    protected IDistributedCacheClientFactory CacheClientFactory => _cacheClientFactory;

    public List<T> ServiceList => _data.ServiceList;

    protected abstract string HeartbeatApi { get; set; }

    protected abstract ILogger<BaseSchedulerManager<T, TOnlineEvent, TMonitorEvent>> Logger { get; }

    public virtual async Task StartManagerAsync(CancellationToken stoppingToken)
    {
        if(! await WaitForAppStartup(_hostApplicationLifetime, stoppingToken))
        {
            return;
        }

        var _server = ServiceProvider.GetService<IServer>()!;

        var addressFeature = _server.Features.Get<IServerAddressesFeature>()!;

        if (addressFeature.Addresses.Any())
        {
            _data.AddressList = addressFeature.Addresses.ToList();
            await OnManagerStartAsync();
        }
    }

    private Task RegisterHeartbeat()
    {
        Task.Run(async () =>
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
        var checkList = _data.ServiceList.FindAll(p => p.Status != ServiceStatus.Stopped);

        if(!checkList.Any())
        {
            return;
        }

        foreach (var item in checkList)
        {
            await CheckHeartbeat(item);
        }

        _data.ServiceList.RemoveAll(p => p.Status == ServiceStatus.Stopped);
    }

    protected async Task CheckHeartbeat(T item)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            _ = await client.GetAsync(item.GetServiceUrl() + item.HeartbeatApi);
            item.Status = ServiceStatus.Normal;
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
                item.Status = ServiceStatus.Stopped;
            }
            else
            {
                item.Status = ServiceStatus.Error;
            }
        }
    }

    public virtual async Task OnManagerStartAsync()
    {
        await Online(false);
        await RegisterHeartbeat();
    }

    public async virtual Task Online(bool isResponse = false)
    {
        if (!_data.AddressList.Any())
        {
            return;
        }

        var httpAddress = _data.AddressList.FirstOrDefault(address => address.StartsWith(Uri.UriSchemeHttp + Uri.SchemeDelimiter));

        var httpsAddress = _data.AddressList.FirstOrDefault(address => address.StartsWith(Uri.UriSchemeHttps + Uri.SchemeDelimiter));

        var @event = new TOnlineEvent();

        var service = new T()
        {
            HttpHost = await GetAddresssHost(httpAddress),
            HttpsHost = await GetAddresssHost(httpsAddress),
            HttpPort = GetAddressPort(httpAddress),
            HttpsPort = GetAddressPort(httpsAddress),
            HeartbeatApi = HeartbeatApi,
            Status = ServiceStatus.Normal
        };

        _data.ServiceId = MD5Utils.Encrypt(EncryptType.Md5, service.GetServiceUrl());
        service.ServiceId = _data.ServiceId;

        @event.IsPong = isResponse;
        @event.OnlineService = service;

        await _eventBus.PublishAsync(@event);

        await _eventBus.CommitAsync();
    }

    public async virtual Task MonitorHandler(TMonitorEvent @event)
    {
        if (@event.OnlineService is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(@event.OnlineService.HttpHost) || (@event.OnlineService.HttpPort == 0 && @event.OnlineService.HttpsPort == 0))
        {
            return;
        }

        var service = ServiceList.FirstOrDefault(w => w.HttpHost == @event.OnlineService.HttpHost && ((w.HttpPort != 0 && w.HttpPort == @event.OnlineService.HttpPort) || (w.HttpsPort != 0 && w.HttpsPort == @event.OnlineService.HttpsPort)));

        if (service == null)
        {
            var model = new T()
            {
                HttpHost = @event.OnlineService.HttpHost,
                HttpsHost = @event.OnlineService.HttpsHost,
                HttpPort = @event.OnlineService.HttpPort,
                HttpsPort = @event.OnlineService.HttpsPort,
                Status = ServiceStatus.Normal,
                HeartbeatApi = @event.OnlineService.HeartbeatApi,
                ServiceId = @event.OnlineService.ServiceId,
            };

            _data.ServiceList.Add(model);
        }
        else
        {
            service.HttpPort = @event.OnlineService.HttpPort;
            service.HttpsPort = @event.OnlineService.HttpsPort;
            service.HttpHost = @event.OnlineService.HttpHost;
            service.HttpsHost = @event.OnlineService.HttpsHost;
            service.Status = ServiceStatus.Normal;
            service.HeartbeatApi = @event.OnlineService.HeartbeatApi;
            service.ServiceId = MD5Utils.Encrypt(EncryptType.Md5, service.GetServiceUrl());
        }

        if (!@event.IsPong)
        {
            await Online(true);
        }
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

    private async Task<string> GetAddresssHost(string? address)
    {
        if (!string.IsNullOrEmpty(address))
        {
            var uri = new Uri(address);

            var currentIp = await GetCurrentIp();

            var isSuccess = await TryRequestCurrentIp(uri.Scheme, currentIp, uri.Port);

            if (isSuccess)
            {
                return currentIp;
            }

            return uri.Host;
        }

        return string.Empty;
    }

    /// <summary>
    /// Use local IP first, if try request is normal
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    private async Task<bool> TryRequestCurrentIp(string scheme, string? currentIp, int port)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(currentIp))
            {
                var client = _httpClientFactory.CreateClient();

                var requestUrl = $"{scheme}://{currentIp}:{port}{HeartbeatApi}";

                var response = await client.GetAsync(requestUrl);

                return response.IsSuccessStatusCode;
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "tryRequestCurrentIp error");
        }

        return false;
    }

    static async Task<bool> WaitForAppStartup(IHostApplicationLifetime hostApplicationLifetime, CancellationToken stoppingToken)
    {
        var startedSource = new TaskCompletionSource();
        var cancelledSource = new TaskCompletionSource();

        await using var startedCancellationTokenRegistration =
            hostApplicationLifetime.ApplicationStarted.Register(() => startedSource.SetResult());
        await using var cancellationTokenRegistration = stoppingToken.Register(() => cancelledSource.SetResult());

        Task completedTask = await Task.WhenAny(startedSource.Task, cancelledSource.Task).ConfigureAwait(false);

        return completedTask == startedSource.Task;
    }
}
