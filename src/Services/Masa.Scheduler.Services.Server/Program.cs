﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMasaStackConfigAsync(MasaStackProject.Scheduler, MasaStackApp.Service);
var masaStackConfig = builder.Services.GetMasaStackConfig();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter(opt =>
    {
        opt.AppId = masaStackConfig.GetServiceId(MasaStackProject.Scheduler);
        opt.AppIdSuffix = "";
        opt.AppPort = 19601;
    }, false);
}

builder.Services.AddObservable(builder.Logging, () =>
{
    return new MasaObservableOptions
    {
        ServiceNameSpace = builder.Environment.EnvironmentName,
        ServiceVersion = masaStackConfig.Version,
        ServiceName = masaStackConfig.GetServiceId(MasaStackProject.Scheduler),
        Layer = masaStackConfig.Namespace,
        ServiceInstanceId = builder.Configuration.GetValue<string>("HOSTNAME")!
    };
}, () =>
{
    return masaStackConfig.OtlpUrl;
});

var quartzConnectString = masaStackConfig.GetConnectionString(MasaStackProject.Scheduler.Name);
var publicConfiguration = builder.Services.GetMasaConfiguration().ConfigurationApi.GetPublic();
var identityServerUrl = masaStackConfig.GetSsoDomain();
builder.Services.AddObjectStorage(option => option.UseAliyunStorage());
builder.Services.AddMasaIdentity(options =>
{
    options.Environment = "environment";
    options.UserName = "name";
    options.UserId = "sub";
    options.Mapping(nameof(MasaUser.CurrentTeamId), IdentityClaimConsts.CURRENT_TEAM);
    options.Mapping(nameof(MasaUser.StaffId), IdentityClaimConsts.STAFF);
    options.Mapping(nameof(MasaUser.Account), IdentityClaimConsts.ACCOUNT);
});

builder.Services
.AddAuthorization()
.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.Authority = identityServerUrl;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters.ValidateAudience = false;
    options.MapInboundClaims = false;
    options.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (
            sender,
            certificate,
            chain,
            sslPolicyErrors) =>
        { return true; }
    };
});

var redisOptions = new RedisConfigurationOptions
{
    Servers = new List<RedisServerOptions> {
new RedisServerOptions()
{
Host= masaStackConfig.RedisModel.RedisHost,
Port= masaStackConfig.RedisModel.RedisPort
}
},
    DefaultDatabase = masaStackConfig.RedisModel.RedisDb,
    Password = masaStackConfig.RedisModel.RedisPassword,
    ClientName = builder.Configuration.GetValue<string>("HOSTNAME") ?? masaStackConfig.GetServiceId(MasaStackProject.Scheduler)
};
builder.Services.AddI18n(Path.Combine("Assets", "I18n"));
builder.Services.AddMultilevelCache(options => options.UseStackExchangeRedisCache(redisOptions));
builder.Services.AddSingleton(service =>
{
    var connection = StackExchange.Redis.ConnectionMultiplexer.Connect(redisOptions);
    return connection;
});
builder.Services
.AddAuthClient(masaStackConfig.GetAuthServiceDomain(), redisOptions)
.AddPmClient(masaStackConfig.GetPmServiceDomain())
.AddAlertClient(masaStackConfig.GetAlertServiceDomain());

var dbType = masaStackConfig.GetDbType();

builder.Services.AddMapster();
builder.Services.AddServerManager();
builder.Services.AddHttpClient();
builder.Services.AddMasaSignalR(redisOptions);
builder.Services.AddQuartzUtils(quartzConnectString, dbType);
builder.Services.AddSchedulerLogger();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer xxxxxxxxxxxxxxx\"",
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    })
    .AddFluentValidationAutoValidation()
    .AddDomainEventBus(options =>
    {
        options
        .UseIntegrationEventBus<IntegrationEventLogService>(options => options.UseDapr().UseEventLog<SchedulerDbContext>())
        .UseEventBus(eventBusBuilder =>
        {
            eventBusBuilder.UseMiddleware(typeof(ValidatorMiddleware<>));
        })
        .UseUoW<SchedulerDbContext>(dbOptions => dbOptions.UseDbSql(dbType).AddInterceptors(new QueryWithNoLockDbCommandInterceptor()).UseFilter(),useTransaction:false)
        .UseRepository<SchedulerDbContext>();
    });
await builder.Services.AddStackIsolationAsync(MasaStackProject.Scheduler.Name);

if (dbType == "PostgreSql")
{
    await builder.Services.MigratePostgreSqlAsync();
}
else
{
    await builder.Services.MigrateSqlServerAsync();
}

builder.Services.AddStackMiddleware();
var app = builder.AddServices(options =>
{
    options.MapHttpMethodsForUnmatched = new[] { "Post" };
});

app.UseI18n();

app.UseMasaExceptionHandler(opt =>
{
    opt.ExceptionHandler = context =>
    {
        if (context.Exception is ValidationException validationException)
        {
            context.ToResult(validationException.Errors.Select(err => err.ToString()).FirstOrDefault()!);
        }
    };
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStackIsolation();
app.UseCloudEvents();
app.UseMasaCloudEvents();
app.UseEndpoints(endpoints =>
{
    endpoints.MapSubscribeHandler();
    endpoints.MapHub<NotificationsHub>("/server-hub/notifications");
});

app.UseStackMiddleware();

app.UseHttpsRedirection();

app.Run();


