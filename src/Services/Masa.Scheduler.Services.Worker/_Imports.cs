﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using Masa.Scheduler.Contracts.Server.Model;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
global using Masa.Utils.Caching.Core.Interfaces;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents.Logs;
global using Masa.Contrib.Dispatcher.IntegrationEvents.Dapr;
global using Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EF;
global using Masa.Contrib.Data.UoW.EF;
global using Masa.Scheduler.Services.Worker.Infrastructure;
global using Masa.Contrib.Isolation.UoW.EF;
global using Masa.Contrib.Data.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using System.Reflection;
global using Microsoft.AspNetCore.Hosting.Server;
global using Microsoft.AspNetCore.Hosting.Server.Features;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Managers;
global using Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Masa.Contrib.Data.Mapping.Mapster;
global using Masa.Scheduler.Services.Worker.Managers.Workers;
global using Microsoft.OpenApi.Models;
global using FluentValidation.AspNetCore;
global using Masa.Contrib.Ddd.Domain;
global using Masa.Contrib.Dispatcher.Events;
global using Masa.BuildingBlocks.Dispatcher.Events;
global using FluentValidation;
global using Masa.Scheduler.Services.Worker.Infrastructure.Middleware;
global using Masa.Contrib.Isolation.MultiEnvironment;
global using Masa.Contrib.Data.Contracts.EF;
global using Masa.Contrib.Ddd.Domain.Repository.EF;
global using Masa.Contrib.Service.MinimalAPIs;
global using Dapr;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerWorker;
global using Masa.Contrib.ReadWriteSpliting.Cqrs.Commands;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Consts;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using System.Text.Json;
global using Masa.Scheduler.Services.Worker.Domain.Managers.Workers.Data;
global using Microsoft.AspNetCore.Mvc;
global using System.Collections.Concurrent;
global using System.Net;
global using System.Text;
global using Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;
global using System.IO.Compression;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Utils;
global using Dapr.Client;
global using Masa.Contrib.Dispatcher.IntegrationEvents;
global using Masa.Utils.Development.Dapr.AspNetCore;
global using Masa.BuildingBlocks.Configuration;
global using Masa.Contrib.Configuration.ConfigurationApi.Dcc;
global using Masa.Utils.Caching.DistributedMemory.DependencyInjection;
global using Masa.Utils.Caching.Redis.Models;