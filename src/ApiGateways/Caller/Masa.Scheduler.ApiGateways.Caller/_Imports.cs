// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using Masa.Contrib.Service.Caller.HttpClient;
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
global using Masa.Contrib.Service.Caller;
global using Masa.BuildingBlocks.Service.Caller;
global using Masa.BuildingBlocks.StackSdks.Auth.Contracts.Provider;
global using System.Net.Http.Headers;