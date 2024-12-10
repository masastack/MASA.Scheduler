// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using System.Collections.Concurrent;
global using Masa.BuildingBlocks.Data;
global using Masa.BuildingBlocks.Data.UoW;
global using Masa.BuildingBlocks.Data.Contracts;
global using Masa.Contrib.Ddd.Domain.Repository.EFCore;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.Extensions.Configuration;
global using System.Reflection;
global using Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Resources;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;
global using Masa.Scheduler.Services.Server.Domain.Repositories;
global using System.Linq.Expressions;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
global using System.Text.Json;
global using Masa.Scheduler.EntityFrameworkCore.EntityConfigurations.ValueConverts;
global using Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using System.Data.Common;
global using System.Text.RegularExpressions;