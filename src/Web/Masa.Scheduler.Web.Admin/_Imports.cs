// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

// System Namespaces
global using System.Reflection;
global using System.Text;

// Third-party Libraries
global using FluentValidation;
global using Humanizer;
global using Humanizer.Localisation;
global using Mapster;

// Microsoft Namespaces
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Forms;
global using Microsoft.AspNetCore.Components.Rendering;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.JSInterop;
global using Quartz;

// MASA Blazor and Components
global using Masa.Blazor;
global using Masa.Stack.Components;
global using Masa.Stack.Components.Configs;
global using Masa.Stack.Components.Extensions;
global using Masa.Stack.Components.Models;

// MASA Building Blocks
global using Masa.BuildingBlocks.Authentication.Identity;
global using Masa.BuildingBlocks.Data.Mapping;
global using Masa.BuildingBlocks.StackSdks.Alert;
global using Masa.BuildingBlocks.StackSdks.Alert.Enum;
global using Masa.BuildingBlocks.StackSdks.Alert.Model;
global using Masa.BuildingBlocks.StackSdks.Auth;
global using Masa.BuildingBlocks.StackSdks.Auth.Contracts;
global using Masa.BuildingBlocks.StackSdks.Auth.Contracts.Model;
global using Masa.BuildingBlocks.StackSdks.Config;
global using Masa.BuildingBlocks.StackSdks.Pm;
global using Masa.BuildingBlocks.StackSdks.Pm.Model;
global using Masa.BuildingBlocks.StackSdks.Tsc;
global using Masa.BuildingBlocks.StackSdks.Tsc.Contracts.Model;

// MASA Contrib Packages
global using Masa.Contrib.StackSdks.Caller;
global using Masa.Contrib.StackSdks.Config;

// MASA Scheduler Contracts and Models
global using Masa.Scheduler.ApiGateways.Caller;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Consts;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Contracts.Server.Infrastructure.SignalRClients;
global using Masa.Scheduler.Contracts.Server.Model;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerResources;
global using Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;

// MASA Scheduler Web Admin Components and Store
global using Masa.Scheduler.Web.Admin.Components.AlarmRules;
global using Masa.Scheduler.Web.Admin.Components.AlarmRules.ViewModel;
global using Masa.Scheduler.Web.Admin.Pages.SchedulerResources.Components;
global using Masa.Scheduler.Web.Admin.Store;

global using Masa.BuildingBlocks.StackSdks.Pm.Enum;
global using Masa.Scheduler.Web.Admin.Model;

global using Masa.Contrib.StackSdks.Tsc.Storage.Clickhouse.Apm.Models.Response;
global using System.Text.Json;
global using Masa.Scheduler.Web.Admin.Components.Tsc;
global using System.Security.Cryptography;
global using System.Web;
global using Masa.BuildingBlocks.StackSdks.Tsc.Storage.Contracts;
global using Masa.Contrib.StackSdks.Tsc.Storage.Clickhouse;
global using Masa.Contrib.StackSdks.Tsc.Storage.Clickhouse.Apm.Models.Request;
global using Masa.BuildingBlocks.StackSdks.Tsc.Contracts.Log;
global using Masa.BuildingBlocks.StackSdks.Tsc.Contracts.Trace;
global using DeepCloner.Core;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using Masa.Contrib.StackSdks.Tsc.Storage.Clickhouse.Apm.Models;
global using Microsoft.Extensions.Caching.Memory;
global using System.ComponentModel;