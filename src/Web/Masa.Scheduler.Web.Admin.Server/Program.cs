// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability(true);

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

builder.AddMasaStackComponentsForServer("wwwroot/i18n", authBaseAddress, mcBaseAddress);

builder.Services.AddHttpContextAccessor();

builder.Services.AddMapster();

builder.Services.AddGlobalForServer();

builder.Services.AddScoped<TokenProvider>();

builder.Services.AddMasaSignalRClient(options => options.SignalRServiceUrl = signalRBaseAddress);

var oidcOptions = builder.Services.GetMasaConfiguration().Local.GetSection("$public.OIDC:AuthClient").Get<MasaOpenIdConnectOptions>();

builder.Services.AddMasaOpenIdConnect(oidcOptions);

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