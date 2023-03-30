// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);
await builder.Services.AddMasaStackConfigAsync();
var masaStackConfig = builder.Services.GetMasaStackConfig();
builder.Services.AddObservable(builder.Logging, () =>
{
    return new MasaObservableOptions
    {
        ServiceNameSpace = builder.Environment.EnvironmentName,
        ServiceVersion = masaStackConfig.Version,
        ServiceName = masaStackConfig.GetWebId(MasaStackConstant.SCHEDULER),
        Layer = masaStackConfig.Namespace,
        ServiceInstanceId = builder.Configuration.GetValue<string>("HOSTNAME")
    };
}, () =>
{
    return masaStackConfig.OtlpUrl;
}, true);

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseKestrel(option =>
    {
        option.ConfigureHttpsDefaults(options =>
        {
            options.ServerCertificate = X509Certificate2.CreateFromPemFile("./ssl/tls.crt", "./ssl/tls.key");
            options.CheckCertificateRevocation = false;
        });
    });
}

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor();

var authBaseAddress = masaStackConfig.GetAuthServiceDomain();
var mcBaseAddress = masaStackConfig.GetMcServiceDomain();
var schedulerBaseAddress = masaStackConfig.GetSchedulerServiceDomain();

#if DEBUG
schedulerBaseAddress = "https://localhost:19611";
#endif

var signalRBaseAddress = schedulerBaseAddress + "/server-hub/notifications";

builder.AddMasaStackComponentsForServer("wwwroot/i18n", authBaseAddress, mcBaseAddress);
builder.Services.AddTscClient(masaStackConfig.GetTscServiceDomain()).AddAlertClient(masaStackConfig.GetAlertServiceDomain());
builder.Services.AddHttpContextAccessor();

builder.Services.AddMapster();

builder.Services.AddGlobalForServer();

builder.Services.AddScoped<TokenProvider>();

builder.Services.AddMasaSignalRClient(options => options.SignalRServiceUrl = signalRBaseAddress);

MasaOpenIdConnectOptions masaOpenIdConnectOptions = new MasaOpenIdConnectOptions
{
    Authority = masaStackConfig.GetSsoDomain(),
    ClientId = masaStackConfig.GetWebId(MasaStackConstant.SCHEDULER),
    Scopes = new List<string> { "offline_access" }
}; ;

IdentityModelEventSource.ShowPII = true;
builder.Services.AddMasaOpenIdConnect(masaOpenIdConnectOptions);
StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services.AddSchedulerApiGateways(options =>
{
    options.SchedulerServerBaseAddress = schedulerBaseAddress;
    options.AuthorityEndpoint = masaOpenIdConnectOptions.Authority;
    options.ClientId = masaOpenIdConnectOptions.ClientId;
    options.ClientSecret = masaOpenIdConnectOptions.ClientSecret;
});

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