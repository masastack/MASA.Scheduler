// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

// System Namespaces
global using System.Security.Cryptography.X509Certificates;

// Microsoft Namespaces
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Hosting.StaticWebAssets;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.IdentityModel.Logging;
global using Microsoft.IdentityModel.Protocols.OpenIdConnect;

// MASA Stack Components
global using Masa.Stack.Components;
global using Masa.Stack.Components.Extensions.OpenIdConnect;

// MASA Contrib Packages
global using Masa.Contrib.StackSdks.Caller;
global using Masa.Contrib.StackSdks.Config;
global using Masa.Contrib.StackSdks.Tsc;

// MASA Scheduler Specific
global using Masa.Scheduler.ApiGateways.Caller;
global using Masa.Scheduler.Contracts.Server.Infrastructure.SignalRClients;