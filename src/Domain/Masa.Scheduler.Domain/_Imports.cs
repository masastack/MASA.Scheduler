// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using Masa.BuildingBlocks.Ddd.Domain.Entities.Full;
global using Masa.BuildingBlocks.Ddd.Domain.Entities;
global using System.Collections.ObjectModel;
global using Masa.BuildingBlocks.Ddd.Domain.Repositories;
global using Masa.BuildingBlocks.Ddd.Domain.Values;
global using Masa.Contrib.Ddd.Domain;
global using Masa.BuildingBlocks.Ddd.Domain.Events;
global using Masa.BuildingBlocks.RulesEngine;
global using Masa.Contrib.RulesEngine.MicrosoftRulesEngine;
global using System.Collections.Concurrent;
global using Masa.BuildingBlocks.Dispatcher.Events;
global using Masa.BuildingBlocks.Data;
global using FluentValidation.Results;
global using Masa.Contrib.Dispatcher.Events;
global using Microsoft.Extensions.Logging;
global using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Commands;
global using System.Threading.Channels;
global using System.Text.Json.Serialization;
global using Masa.BuildingBlocks.Data.Contracts;
global using Masa.BuildingBlocks.Data.UoW;
global using Masa.BuildingBlocks.Ddd.Domain.Services;
global using Masa.BuildingBlocks.Caching;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Scheduler.Services.Server.Domain.Events;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;
global using System.Linq.Expressions;