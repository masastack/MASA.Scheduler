param(
    [Parameter(Mandatory = $true)][string]$t,
    [Parameter(Mandatory = $true)][string]$u,
    [Parameter(Mandatory = $true)][string]$p,
    [Alias("H")][string]$DockerHost
)

Write-Host "Hello $t"

$dockerBaseArgs = @()
if (-not [string]::IsNullOrWhiteSpace($DockerHost))
{
    if ($DockerHost -notmatch "^[a-z]+://")
    {
        $DockerHost = "tcp://$DockerHost"
    }

    $dockerBaseArgs += @("--host", $DockerHost)
    Write-Host "Use remote Docker host: $DockerHost"
}

function Invoke-Docker
{
    param(
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Args
    )

    & docker @dockerBaseArgs @Args
    if ($LASTEXITCODE -ne 0)
    {
        throw "Docker command failed: docker $($Args -join ' ')"
    }
}

Invoke-Docker login --username=$u registry.cn-hangzhou.aliyuncs.com --password=$p

$WorkerDockerfilePath = "./src/Services/Masa.Scheduler.Services.Worker/Dockerfile"
$WorkerServerName = "masa-scheduler-services-worker"
$ServiceDockerfilePath = "./src/Services/Masa.Scheduler.Services.Server/Dockerfile"
$ServiceServerName = "masa-scheduler-services-server"

Invoke-Docker build -t registry.cn-hangzhou.aliyuncs.com/masastack/${WorkerServerName}:$t -f $WorkerDockerfilePath .
Invoke-Docker push registry.cn-hangzhou.aliyuncs.com/masastack/${WorkerServerName}:$t

Invoke-Docker build -t registry.cn-hangzhou.aliyuncs.com/masastack/${ServiceServerName}:$t -f $ServiceDockerfilePath .
Invoke-Docker push registry.cn-hangzhou.aliyuncs.com/masastack/${ServiceServerName}:$t