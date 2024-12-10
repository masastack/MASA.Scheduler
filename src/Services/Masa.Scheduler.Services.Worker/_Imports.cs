// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

// System Namespaces
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.IO.Compression;
global using System.Net;
global using System.Reflection;
global using System.Text.Json;

// Third-party Libraries
global using Dapr.Client;
global using FluentValidation;
global using FluentValidation.AspNetCore;

// Microsoft Namespaces
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.OpenApi.Models;

// MASA Building Blocks
global using Masa.BuildingBlocks.Caching;
global using Masa.BuildingBlocks.Data.UoW;
global using Masa.BuildingBlocks.Ddd.Domain.Repositories;
global using Masa.BuildingBlocks.Dispatcher.Events;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
global using Masa.BuildingBlocks.Isolation;
global using Masa.BuildingBlocks.StackSdks.Config;
global using Masa.BuildingBlocks.StackSdks.Isolation;

// MASA Contrib Packages
global using Masa.Contrib.Caching.Distributed.StackExchangeRedis;
global using Masa.Contrib.Configuration.ConfigurationApi.Dcc;
global using Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore;
global using Masa.Contrib.StackSdks.Config;
global using Masa.Contrib.StackSdks.Isolation;
global using Masa.Contrib.StackSdks.Tsc;

// MASA Scheduler Contracts
global using Masa.Scheduler;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Consts;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Logger;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Managers;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Utils;
global using Masa.Scheduler.Contracts.Server.Model;

// MASA Scheduler Services Worker
global using Masa.Scheduler.Services.Worker.Domain.Managers.Workers;
global using Masa.Scheduler.Services.Worker.Domain.Managers.Workers.Data;
global using Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;
global using Masa.Scheduler.Services.Worker.Infrastructure;
global using Masa.Scheduler.Services.Worker.Infrastructure.Extensions;
global using Masa.Scheduler.Services.Worker.Infrastructure.Middleware;
global using Masa.Scheduler.Services.Worker.Managers.Workers;

// Utility and Security
global using Masa.Utils.Security.Cryptography;