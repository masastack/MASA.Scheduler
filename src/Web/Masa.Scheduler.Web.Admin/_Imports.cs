﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using BlazorComponent;
global using BlazorComponent.I18n;
global using Humanizer;
global using Masa.Blazor;
global using Masa.BuildingBlocks.Authentication.Identity;
global using Masa.BuildingBlocks.Data.Mapping;
global using Masa.BuildingBlocks.StackSdks.Auth.Contracts;
global using Masa.Scheduler.ApiGateways.Caller;
global using Masa.Scheduler.ApiGateways.Caller.Services;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Consts;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Contracts.Server.Infrastructure.SignalRClients;
global using Masa.Scheduler.Contracts.Server.Model;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerResources;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;
global using Masa.Scheduler.Contracts.Server.Responses;
global using Masa.Scheduler.Web.Admin.Data.Base;
global using Masa.Scheduler.Web.Admin.Global;
global using Masa.Scheduler.Web.Admin.Model;
global using Masa.Scheduler.Web.Admin.Pages.SchedulerResources.Components;
global using Masa.Stack.Components;
global using Masa.Stack.Components.Models;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Forms;
global using Microsoft.AspNetCore.Components.Rendering;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.JSInterop;
global using Quartz;
global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;
global using System.Globalization;
global using System.Net.Http.Json;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;
global using static Masa.Stack.Components.JsInitVariables;
global using Masa.BuildingBlocks.SearchEngine.AutoComplete;
global using Masa.BuildingBlocks.StackSdks.Auth;
global using Masa.Contrib.StackSdks.Caller;
global using Masa.BuildingBlocks.StackSdks.Alert.Enum;
global using Masa.BuildingBlocks.StackSdks.Alert.Model;
global using FluentValidation;
global using Masa.BuildingBlocks.StackSdks.Alert;
global using Masa.BuildingBlocks.StackSdks.Pm;
global using Masa.BuildingBlocks.StackSdks.Pm.Model;
global using Masa.BuildingBlocks.StackSdks.Tsc;
global using Masa.BuildingBlocks.StackSdks.Tsc.Contracts.Model;
global using Mapster;
global using Masa.BuildingBlocks.StackSdks.Config;
global using Masa.Contrib.StackSdks.Config;
global using Masa.Scheduler.Web.Admin.Components.AlarmRules;