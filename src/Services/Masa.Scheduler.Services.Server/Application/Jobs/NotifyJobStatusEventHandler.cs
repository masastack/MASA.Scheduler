// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class NotifyJobStatusEventHandler
{
    private IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotifyJobStatusEventHandler> _logger;

    public NotifyJobStatusEventHandler(IHttpClientFactory httpClientFactor, ILogger<NotifyJobStatusEventHandler> logger)
    {
        _httpClientFactory = httpClientFactor;
        _logger = logger;
    }

    [EventHandler]
    public async Task HandleEventAsync(NotifyJobStatusDomainEvent eto)
    {
        var client = _httpClientFactory.CreateClient();
        var input = new StringContent(JsonSerializer.Serialize(new { eto.JobId, eto.Status }), Encoding.UTF8, "application/json");

        try
        {
            using var response = await client.PostAsync(eto.NotifyUrl, input);

            if (!response.IsSuccessStatusCode)
            {
                throw new UserFriendlyException(response.StatusCode.ToString());
            }
            var content = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notify job run result fail");
            throw new UserFriendlyException(ex.Message);
        }
    }
}
