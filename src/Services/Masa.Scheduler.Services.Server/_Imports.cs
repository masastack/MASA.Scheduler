// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using Dapr;

global using FluentValidation;
global using FluentValidation.AspNetCore;

global using Mapster;
global using Masa.BuildingBlocks.Authentication.Identity;
global using Masa.BuildingBlocks.Caching;
global using Masa.BuildingBlocks.Configuration;
global using Masa.BuildingBlocks.Data.Mapping;
global using Masa.BuildingBlocks.Data.UoW;
global using Masa.BuildingBlocks.Ddd.Domain.Events;
global using Masa.BuildingBlocks.Ddd.Domain.Repositories;
global using Masa.BuildingBlocks.Ddd.Domain.Services;
global using Masa.BuildingBlocks.Dispatcher.Events;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
global using Masa.BuildingBlocks.Isolation;
global using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Commands;
global using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Queries;
global using Masa.BuildingBlocks.StackSdks.Alert;
global using Masa.BuildingBlocks.StackSdks.Auth;
global using Masa.BuildingBlocks.StackSdks.Auth.Contracts;
global using Masa.BuildingBlocks.StackSdks.Config;
global using Masa.BuildingBlocks.StackSdks.Config.Consts;
global using Masa.BuildingBlocks.StackSdks.Config.Models;
global using Masa.BuildingBlocks.StackSdks.Isolation;
global using Masa.BuildingBlocks.StackSdks.Pm;
global using Masa.BuildingBlocks.Storage.ObjectStorage;

global using Masa.Contrib.Caching.Distributed.StackExchangeRedis;
global using Masa.Contrib.Configuration.ConfigurationApi.Dcc;
global using Masa.Contrib.Dispatcher.Events;
global using Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore;
global using Masa.Contrib.StackSdks.Config;
global using Masa.Contrib.StackSdks.Isolation;
global using Masa.Contrib.StackSdks.Middleware;
global using Masa.Contrib.StackSdks.Tsc;
global using Masa.Contrib.Storage.ObjectStorage.Aliyun;
global using Masa.Scheduler;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Consts;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Logger;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Managers;
global using Masa.Scheduler.Contracts.Server.Model;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerResources;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;
global using Masa.Scheduler.Contracts.Server.Responses;
global using Masa.Scheduler.Contracts.Server.Validator;

global using Masa.Scheduler.Services.Server.Application.Auths.Queries;
global using Masa.Scheduler.Services.Server.Application.Jobs.Commands;
global using Masa.Scheduler.Services.Server.Application.Jobs.Queries;
global using Masa.Scheduler.Services.Server.Application.Projects.Queries;
global using Masa.Scheduler.Services.Server.Application.Resources.Commands;
global using Masa.Scheduler.Services.Server.Application.Resources.Queries;
global using Masa.Scheduler.Services.Server.Application.Tasks.Commands;
global using Masa.Scheduler.Services.Server.Application.Tasks.Queries;

global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Resources;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;
global using Masa.Scheduler.Services.Server.Domain.Events;
global using Masa.Scheduler.Services.Server.Domain.Managers.Servers;
global using Masa.Scheduler.Services.Server.Domain.Managers.Servers.Data;
global using Masa.Scheduler.Services.Server.Domain.QuartzJob;
global using Masa.Scheduler.Services.Server.Domain.Repositories;
global using Masa.Scheduler.Services.Server.Domain.Services;

global using Masa.Scheduler.Services.Server.Infrastructure.Common;
global using Masa.Scheduler.Services.Server.Infrastructure.Extensions;
global using Masa.Scheduler.Services.Server.Infrastructure.Middleware;
global using Masa.Scheduler.Services.Server.Infrastructure.Quartz;
global using Masa.Scheduler.Services.Server.Infrastructure.SignalR;
global using Masa.Scheduler.Services.Server.Infrastructure.SignalR.Hubs;

global using Masa.Scheduler.EntityFrameworkCore;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Options;
global using Microsoft.OpenApi.Models;
global using Quartz;
global using StackExchange.Redis;

global using System.Data;
global using System.Linq.Expressions;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.RegularExpressions;

// Alias to avoid naming conflicts
global using ValidationException = FluentValidation.ValidationException;