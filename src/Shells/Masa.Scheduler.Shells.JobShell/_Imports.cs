// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using Masa.BuildingBlocks.StackSdks.Scheduler;
global using Masa.BuildingBlocks.StackSdks.Scheduler.Model;
global using Masa.Contrib.StackSdks.Tsc;
global using Masa.Scheduler.Shells.JobShell.Extensions;
global using Masa.Scheduler.Shells.JobShell.Models;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using OpenTelemetry.Logs;
global using OpenTelemetry.Metrics;
global using OpenTelemetry.Resources;
global using OpenTelemetry.Trace;
global using System.Reflection;
global using System.Text.Json;
