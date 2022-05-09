﻿global using Dapr.Actors;
global using Dapr.Actors.Client;
global using Dapr.Actors.Runtime;
global using FluentValidation;
global using FluentValidation.AspNetCore;
global using Masa.BuildingBlocks.Data.UoW;
global using Masa.BuildingBlocks.Ddd.Domain.Entities;
global using Masa.BuildingBlocks.Ddd.Domain.Events;
global using Masa.BuildingBlocks.Ddd.Domain.Repositories;
global using Masa.BuildingBlocks.Dispatcher.Events;
global using Masa.Contrib.Data.UoW.EF;
global using Masa.Contrib.Ddd.Domain;
global using Masa.Contrib.Ddd.Domain.Events;
global using Masa.Contrib.Ddd.Domain.Repository.EF;
global using Masa.Contrib.Dispatcher.Events;
global using Masa.Contrib.Dispatcher.Events.Enums;
global using Masa.Contrib.Dispatcher.IntegrationEvents.Dapr;
global using Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EF;
global using Masa.Contrib.ReadWriteSpliting.Cqrs.Commands;
global using Masa.Contrib.Service.MinimalAPIs;
global using Masa.Utils.Data.EntityFrameworkCore;
global using MASA.Scheduler.Service.Actors;
global using MASA.Scheduler.Service.Application.Jobs.Commands;
global using MASA.Scheduler.Service.Application.Jobs.Queries;
global using MASA.Scheduler.Service.Domain.Aggregates.Jobs;
global using MASA.Scheduler.Service.Domain.Events;
global using MASA.Scheduler.Service.Domain.Repositories;
global using MASA.Scheduler.Service.Domain.Services;
global using MASA.Scheduler.Service.Infrastructure;
global using MASA.Scheduler.Service.Infrastructure.Middleware;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
global using Masa.Utils.Caching.Redis;
global using Masa.Utils.Caching.Redis.DependencyInjection;
global using Masa.Utils.Caller.HttpClient;
global using Masa.Contrib.BasicAbility.Pm;
global using Masa.Contrib.Isolation.MultiEnvironment;
global using Masa.Utils.Data.EntityFrameworkCore.SqlServer;
global using Masa.Contrib.Isolation.UoW.EF;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.OpenApi.Models;