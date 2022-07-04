// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = "";
    options.RequireHttpsMetadata = false;
    options.Audience = "";
});

builder.Services.AddMasaRedisCache(builder.Configuration.GetSection("RedisConfig"));
builder.Services.AddPmClient(builder.Configuration.GetValue<string>("PmClient:Url"));
builder.Services.AddMapping();
builder.Services.AddWorkerManager();
builder.Services.AddHttpClient();
builder.Services.AddMasaSignalR();
builder.Services.AddQuartzUtils();
//builder.Services.AddQuartzJob();

var secretStoreName = builder.Configuration.GetValue<string>("SecretStoreName");

builder.Services.AddAliyunStorage(serviceProvider =>
{
    try
    {
        var daprClient = serviceProvider.GetRequiredService<DaprClient>();
        var accessId = daprClient.GetSecretAsync(secretStoreName, "access_id").Result.First().Value;
        var accessSecret = daprClient.GetSecretAsync(secretStoreName, "access_secret").Result.First().Value;
        var endpoint = daprClient.GetSecretAsync(secretStoreName, "endpoint").Result.First().Value;
        var roleArn = daprClient.GetSecretAsync(secretStoreName, "roleArn").Result.First().Value;
        return new AliyunStorageOptions(accessId, accessSecret, endpoint, roleArn, "SessionTest")
        {
            Sts = new AliyunStsOptions()
            {
                RegionId = "cn-hangzhou"
            }
        };
    }
    catch(Exception ex)
    {
        throw;
    }
    
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter();
}

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
        .UseDaprEventBus<IntegrationEventLogService>(options => options.UseEventLog<SchedulerDbContext>())
        .UseEventBus(eventBusBuilder =>
        {
            eventBusBuilder.UseMiddleware(typeof(ValidatorMiddleware<>));
            eventBusBuilder.UseMiddleware(typeof(LogMiddleware<>));
        })
        .UseIsolationUoW<SchedulerDbContext>(
            isolationBuilder => isolationBuilder.UseMultiEnvironment("env"),
            dbOptions => dbOptions.UseSqlServer().UseFilter())
        .UseRepository<SchedulerDbContext>();
    })
    .AddServices(builder);

app.UseMasaExceptionHandling(opt =>
{
    opt.CustomExceptionHandler = exception =>
    {
        Exception friendlyException = exception;
        if (exception is ValidationException validationException)
        {
            friendlyException = new UserFriendlyException(validationException.Errors.Select(err => err.ToString()).FirstOrDefault()!);
        }
        return (friendlyException, false);
    };
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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
