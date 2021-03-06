// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using Masa.Utils.Caller.HttpClient;
global using Masa.Utils.Caller.Core;
global using Masa.Scheduler.ApiGateways.Caller.Services;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.DependencyInjection;
global using System.Reflection;
global using Masa.Scheduler.Contracts.Server.Responses;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerResources;
global using Masa.Scheduler.Contracts.Server.Model;
global using Microsoft.AspNetCore.Authentication;