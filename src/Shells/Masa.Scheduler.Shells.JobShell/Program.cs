// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder();
Debug.WriteLine(string.Join("@", args));
var cmd = args.ToShellCommand();
if (cmd == null)
    throw new ArgumentNullException(nameof(ShellCommandModel));

Debug.WriteLine(cmd.ToString());
ShellHelper.UseOpenTelemtry(builder, cmd.Value);
var app = builder.Build();
await ShellHelper.Execute(app.Services, cmd.Value);