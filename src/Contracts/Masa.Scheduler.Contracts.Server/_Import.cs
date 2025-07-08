// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using FluentValidation;
global using Masa.BuildingBlocks.Caching;
global using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
global using Masa.BuildingBlocks.StackSdks.Config;
global using Masa.BuildingBlocks.StackSdks.Config.Models;
global using Masa.Contrib.Configuration.ConfigurationApi.Dcc.Options;
global using Masa.Scheduler.Contracts.Server.Dtos;
global using Masa.Scheduler.Contracts.Server.Infrastructure.Enums;
global using Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;
global using Masa.Scheduler.Contracts.Server.Messages;
global using Masa.Scheduler.Contracts.Server.Model;
global using Masa.Utils.Security.Cryptography;
global using Microsoft.AspNetCore.Hosting.Server;
global using Microsoft.AspNetCore.Hosting.Server.Features;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using System.Diagnostics;
global using System.Net;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using HttpMethods = Masa.Scheduler.Contracts.Server.Infrastructure.Enums.HttpMethods;
