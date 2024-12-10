// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

// System Namespaces
global using System.Linq.Expressions;
global using System.Text.Json.Serialization;

// MASA Building Blocks
global using Masa.BuildingBlocks.Ddd.Domain.Entities.Full;
global using Masa.BuildingBlocks.Ddd.Domain.Events;
global using Masa.BuildingBlocks.Ddd.Domain.Repositories;
global using Masa.BuildingBlocks.Ddd.Domain.Values;
global using Masa.BuildingBlocks.Dispatcher.Events;

// MASA Scheduler Contracts
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;

// MASA Scheduler Domain Aggregates and Events
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;
global using Masa.Scheduler.Services.Server.Domain.Events;