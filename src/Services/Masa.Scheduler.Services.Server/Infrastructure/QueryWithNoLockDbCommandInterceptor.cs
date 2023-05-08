﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure;

public class QueryWithNoLockDbCommandInterceptor : DbCommandInterceptor
{
    private static readonly Regex TableAliasRegex =
        new Regex(@"(?<tableAlias>AS \[[a-zA-Z]\w*\](?! (WITH \(NOLOCK\)|ON)))",
            RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        command.CommandText = TableAliasRegex.Replace(
            command.CommandText,
            "${tableAlias} WITH (NOLOCK)"
            );
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        command.CommandText = TableAliasRegex.Replace(
            command.CommandText,
            "${tableAlias} WITH (NOLOCK)"
            );
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        command.CommandText = TableAliasRegex.Replace(
            command.CommandText,
            "${tableAlias} WITH (NOLOCK)"
            );
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        command.CommandText = TableAliasRegex.Replace(
            command.CommandText,
            "${tableAlias} WITH (NOLOCK)"
            );
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}