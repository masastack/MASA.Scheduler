// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter(opt =>
    {
        opt.AppId = "masa-scheduler-service-server";
        opt.AppIdSuffix = "";
        opt.DaprHttpPort = 10602;
        opt.DaprGrpcPort = 10601;
    });
}

builder.AddObservability();

builder.Services.AddMasaIdentityModel(IdentityType.MultiEnvironment, options =>
{
    options.Environment = "environment";
    options.UserName = "name";
    options.UserId = "sub";
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.Authority = builder.GetMasaConfiguration().ConfigurationApi.GetDefault().GetValue<string>("AppSettings:IdentityServerUrl");
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters.ValidateAudience = false;
    options.MapInboundClaims = false;
});
builder.AddMasaConfiguration(configurationBuilder =>
{
    configurationBuilder.UseDcc();
});
var configuration = builder.GetMasaConfiguration().ConfigurationApi.GetDefault();
builder.Services.AddAuthClient(configuration.GetValue<string>("AppSettings:AuthClient:Url"));
builder.Services.AddMasaRedisCache(configuration.GetSection("RedisConfig").Get<RedisConfigurationOptions>()).AddMasaMemoryCache();
builder.Services.AddPmClient(configuration.GetValue<string>("AppSettings:PmClient:Url"));
builder.Services.AddMapster();
builder.Services.AddServerManager();
builder.Services.AddHttpClient();
builder.Services.AddMasaSignalR();
builder.Services.AddQuartzUtils();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("A healthy result."))
    .AddDbContextCheck<SchedulerDbContext>();

builder.Services.AddAliyunStorage(async serviceProvider =>
{
    var daprClient = serviceProvider.GetRequiredService<DaprClient>();

    var secrets = await daprClient.GetSecretAsync("localsecretstore", "masa-scheduler-secret");
    var accessId = secrets.GetValueOrDefault("access_id", string.Empty);
    var accessSecret = secrets.GetValueOrDefault("access_secret", string.Empty);
    var endpoint = secrets.GetValueOrDefault("endpoint", string.Empty);
    var roleArn = secrets.GetValueOrDefault("roleArn", string.Empty);

    return new AliyunStorageOptions(accessId, accessSecret, endpoint, roleArn, "SessionTest")
    {
        Sts = new AliyunStsOptions()
        {
            RegionId = "cn-hangzhou"
        }
    };
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
    .AddFluentValidation(options =>
    {
        options.RegisterValidatorsFromAssemblyContaining<Program>();
    })
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
            dbOptions => dbOptions.UseSqlServer().UseFilter())
        .UseRepository<SchedulerDbContext>();
    })
    .AddServices(builder);

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
app.UseHttpsRedirection();

app.Run();
