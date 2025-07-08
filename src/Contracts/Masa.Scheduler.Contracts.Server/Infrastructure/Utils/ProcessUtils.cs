// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Utils;

public class ProcessUtils
{
    private readonly ILogger<ProcessUtils>? _logger;

    public ProcessUtils(ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.CreateLogger<ProcessUtils>();
    }

    public Process Run(
        string fileName,
        string arguments,
        bool createNoWindow = true,
        bool isWait = false)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = !createNoWindow,
            CreateNoWindow = createNoWindow
        };
        var process = new Process()
        {
            StartInfo = processStartInfo,
        };
        if (createNoWindow)
        {
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;

            process.OutputDataReceived += (_, args) => OnOutputDataReceived(args);
            process.ErrorDataReceived += (_, args) => OnErrorDataReceived(args);
        }

        process.Start();
        if (createNoWindow)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        process.Exited += (_, _) => OnExited();
        string command = process.ProcessName + arguments;

        if (isWait)
        {
            process.WaitForExit();
        }
        return process;
    }

    public event EventHandler<DataReceivedEventArgs> OutputDataReceived = default!;

    public event EventHandler<DataReceivedEventArgs>? ErrorDataReceived;

    public event EventHandler Exit = default!;

    protected virtual void OnOutputDataReceived(DataReceivedEventArgs args)
    {
        try
        {
            OutputDataReceived(this, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "OnOutputDataReceived error ");
        }
    }

    protected virtual void OnErrorDataReceived(DataReceivedEventArgs args)
    {
        try
        {
            ErrorDataReceived?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "OnErrorDataReceived error");
        }
    }

    protected virtual void OnExited() => Exit(this, EventArgs.Empty);
}
