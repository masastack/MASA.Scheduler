// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

// MASA Stack Building Blocks
global using Masa.BuildingBlocks.Data.UoW;
global using Masa.Contrib.Ddd.Domain.Repository.EFCore;

// Microsoft Entity Framework Core
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// Microsoft Extensions
global using Microsoft.Extensions.Logging;

// System Namespaces
global using System.Data.Common;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Text.Json;
global using System.Text.RegularExpressions;

// MASA Scheduler Domain Aggregates
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Resources;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;

// MASA Scheduler Repositories and Configurations
global using Masa.Scheduler.Services.Server.Domain.Repositories;
global using Masa.Scheduler.EntityFrameworkCore.EntityConfigurations.ValueConverts;

// Integration Events
global using Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore;