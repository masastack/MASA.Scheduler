// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMasaStackConfigAsync();
var masaStackConfig = builder.Services.GetMasaStackConfig();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter(opt =>
    {
        opt.AppId = "masa-scheduler-service-server";
        //opt.AppIdSuffix = "";
        opt.AppPort = 19601;
        opt.DaprGrpcPort = 19501;
    }, false);
}

builder.Services.AddObservable(builder.Logging, () =>
{
    return new MasaObservableOptions
    {
        ServiceNameSpace = builder.Environment.EnvironmentName,
        ServiceVersion = masaStackConfig.Version,
        ServiceName = masaStackConfig.GetServerId(MasaStackConstant.SCHEDULER)
    };
}, () =>
{
    return masaStackConfig.OtlpUrl;
});
var connectString = "Server=.;Database=scheduler_dev;User Id=sa;Password=Tcsnwzh425;";
var quartzConnectString = connectString;
var publicConfiguration = builder.Services.GetMasaConfiguration().ConfigurationApi.GetPublic();
var identityServerUrl = masaStackConfig.GetSsoDomain();

var ossOptions = publicConfiguration.GetSection("$public.OSS").Get<OssOptions>();
builder.Services.AddObjectStorage(option => option.UseAliyunStorage(new AliyunStorageOptions(ossOptions.AccessId, ossOptions.AccessSecret, ossOptions.Endpoint, ossOptions.RoleArn, ossOptions.RoleSessionName)
{
    Sts = new AliyunStsOptions()
    {
        RegionId = ossOptions.RegionId
    }
}));

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
builder.Services.AddDistributedLock(medallionBuilder => medallionBuilder.UseRedis("127.0.0.1:6379"));

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("A healthy result."))
    .AddDbContextCheck<SchedulerDbContext>();

builder.Services.AddScoped(service =>
{
    var content = service.GetRequiredService<IHttpContextAccessor>();
    AuthenticationHeaderValue.TryParse(content.HttpContext?.Request.Headers.Authorization.ToString(), out var auth);
    return new TokenProvider { AccessToken = auth?.Parameter };
});

builder.Services
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
            dbOptions => dbOptions.UseSqlServer(connectString).UseFilter())
        .UseRepository<SchedulerDbContext>();
    });
builder.Services.AddStackMiddleware();
await builder.Services.MigrateAsync();

var app = builder.AddServices(options =>
{
    options.MapHttpMethodsForUnmatched = new string[] { "Post" };
});
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
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAddStackMiddleware();
app.UseCloudEvents();
app.UseEndpoints(endpoints =>
{
    endpoints.MapSubscribeHandler();
    endpoints.MapHub<NotificationsHub>("/server-hub/notifications");
});
app.UseHttpsRedirection();

app.Run();
