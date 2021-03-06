// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using BlazorComponent;
global using BlazorComponent.I18n;
global using Masa.Blazor;
global using Masa.Scheduler.Web.Admin.Data.Base;
global using Masa.Scheduler.Web.Admin.Global;
global using Masa.Scheduler.Web.Admin.Global.Nav.Model;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Forms;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Http;
global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;
global using System.Net.Http.Json;
global using System.Reflection;
global using System.Text.Json;
global using Masa.Scheduler.ApiGateways.Caller;
global using Masa.Scheduler.ApiGateways.Caller.Services;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Stack.Components;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;
global using Masa.Scheduler.Contracts.Server.Model;
global using Masa.Scheduler.Contracts.Server.Responses;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;
global using Masa.Scheduler.Contracts.Server.Infrastructure.SignalRClients;
global using Humanizer;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Consts;
global using System.Globalization;
global using Microsoft.AspNetCore.SignalR.Client;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerResources;
global using Masa.Scheduler.Web.Admin.Model;
global using Microsoft.JSInterop;
global using Microsoft.Extensions.DependencyInjection;
global using Quartz;
global using Masa.BuildingBlocks.Identity.IdentityModel;
global using static Masa.Stack.Components.JsInitVariables;