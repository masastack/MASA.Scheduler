// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Shells.JobShell.Extensions;


public static class ObservabilityExtensions
{
    public static void AddObservability(this WebApplicationBuilder builder, string endPoint)
    {
        var serviceName = "masa-scheduler-job-shell";
        var serviceVersion = "1.0.0";

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
            options.AspNetCoreInstrumentationOptions.AppendDefaultFilter(options,true);

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
