// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Contrib.StackSdks.Config;
using Masa.Contrib.StackSdks.Tsc;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMasaStackConfig();
var masaStackConfig = builder.Services.GetMasaStackConfig();
builder.Services.AddObservable(builder.Logging, () =>
{
    return new MasaObservableOptions
    {
        ServiceNameSpace = builder.Environment.EnvironmentName,
        ServiceVersion = masaStackConfig.Version,
        ServiceName = masaStackConfig.GetServiceId("scheduler", "ui")
    };
}, () =>
{
    return masaStackConfig.OtlpUrl;
}, true);

builder.WebHost.UseKestrel(option =>
{
    option.ConfigureHttpsDefaults(options =>
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TLS_NAME")))
        {
            options.ServerCertificate = new X509Certificate2(Path.Combine("Certificates", "7348307__lonsid.cn.pfx"), "cqUza0MN");
        }
        else
        {
            options.ServerCertificate = X509Certificate2.CreateFromPemFile("./ssl/tls.crt", "./ssl/tls.key");
        }
        options.CheckCertificateRevocation = false;
    });
});

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor();

var authBaseAddress = masaStackConfig.GetAuthServiceDomain();
var mcBaseAddress = masaStackConfig.GetMcServiceDomain();
var schedulerBaseAddress = masaStackConfig.GetSchedulerServiceDomain();
var signalRBaseAddress = schedulerBaseAddress + "/server-hub/notifications";

builder.Services.AddSchedulerApiGateways(options => options.SchedulerServerBaseAddress = schedulerBaseAddress);

builder.AddMasaStackComponentsForServer("wwwroot/i18n", authBaseAddress, mcBaseAddress);

builder.Services.AddHttpContextAccessor();

builder.Services.AddMapster();

builder.Services.AddGlobalForServer();

builder.Services.AddScoped<TokenProvider>();

builder.Services.AddMasaSignalRClient(options => options.SignalRServiceUrl = signalRBaseAddress);

MasaOpenIdConnectOptions masaOpenIdConnectOptions = new MasaOpenIdConnectOptions
{
    Authority = masaStackConfig.GetSsoDomain(),
    ClientId = masaStackConfig.GetServiceId("scheduler", "ui"),
    Scopes = new List<string> { "offline_access" }
}; ;

IdentityModelEventSource.ShowPII = true;
builder.Services.AddMasaOpenIdConnect(masaOpenIdConnectOptions);
StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();