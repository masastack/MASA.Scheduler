param($t,$u,$p)

#docker build 
#Write-Host "Hello.$args"
Write-Host "Hello $t"

docker login --username=$u registry.cn-hangzhou.aliyuncs.com --password=$p

$WorkerDockerfilePath="./src/Services/Masa.Scheduler.Services.Worker/Dockerfile"
$WorkerServerName="masa-scheduler-services-worker"
$ServiceDockerfilePath="./src/Services/Masa.Scheduler.Services.Server/Dockerfile"
$ServiceServerName="masa-scheduler-services-server"
$WebDockerfilePath="./src/Web/Masa.Scheduler.Web.Admin.Server/Dockerfile"
$WebServerName="masa-scheduler-web-admin"

docker build -t registry.cn-hangzhou.aliyuncs.com/masastack/${WorkerServerName}:$t  -f $WorkerDockerfilePath .
docker push registry.cn-hangzhou.aliyuncs.com/masastack/${WorkerServerName}:$t 

docker build -t registry.cn-hangzhou.aliyuncs.com/masastack/${ServiceServerName}:$t  -f $ServiceDockerfilePath .
docker push registry.cn-hangzhou.aliyuncs.com/masastack/${ServiceServerName}:$t 

docker build -t registry.cn-hangzhou.aliyuncs.com/masastack/${WebServerName}:$t  -f $WebDockerfilePath .
docker push registry.cn-hangzhou.aliyuncs.com/masastack/${WebServerName}:$t 