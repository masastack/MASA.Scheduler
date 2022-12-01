// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservable(builder.Logging, builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter(opt =>
    {
        opt.AppId = "masa-scheduler-service-worker";
        opt.AppIdSuffix = "";
        opt.DaprHttpPort = 10604;
        opt.DaprGrpcPort = 10603;
    });
}
builder.Services.AddMasaConfiguration(configurationBuilder =>
{
    configurationBuilder.UseDcc();
});
var publicConfiguration = builder.Services.GetMasaConfiguration().ConfigurationApi.GetPublic();
builder.Services.AddDaprClient();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = publicConfiguration.GetValue<string>("$public.AppSettings:IdentityServerUrl");
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters.ValidateAudience = false;
    options.MapInboundClaims = false;
});

var redisOptions = publicConfiguration.GetSection("$public.RedisConfig").Get<RedisConfigurationOptions>();
builder.Services.AddStackExchangeRedisCache(redisOptions)
    .AddMultilevelCache();
builder.Services.AddMapster();
builder.Services.AddWorkerManager();
builder.Services.AddHttpClient();
builder.Services.AddSchedulerLogger();

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
            isolationBuilder => isolationBuilder.UseMultiEnvironment("env"),
            dbOptions => dbOptions.UseSqlServer().UseFilter())
        .UseRepository<SchedulerDbContext>();
    })
    .AddServices(builder, options =>
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

app.UseCloudEvents();
app.UseEndpoints(endpoints =>
{
    endpoints.MapSubscribeHandler();
});
app.UseHttpsRedirection();

app.Run();
