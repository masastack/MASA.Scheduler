param($t,$u,$p,$s)

#docker build 
#Write-Host "Hello.$args"
Write-Host "Hello $t"

switch($s)
{
  server {$DockerfilePath="./src/Services/Masa.Scheduler.Services.Server/Dockerfile";$ServerName="masa-scheduler-services-server"}
  work {$DockerfilePath="./src/Services/Masa.Scheduler.Services.Worker/Dockerfile";$ServerName="masa-scheduler-services-worker"}
  web  {$DockerfilePath="./src/Web/Masa.Scheduler.Web.Admin.Server/Dockerfile";$ServerName="masa-scheduler-web-admin-server"}
}
docker login --username=$u registry.cn-hangzhou.aliyuncs.com --password=$p
docker build -t registry.cn-hangzhou.aliyuncs.com/masa/${ServerName}:$t  -f $DockerfilePath .
docker push registry.cn-hangzhou.aliyuncs.com/masa/${ServerName}:$t 