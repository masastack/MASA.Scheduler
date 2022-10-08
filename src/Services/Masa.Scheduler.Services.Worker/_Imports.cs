﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using Masa.Scheduler.Contracts.Server.Model;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents.Logs;
global using Masa.Contrib.Dispatcher.IntegrationEvents.Dapr;
global using Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore;
global using Masa.Contrib.Data.UoW.EFCore;
global using Masa.Scheduler.Services.Worker.Infrastructure;
global using Masa.Contrib.Isolation.UoW.EFCore;
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
global using Masa.Contrib.Data.Contracts.EFCore;
global using Masa.Contrib.Ddd.Domain.Repository.EFCore;
global using Dapr;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerWorker;
global using Masa.Contrib.ReadWriteSplitting.Cqrs.Commands;
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
global using Masa.Contrib.Caching.MultilevelCache;
global using Masa.Contrib.Caching.Distributed.StackExchangeRedis;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Extensions;
global using Masa.Utils.Security.Cryptography;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Logger;
global using Masa.BuildingBlocks.Ddd.Domain.Entities.Full;
global using Masa.BuildingBlocks.Caching;