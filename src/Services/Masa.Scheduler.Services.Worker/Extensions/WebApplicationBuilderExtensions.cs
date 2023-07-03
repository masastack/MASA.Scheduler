// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationBuilderExtensions
{
    public static void AddObservable(this WebApplicationBuilder builder, IMasaStackConfig masaStackConfig)
    {
        builder.Services.AddAutoInject();
        MasaApp.TrySetServiceCollection(builder.Services);

        var option = new MasaObservableOptions
        {
            ServiceNameSpace = builder.Environment.EnvironmentName,
            ServiceVersion = masaStackConfig.Version,
            ServiceName = masaStackConfig.GetId(MasaStackProject.Scheduler, MasaStackApp.Worker)
        };
        var resources = ResourceBuilder.CreateDefault().AddMasaService(option);

        Uri? uri = null;
        if (!string.IsNullOrEmpty(masaStackConfig.OtlpUrl) && !Uri.TryCreate(masaStackConfig.OtlpUrl, UriKind.Absolute, out uri))
            throw new UriFormatException($"{nameof(masaStackConfig.OtlpUrl)}:{masaStackConfig.OtlpUrl} is invalid url");

        builder.Logging.AddMasaOpenTelemetry(builder =>
        {
            builder.SetResourceBuilder(resources);
            builder.AddOtlpExporter(options => options.Endpoint = uri);
        });

        builder.Services.AddMasaMetrics(builder =>
        {
            builder.SetResourceBuilder(resources);
            builder.AddOtlpExporter(options => options.Endpoint = uri);
        });

        using var serviceProvider = builder.Services.BuildServiceProvider();

        var httpClientInterceptors = serviceProvider.GetRequiredService<IEnumerable<IHttpClientTracingInterceptor>>();

        builder.Services.AddMasaTracing(builder =>
        {
            builder.HttpClientInstrumentationOptions += delegate (HttpClientInstrumentationOptions options) {
                foreach (var item in httpClientInterceptors)
                {
                    options.EnrichWithHttpResponseMessage += item.OnHttpResponseMessage;
                    options.EnrichWithHttpRequestMessage += item.OnHttpRequestMessage;
                }
            };

            builder.AspNetCoreInstrumentationOptions.AppendDefaultFilter(builder, false);
            builder.BuildTraceCallback = options =>
            {
                options.SetResourceBuilder(resources);
                options.AddOtlpExporter(options => options.Endpoint = uri);
            };
        });
    }
}
