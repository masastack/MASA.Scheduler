// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Scheduler.ApiGateways.Caller;
using Masa.Scheduler.Contracts.Server.Infrastructure.Extensions;
using Masa.Scheduler.Contracts.Server.Infrastructure.SignalRClients;
using Masa.Stack.Components;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using System.Security.Cryptography.X509Certificates;
using Masa.Utils.Data.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;

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
builder.Services.AddSchedulerApiGateways(options => options.SchedulerServerBaseAddress = builder.Configuration["SchedulerServerBaseAddress"]);
builder.Services.AddMasaStackComponentsForServer("wwwroot/i18n", builder.Configuration["AuthServiceBaseAddress"], builder.Configuration["McServiceBaseAddress"]);
builder.Services.AddElasticsearchClient("auth", option => option.UseNodes("http://10.10.90.44:31920/").UseDefault())
                   .AddAutoComplete(option => option.UseIndexName(builder.Configuration["UserAutoComplete"]));
builder.Services.AddHttpContextAccessor();
builder.Services.AddMapster();
builder.Services.AddGlobalForServer();
builder.Services.AddMasaSignalRClient(options=> options.SignalRServiceUrl = builder.Configuration["SignalRServiceUrl"]);
builder.Services.AddMasaOpenIdConnect(builder.Configuration);
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