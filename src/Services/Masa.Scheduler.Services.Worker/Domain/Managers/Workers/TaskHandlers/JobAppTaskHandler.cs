// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;

public class JobAppTaskHandler : ITaskHandler
{
    const string RESOURCE_PATH = "/ResourceFiles";
    const string EXTRACT_PATH = "/ExtractFiles";
    const string JOB_SHELL_SOURCE_PATH = "/JobShell";
    const string JOB_SHELL_NAME = "Masa.Scheduler.Shells.JobShell.dll";
    const string DLL_EXTENSION = ".dll";
    const string ZIP_EXTENSION = ".zip";

    private string _rootPath = Environment.CurrentDirectory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<JobAppTaskHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly SchedulerLogger _schedulerLogger;
    private Guid _taskId;
    private Guid _jobId;

    public JobAppTaskHandler(IHttpClientFactory httpClientFactory, ILogger<JobAppTaskHandler> logger, ILoggerFactory loggerFactory, IConfiguration configuration, SchedulerLogger schedulerLogger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _configuration = configuration;
        _schedulerLogger = schedulerLogger;
    }

    TaskRunStatus _runStatus = TaskRunStatus.Failure;

    public async Task<TaskRunStatus> RunTask(Guid taskId, SchedulerJobDto jobDto, DateTimeOffset excuteTime ,CancellationToken token)
    {
        _taskId = taskId;
        _jobId = jobDto.Id;

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

        var jobExtractPath = GetJobExtractPath(jobDto.Id, resource);
        var jobResoucePath = GetJobResourcePath(jobDto.Id, resource);

        await ProcessResource(resource, jobExtractPath, jobResoucePath);

        var processUtils = new ProcessUtils(_loggerFactory);

        processUtils.OutputDataReceived += JobAppOutputDataReceived;
        processUtils.ErrorDataReceived += JobAppErrorDataReceived;
        processUtils.Exit += JobApp_Exit;

        try
        {
            _schedulerLogger.LogInformation($"Process start", WriterTypes.Worker, taskId, jobDto.Id);
            var process = processUtils.Run("dotnet", GetJobShellRunParameter(jobDto, jobExtractPath, taskId, excuteTime));

            token.Register(() =>
            {
                if (!process.HasExited)
                {
                    _schedulerLogger.LogInformation($"Process kill", WriterTypes.Worker, taskId, jobDto.Id);
                    _runStatus = TaskRunStatus.Failure;
                    process.Kill();
                }
            });

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            _schedulerLogger.LogError(ex, "Process run Error", WriterTypes.Worker, taskId, jobDto.Id);
            throw;
        }

        return _runStatus;
    }

    private void JobApp_Exit(object? sender, EventArgs e)
    {
        _schedulerLogger.LogInformation("Job Exits", WriterTypes.Worker, _taskId, _jobId);
    }

    private string GetJobShellRunParameter(SchedulerJobDto dto, string jobExtractPath, Guid taskId, DateTimeOffset excuteTime)
    {
        var otlpEndpoint = _configuration.GetValue<string>("Local:OTLP:Endpoint");
        var parameterList = new List<string>()
        {
            _rootPath + Path.Combine(JOB_SHELL_SOURCE_PATH, JOB_SHELL_NAME),
            taskId.ToString(),
            Path.Combine(jobExtractPath, dto.JobAppConfig.JobEntryAssembly),
            dto.JobAppConfig.JobEntryClassName,
            dto.JobAppConfig.JobParams,
            excuteTime.Ticks.ToString(),
            excuteTime.Offset.Ticks.ToString(),
            dto.Id.ToString(),
            otlpEndpoint
        };

        _schedulerLogger.LogInformation($"JobShell run parameter: {string.Join(" ", parameterList)}", WriterTypes.Worker, _taskId, _jobId);

        return string.Join(" ", parameterList);
    }

    private string GetJobExtractPath(Guid jobId, SchedulerResourceDto resouce)
    {
        var version = resouce.Version;

        var jobExtractPath = _rootPath + Path.Combine(EXTRACT_PATH, jobId.ToString(), version);

        if (!Directory.Exists(jobExtractPath))
        {
            Directory.CreateDirectory(jobExtractPath);
        }

        return jobExtractPath;
    }

    private void JobAppErrorDataReceived(object? sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            return;
        }

        _schedulerLogger.LogError(e.Data, WriterTypes.Worker, _taskId, _jobId);

