// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.
global using System.ComponentModel;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Contracts.Server.Messages;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using FluentValidation;
global using System.Reflection;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;
global using HttpMethods = Masa.Scheduler.Contracts.Server.Infrastructure.Enums.HttpMethods;
global using Masa.Scheduler.Contracts.Server.Model;
global using Masa.Contrib.Dispatcher.IntegrationEvents.Dapr;
global using Masa.Utils.Caching.Core.Interfaces;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
global using Microsoft.AspNetCore.Hosting.Server;
global using Microsoft.AspNetCore.Hosting.Server.Features;
global using Microsoft.Extensions.DependencyInjection;
global using System.Text.Json;
global using Dapr;
global using Dapr.Client;
global using Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Consts;
global using Microsoft.Extensions.Logging;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Utils;