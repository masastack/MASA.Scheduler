// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using Masa.Scheduler.ApiGateways.Caller;
global using Masa.Scheduler.Contracts.Server.Infrastructure.SignalRClients;
global using Masa.Stack.Components;
global using Microsoft.AspNetCore.Hosting.StaticWebAssets;
global using System.Security.Cryptography.X509Certificates;
global using Masa.Utils.Data.Elasticsearch;
global using Microsoft.Extensions.DependencyInjection;
global using Masa.BuildingBlocks.Configuration;
global using Masa.Contrib.Service.Caller.Authentication.OpenIdConnect;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.IdentityModel.Protocols.OpenIdConnect;
global using Masa.Stack.Components.Extensions.OpenIdConnect;