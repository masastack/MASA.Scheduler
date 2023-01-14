// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.StackSdks.Auth.Contracts.Consts;
using Masa.BuildingBlocks.StackSdks.Auth.Contracts;
using Masa.Contrib.Configuration.ConfigurationApi.Dcc.Options;
using Masa.Contrib.StackSdks.Config;
using Masa.Contrib.StackSdks.Tsc;
using Masa.Scheduler.Services.Server.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMasaStackConfig();
var masaStackConfig = builder.Services.GetMasaStackConfig();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter(opt =>
    {
        opt.AppId = "masa-scheduler-service-server";
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
        ServiceName = masaStackConfig.GetServerId("scheduler")
    };
}, () =>
{
    return masaStackConfig.OtlpUrl;
});

DccOptions dccOptions = masaStackConfig.GetDccMiniOptions<DccOptions>();
builder.Services.AddMasaConfiguration(configurationBuilder => configurationBuilder.UseDcc(dccOptions));
var quartzConnectString = masaStackConfig.GetConnectionString("scheduler_dev");
var publicConfiguration = builder.Services.GetMasaConfiguration().ConfigurationApi.GetPublic();
var identityServerUrl = masaStackConfig.GetSsoDomain();
var ossOptions = publicConfiguration.GetSection("$public.OSS").Get<OssOptions>();

builder.Services.AddAliyunStorage(new AliyunStorageOptions(ossOptions.AccessId, ossOptions.AccessSecret, ossOptions.Endpoint, ossOptions.RoleArn, ossOptions.RoleSessionName)
{
    Sts = new AliyunStsOptions()
    {
        RegionId = ossOptions.RegionId
    }
});

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
    });

var redisOptions = new RedisConfigurationOptions
{
    Servers = new List<RedisServerOptions> {
        new RedisServerOptions()
        {
            Host= masaStackConfig.RedisModel.RedisHost,
            Port=   masaStackConfig.RedisModel.RedisPort
        }
    },
    DefaultDatabase = masaStackConfig.RedisModel.RedisDb,
    Password = masaStackConfig.RedisModel.RedisPassword
};

builder.Services.AddMultilevelCache(options => options.UseStackExchangeRedisCache(redisOptions));

builder.Services
    .AddAuthClient(masaStackConfig.GetAuthServiceDomain(), redisOptions)
    .AddPmClient(masaStackConfig.GetPmServiceDomain());

builder.Services.AddMapster();
builder.Services.AddServerManager();
builder.Services.AddHttpClient();
builder.Services.AddMasaSignalR(redisOptions);
builder.Services.AddQuartzUtils(quartzConnectString);
builder.Services.AddSchedulerLogger();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("A healthy result."))
    .AddDbContextCheck<SchedulerDbContext>();

builder.Services.AddScoped(service =>
{
    var content = service.GetRequiredService<IHttpContextAccessor>();
    AuthenticationHeaderValue.TryParse(content.HttpContext?.Request.Headers.Authorization.ToString(), out var auth);
    return new TokenProvider { AccessToken = auth?.Parameter };
});

var app = builder.Services
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer xxxxxxxxxxxxxxx\"",
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
                new string[] {}
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
        .UseIsolationUoW<SchedulerDbContext>(
            isolationBuilder => isolationBuilder.UseMultiEnvironment("env_key"),
            dbOptions => dbOptions.UseSqlServer(masaStackConfig.GetConnectionString("scheduler_dev")).UseFilter())
        .UseRepository<SchedulerDbContext>();
    })
    .AddServices(builder, options=>
    {
        options.MapHttpMethodsForUnmatched = new[] { "Post" }; 
    });
await builder.MigrateDbContextAsync<SchedulerDbContext>();
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCloudEvents();
app.UseEndpoints(endpoints =>
{
    endpoints.MapSubscribeHandler();
    endpoints.MapHub<NotificationsHub>("/server-hub/notifications");
});
app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});
app.UseHttpsRedirection();

app.Run();