        _runStatus = TaskRunStatus.Failure;
    }

    private void JobAppOutputDataReceived(object? sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            return;
        }

        var output = e.Data;

        _schedulerLogger.LogInformation($"JobShell output logger, {output}", WriterTypes.Worker, _taskId, _jobId);

        if (output.StartsWith("{") && output.EndsWith("}"))
        {
            try
            {
                var result = JsonSerializer.Deserialize<JobShellRunResult>(output);

                if (result != null)
                {
                    if (result.IsSuccess)
                    {
                        _runStatus = TaskRunStatus.Success;
                    }
                    else
                    {
                        throw new UserFriendlyException(result.Message);
                    }
                }
                else
                {
                    _runStatus = TaskRunStatus.Failure;
                }
            }
            catch (Exception ex)
            {
                _schedulerLogger.LogError(ex, $"JobShell Result Deserialize Error, output: {output}, exception message: {ex.Message}", WriterTypes.Worker, _taskId, _jobId);
            }
        }
    }

    private async Task ProcessResource(SchedulerResourceDto resource, string jobExtractPath, string resourcePath)
    {
        var filePath = Path.Combine(resourcePath, resource.Name);

        if (!File.Exists(filePath))
        {
            _schedulerLogger.LogInformation($"Start download Resource", WriterTypes.Worker, _taskId, _jobId);
            await DownloadResource(resource, resourcePath);
            _schedulerLogger.LogInformation($"Download resource success", WriterTypes.Worker, _taskId, _jobId);
        }

        if (!File.Exists(filePath))
        {
            throw new UserFriendlyException("Resource Files not exists");
        }

        if (resource.Name.EndsWith(DLL_EXTENSION))
        {
            _schedulerLogger.LogError($"Start copy resource. version: {resource.Version}", WriterTypes.Worker, _taskId, _jobId);
            await CopyFolder(resourcePath, jobExtractPath);
            _schedulerLogger.LogError($"Copy resource success. version: {resource.Version}", WriterTypes.Worker, _taskId, _jobId);
        }
        else
        {
            _schedulerLogger.LogError($"Start decompress files. version: {resource.Version}", WriterTypes.Worker, _taskId, _jobId);
            DeCompressFile(resource, resourcePath, jobExtractPath);
            _schedulerLogger.LogError($"Decompress files success. version: {resource.Version}", WriterTypes.Worker, _taskId, _jobId);

            if (!Directory.Exists(jobExtractPath))
            {  
                throw new UserFriendlyException("Cannot found decompress folder, version: {resource.Version}");
            }
        }
    }

    private Task CopyFolder(string sources, string dest)
    {
        DirectoryInfo dinfo = new DirectoryInfo(sources);
        foreach (var f in dinfo.GetFileSystemInfos())
        {
            string destName = Path.Combine(dest, f.Name);
            if (f is FileInfo)
            {
                File.Copy(f.FullName, destName, true);
            }
            else
            {
                if (!Directory.Exists(destName))
                {
                    Directory.CreateDirectory(destName);
                }
               
                CopyFolder(f.FullName, destName);
            }
        }

        return Task.CompletedTask;
    }

    private async Task DownloadResource(SchedulerResourceDto resource, string resourcePath)
    {
        try
        {
            var path = resourcePath;

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
            _schedulerLogger.LogError(ex, "Download resource error", WriterTypes.Worker, _taskId, _jobId);
            throw;
        }
    }

    private string GetJobResourcePath(Guid jobId, SchedulerResourceDto resource)
    {
        //  /root/ResourcesFiles/0D627EA9-8656-4797-C4FE-08DA62E10FEE/1.0.0
        var path = Path.Combine(_rootPath + RESOURCE_PATH, jobId.ToString(), resource.Version);

        return path;
    }

    private void DeCompressFile(SchedulerResourceDto resource, string resourcePath, string jobExtractPath)
    {
        if (!resource.Name.EndsWith(ZIP_EXTENSION))
        {
            throw new UserFriendlyException("only support zip files");
        }

        var zipFilePath = Path.Combine(resourcePath, resource.Name);

        if (!File.Exists(zipFilePath))
        {
            throw new UserFriendlyException("cannot find zip files");
        }

        try
        {
            ZipFile.ExtractToDirectory(zipFilePath, jobExtractPath, true);
        }
        catch(Exception ex)
        {
            _schedulerLogger.LogError(ex, "Extract resource error", WriterTypes.Worker, _taskId, _jobId);
            throw;
        }
    }
}
