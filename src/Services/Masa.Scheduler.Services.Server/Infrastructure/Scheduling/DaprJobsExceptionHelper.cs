// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public static class DaprJobsExceptionHelper
{
    public static bool IsNotFound(Exception exception)
    {
        var message = exception.Message;
        return message.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
               || message.Contains("404", StringComparison.OrdinalIgnoreCase);
    }
}
