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
global using Masa.Contrib.Service.Caller.Authentication.OpenIdConnect;
global using System.Net.Http.Headers;
global using IdentityModel.Client;
global using IdentityModel;
global using Masa.Contrib.Service.Caller.Authentication.OpenIdConnect.Jwt;
global using Microsoft.Extensions.Logging;
global using Microsoft.IdentityModel.Tokens;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;