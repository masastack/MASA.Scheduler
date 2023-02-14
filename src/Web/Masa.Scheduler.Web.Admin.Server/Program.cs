// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Contrib.Configuration.ConfigurationApi.Dcc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservable(builder.Logging, builder.Configuration, true);

builder.WebHost.UseKestrel(option =>
{
    option.ConfigureHttpsDefaults(options =>
    options.ServerCertificate = new X509Certificate2(Path.Combine("Certificates", "7348307__lonsid.cn.pfx"), "cqUza0MN"));
});

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor();

var authBaseAddress = builder.Configuration["AuthServiceBaseAddress"];
var mcBaseAddress = builder.Configuration["McServiceBaseAddress"];
var schedulerBaseAddress = builder.Configuration["SchedulerServerBaseAddress"];
var signalRBaseAddress = builder.Configuration["SignalRServiceUrl"];

builder.Services.AddSchedulerApiGateways(options => options.SchedulerServerBaseAddress = schedulerBaseAddress);

builder.AddMasaStackComponentsForServer();
var publicConfiguration = builder.Services.GetMasaConfiguration().ConfigurationApi.GetPublic();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMapster();

builder.Services.AddGlobalForServer();

builder.Services.AddScoped<TokenProvider>();

builder.Services.AddMasaSignalRClient(options => options.SignalRServiceUrl = signalRBaseAddress);

var masaOpenIdConnectOptions = publicConfiguration.GetSection("$public.OIDC").Get<MasaOpenIdConnectOptions>();
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