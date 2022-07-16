﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Extensions;

public static class ObservabilityExtensions
{
    public static void AddObservability(this WebApplicationBuilder builder, bool isBlazor = false)
    {
        var endPoint = builder.Configuration.GetValue<string>("OTLP:Endpoint");
        var serviceName = builder.Configuration.GetValue<string>("OTLP:ServiceName");
        var serviceVersion = builder.Configuration.GetValue<string>("OTLP:ServiceVersion");

        var resources = ResourceBuilder.CreateDefault();

        resources.AddMasaService(new MasaObservableOptions
        {
            ServiceName = serviceName,
            ServiceVersion = serviceVersion
        });

        //metrics
        builder.Services.AddMasaMetrics(builder =>
        {
            builder.SetResourceBuilder(resources);
            builder.AddOtlpExporter(
                    option =>
                    {
                        option.Endpoint = new Uri(endPoint);
                    });
        });

        //tracing
        builder.Services.AddMasaTracing(options =>
        {
            if (isBlazor)
            {
                options.AspNetCoreInstrumentationOptions.AppendBlazorFilter(options);
            }
            else
            {
                options.AspNetCoreInstrumentationOptions.AppendDefaultFilter(options);
            }
           
            options.BuildTraceCallback = builder =>
            {
                builder.SetResourceBuilder(resources);
                builder.AddOtlpExporter(
                    option =>
                    {
                        option.Endpoint = new Uri(endPoint);
                    });
            };
        });

        //logging
        builder.Logging.AddMasaOpenTelemetry(builder =>
        {
            builder.SetResourceBuilder(resources);
            builder.IncludeScopes = true;
            builder.IncludeFormattedMessage = true;
            builder.ParseStateValues = true;
            builder.AddOtlpExporter(
               option =>
               {
                   option.Endpoint = new Uri(endPoint);
               });
        });
    }
}
