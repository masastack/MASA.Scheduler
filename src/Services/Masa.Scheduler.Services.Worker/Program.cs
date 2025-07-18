// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMasaStackConfigAsync(MasaStackProject.Scheduler, MasaStackApp.Worker);
var masaStackConfig = builder.Services.GetMasaStackConfig();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter(opt =>
    {
        opt.AppId = masaStackConfig.GetId(MasaStackProject.Scheduler, MasaStackApp.Worker);
        opt.AppIdSuffix = "";
        opt.AppPort = 19602;
    }, false);
}

var publicConfiguration = builder.Services.GetMasaConfiguration().ConfigurationApi.GetPublic();
var identityServerUrl = masaStackConfig.GetSsoDomain();
builder.Services.AddDaprClient();

builder.Services.AddObservable(builder.Logging, () =>
{
    return new MasaObservableOptions
    {
        ServiceNameSpace = builder.Environment.EnvironmentName,
        ServiceVersion = masaStackConfig.Version,
        ServiceName = masaStackConfig.GetId(MasaStackProject.Scheduler, MasaStackApp.Worker),
        Layer = masaStackConfig.Namespace,
        ServiceInstanceId = builder.Configuration.GetValue<string>("HOSTNAME")!
    };
}, () =>
{
    return masaStackConfig.OtlpUrl;
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
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
    Password = masaStackConfig.RedisModel.RedisPassword,
    ClientName = builder.Configuration.GetValue<string>("HOSTNAME") ?? masaStackConfig.GetId(MasaStackProject.Scheduler, MasaStackApp.Worker)
};
builder.Services.AddMultilevelCache(options => options.UseStackExchangeRedisCache(redisOptions));
builder.Services.AddPmClient(masaStackConfig.GetPmServiceDomain());
builder.Services.AddMapster();
builder.Services.AddWorkerManager();
builder.Services.AddHttpClient();
builder.Services.AddSchedulerLogger();
builder.Services.AddCache(redisOptions);
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
         .UseUoW<SchedulerDbContext>(dbOptions => dbOptions.UseDbSql(masaStackConfig.GetDbType()).UseFilter(), useTransaction: false)
        .UseRepository<SchedulerDbContext>();
    });

await builder.Services.AddStackIsolationAsync(MasaStackProject.Scheduler.Name);

var app = builder.AddServices(options =>
{
    options.MapHttpMethodsForUnmatched = new[] { "Post" };
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
});
app.UseHttpsRedirection();

app.Run();
