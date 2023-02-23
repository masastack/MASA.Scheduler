// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(option =>
{
    option.ConfigureHttpsDefaults(options =>
    options.ServerCertificate = new X509Certificate2(Path.Combine("Certificates", "7348307__lonsid.cn.pfx"), "cqUza0MN"));
});

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor();

builder.AddMasaStackComponentsForServer();

var publicConfiguration = builder.Services.GetMasaConfiguration().ConfigurationApi.GetPublic();
var authBaseAddress = publicConfiguration.GetValue<string>("$public.AppSettings:AuthClient:Url");
var mcBaseAddress = publicConfiguration.GetValue<string>("$public.AppSettings:McClient:Url");
var schedulerBaseAddress = publicConfiguration.GetValue<string>("$public.AppSettings:SchedulerClient:Url");
var signalRBaseAddress = schedulerBaseAddress + "/server-hub/notifications";

builder.Services.AddSchedulerApiGateways(options => options.SchedulerServerBaseAddress = schedulerBaseAddress);
builder.Services.AddObservable(builder.Logging, () =>
{
    return new MasaObservableOptions
    {
        ServiceNameSpace = builder.Environment.EnvironmentName,
        ServiceVersion = "1.0.0",
        ServiceName = "masa-scheduler-web-admin"
    };
}, () =>
{
    return publicConfiguration.GetValue<string>("$public.AppSettings:OtlpUrl");
}, true);

builder.Services.AddHttpContextAccessor();

builder.Services.AddMapster();

builder.Services.AddGlobalForServer();

builder.Services.AddScoped<TokenProvider>();

builder.Services.AddMasaSignalRClient(options => options.SignalRServiceUrl = signalRBaseAddress);

builder.Services.AddMasaOpenIdConnect(publicConfiguration.GetSection("$public.OIDC").Get<MasaOpenIdConnectOptions>());

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