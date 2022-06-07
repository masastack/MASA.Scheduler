﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using Dapr.Actors;
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
global using Masa.Scheduler.Services.Server.Application.Jobs.Commands;
global using Masa.Scheduler.Services.Server.Application.Jobs.Queries;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;
global using Masa.Scheduler.Services.Server.Domain.Repositories;
global using Masa.Scheduler.Services.Server.Domain.Services;
global using Masa.Scheduler.Services.Server.Infrastructure;
global using Masa.Scheduler.Services.Server.Infrastructure.Middleware;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
global using Masa.Utils.Caching.Redis;
global using Masa.Utils.Caching.Redis.DependencyInjection;
global using Masa.Utils.Caller.HttpClient;
global using Masa.Contrib.Isolation.MultiEnvironment;
global using Masa.Contrib.Isolation.UoW.EF;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.OpenApi.Models;
global using Masa.BuildingBlocks.Ddd.Domain.Entities.Auditing;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Resources;
global using System.Reflection;
global using Masa.Contrib.ReadWriteSpliting.Cqrs.Queries;
global using Masa.Scheduler.Services.Server.Application.Teams.Queries;
global using Masa.Scheduler.Services.Server.Application.Projects.Queries;
global using Masa.BuildingBlocks.BasicAbility.Pm;
global using Masa.Contrib.BasicAbility.Pm;
global using HttpMethods = Masa.Scheduler.Contracts.Server.Infrastructure.Enums.HttpMethods;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using System.Text.Json;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
global using Masa.Scheduler.Services.Server.Infrastructure.EntityConfigurations.ValueConverts;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Microsoft.AspNetCore.Mvc;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;
global using Mapster;
global using Masa.Scheduler.Contracts.Server.Validator;
global using System.Linq.Expressions;
global using Masa.Contrib.Data.Mapping.Mapster;
global using Masa.BuildingBlocks.Data.Mapping;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;
global using Masa.Scheduler.Services.Server.Application.Tasks.Commands;
global using Masa.Scheduler.Services.Server.Application.Tasks.Queries;
global using Masa.Scheduler.Services.Server.Domain.Events;
global using Masa.Scheduler.Contracts.Server.Model;
global using Masa.Scheduler.Services.Server.Domain.Managers.Servers;
global using Masa.Utils.Caching.Core.Interfaces;
global using Masa.Scheduler.Services.Server.Infrastructure.Common;
global using Masa.Scheduler.Contracts.Server.Responses;
global using Masa.Utils.Exceptions.Extensions;
global using Masa.Contrib.Data.EntityFrameworkCore.SqlServer;
global using Masa.Contrib.Data.Contracts.EF;
global using Masa.Utils.Extensions.Expressions;
global using Masa.Contrib.Data.EntityFrameworkCore;
global using Masa.BuildingBlocks.Ddd.Domain.Entities.Full;
global using Dapr;
global using Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;
global using Microsoft.AspNetCore.Hosting.Server;
global using Microsoft.AspNetCore.Hosting.Server.Features;
global using Masa.Utils.Development.Dapr.AspNetCore;
global using Masa.Scheduler.ApiGateways.Caller;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Consts;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerWorker;
global using Masa.BuildingBlocks.Ddd.Domain.Values;
global using System.Text.Json.Serialization;
global using Masa.Scheduler.Services.Server.Domain.Managers.Servers.Data;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Managers;
global using System.Collections.Concurrent;
global using Microsoft.AspNetCore.SignalR;
global using Masa.Scheduler.Services.Server.Infrastructure.SignalR.Hubs;
global using Masa.Scheduler.Services.Server.Infrastructure.SignalR;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Utils;
