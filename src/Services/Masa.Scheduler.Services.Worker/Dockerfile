FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_sdk:6.0.100 AS publish
WORKDIR /src
COPY . .
ENV CSPROJ="src/Services/Masa.Scheduler.Services.Worker/Masa.Scheduler.Services.Worker.csproj"
RUN dotnet restore $CSPROJ && dotnet publish $CSPROJ -c Release -o /app/publish

ENV CSPROJ="src/Shells/Masa.Scheduler.Shells.JobShell/Masa.Scheduler.Shells.JobShell.csproj"
RUN dotnet restore $CSPROJ && dotnet publish $CSPROJ -c Release -o /app/publish/JobShell

FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_aspnet:6.0.0
WORKDIR /app
COPY --from=publish /app/publish .
COPY src/Services/Masa.Scheduler.Services.Worker/entrypoint.sh /app/entrypoint.sh
RUN sed -i 's/\r$//' /app/entrypoint.sh || true
RUN chmod +x /app/entrypoint.sh
RUN test -f /app/entrypoint.sh
ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80
ENTRYPOINT ["/bin/sh", "/app/entrypoint.sh"]

