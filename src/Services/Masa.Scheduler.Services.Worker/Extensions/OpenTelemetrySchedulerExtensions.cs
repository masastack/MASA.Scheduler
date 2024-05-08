// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenTelemetrySchedulerExtensions
{
    public static IServiceCollection AddSchedulerObservable(this IServiceCollection services,
       ILoggingBuilder loggingBuilder,
       Func<MasaObservableOptions> optionsConfigure,
       Func<string>? otlpUrlConfigure = null)
    {
        var options = optionsConfigure();
        var otlpUrl = otlpUrlConfigure?.Invoke() ?? string.Empty;

        ArgumentNullException.ThrowIfNull(options);

        Uri? uri = null;
        if (!string.IsNullOrEmpty(otlpUrl) && !Uri.TryCreate(otlpUrl, UriKind.Absolute, out uri))
            throw new UriFormatException($"{nameof(otlpUrl)}:{otlpUrl} is invalid url");

        var resources = ResourceBuilder.CreateDefault().AddMasaService(options);
        loggingBuilder.AddMasaOpenTelemetry(builder =>
        {
            builder.SetResourceBuilder(resources);
            builder.AddOtlpExporter(otlp => otlp.Endpoint = uri);
        });

        services.AddMasaMetrics(builder =>
        {
            builder.SetResourceBuilder(resources);
            builder.AddOtlpExporter(options =>
            {
                options.Endpoint = uri;
            });
        });

        services.AddMasaTracing(
            builder =>
            {
                builder.SetResourceBuilder(resources);
                builder.AddOtlpExporter(options => options.Endpoint = uri);
                builder.AddSource("Masa.Scheduler.Background");
            },
            builder =>
            {
                builder.AspNetCoreInstrumentationOptions.AppendDefaultFilter(builder, false);
                builder.AspNetCoreInstrumentationOptions.AppendSchedulerFilter(builder);
            });

        return services;
    }

    public static void AppendSchedulerFilter(this Action<AspNetCoreInstrumentationOptions> options,
        OpenTelemetryInstrumentationOptions openTelemetryInstrumentationOptions)
    {
        options += opt =>
        {
            opt.Filter = (HttpContext httpContext) =>
            {
                var url = httpContext.Request.Path.Value ?? "/";
                return !(url == "/" || url.Contains("/heartbeat"));
            };
        };
        openTelemetryInstrumentationOptions.AspNetCoreInstrumentationOptions += options;
    }
}