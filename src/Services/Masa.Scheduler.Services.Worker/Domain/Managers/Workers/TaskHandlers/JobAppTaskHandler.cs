// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.Extensions.PlatformAbstractions;

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;

public class JobAppTaskHandler : ITaskHandler
{
    const string RESOURCE_PATH = "/ResourceFiles";
    const string EXTRACT_PATH = "/ExtractFiles";
    const string JOB_SHELL_PATH = "/JobShell/Masa.Scheduler.Shells.JobShell.dll";
    const string DLL_EXTENSION = ".dll";
    const string ZIP_EXTENSION = ".zip";

    private string _rootPath = PlatformServices.Default.Application.ApplicationBasePath;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<JobAppTaskHandler> _logger;

    public JobAppTaskHandler(IHttpClientFactory httpClientFactory, ILogger<JobAppTaskHandler> logger, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    TaskRunStatus _runStatus = TaskRunStatus.Failure;

    public async Task<TaskRunStatus> RunTask(Guid taskId, SchedulerJobDto jobDto, DateTimeOffset excuteTime ,CancellationToken token)
    {
        if (jobDto.JobAppConfig is null)
        {
            throw new UserFriendlyException("JobAppConfig is required in JobApp Task");
        }

        if(jobDto.JobAppConfig.SchedulerResourceDto is null)
        {
            throw new UserFriendlyException("Scheduler Resource cannot be null");
        }

        var resource = jobDto.JobAppConfig.SchedulerResourceDto;

        if (string.IsNullOrEmpty(resource.FilePath))
        {
            throw new UserFriendlyException("Scheduler Resource FilePath cannot empty");
        }

        _runStatus = TaskRunStatus.Failure;

        var resourcePath = await GetResourceFullPath(resource);

        var processUtils = new ProcessUtils(_loggerFactory);

        processUtils.OutputDataReceived += JobAppOutputDataReceived;
        processUtils.ErrorDataReceived += JobAppErrorDataReceived;
        processUtils.Exit += JobApp_Exit;

        try
        {
            var process = processUtils.Run("dotnet ", GetJobShellRunParameter(jobDto, resourcePath, taskId, excuteTime));

            token.Register(() =>
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            });

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Process run Error, TaskId: {taskId}");
            throw new UserFriendlyException($"Process run Error, TaskId: {taskId}");
        }

        return _runStatus;
    }

    private void JobApp_Exit(object? sender, EventArgs e)
    {
        _logger.LogInformation("Job Exits");
    }

    private string GetJobShellRunParameter(SchedulerJobDto dto, string resourcePath, Guid taskId, DateTimeOffset excuteTime)
    {
        var parameterList = new List<string>()
        {
            _rootPath + JOB_SHELL_PATH,
            taskId.ToString(),
            Path.Combine(resourcePath, dto.JobAppConfig.JobEntryAssembly),
            dto.JobAppConfig.JobEntryMethod,
            dto.JobAppConfig.JobParams,
            excuteTime.ToString(),
            dto.Id.ToString(),
        };

        return string.Join(" ", parameterList);
    }

    private void JobAppErrorDataReceived(object? sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            return;
        }

        _logger.LogError(e.Data);

        _runStatus = TaskRunStatus.Failure;
    }

    private void JobAppOutputDataReceived(object? sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            return;
        }

        var output = e.Data;

        if (output.StartsWith("{") && output.EndsWith("}"))
        {
            try
            {
                var result = JsonSerializer.Deserialize<JobShellRunResult>(output);

                if (result != null)
                {
                    _runStatus = result.IsSuccess ? TaskRunStatus.Success : TaskRunStatus.Failure;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"JobShell Result Deserialize Error, output: {output}");
            }
        }
        else
        {
            _logger.LogInformation(output);
        }
    }

    private async Task<string> GetResourceFullPath(SchedulerResourceDto resource)
    {
        var filePath = Path.Combine(GetResourceFilePath(resource), resource.Name);
        
        if (!File.Exists(filePath))
        {
           await DownloadResource(resource);
        }

        if (!File.Exists(filePath))
        {
            throw new UserFriendlyException("Resource Files not exists");
        }

        if (resource.Name.EndsWith(DLL_EXTENSION))
        {
            return GetResourceFilePath(resource);
        }
        else
        {
            var extractPath = GetExtractFilePath(resource);

            if (!Directory.Exists(extractPath))
            {
                DeCompressFile(resource);
            }

            if (!Directory.Exists(extractPath))
            {
                throw new UserFriendlyException("Cannot found decompress folder");
            }

            return extractPath;
        }
    }

    private async Task DownloadResource(SchedulerResourceDto resource)
    {
        try
        {
            var path = GetResourceFilePath(resource);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filePath = Path.Combine(path, resource.Name);

            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(resource.FilePath);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();

            await using var fs = File.Create(filePath);

            stream.Seek(0, SeekOrigin.Begin);

            stream.CopyTo(fs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download resource error");
            throw new UserFriendlyException("Download resource error");
        }
    }

    private string GetResourceFilePath(SchedulerResourceDto resource)
    {
        var fileInfo = new FileInfo(resource.Name);

        var fileName = fileInfo.Name.Replace(fileInfo.Extension, "");

        var path = Path.Combine(_rootPath + RESOURCE_PATH, resource.Version + fileName);

        return path;
    }

    private string GetExtractFilePath(SchedulerResourceDto resource)
    {
        var fileInfo = new FileInfo(resource.Name);

        var fileName = fileInfo.Name.Replace(fileInfo.Extension, "");

        return Path.Combine(_rootPath + EXTRACT_PATH, resource.Version + fileName);
    }

    private void DeCompressFile(SchedulerResourceDto resource)
    {
        if (!resource.Name.EndsWith(ZIP_EXTENSION))
        {
            throw new UserFriendlyException("only support zip files");
        }

        var zipFilePath = Path.Combine(GetResourceFilePath(resource), resource.Name);

        if (!File.Exists(zipFilePath))
        {
            throw new UserFriendlyException("cannot find zip files");
        }

        try
        {
            ZipFile.ExtractToDirectory(zipFilePath, GetExtractFilePath(resource));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Extract resource error");
            throw new UserFriendlyException("Extract resource error");
        }
    }
}
