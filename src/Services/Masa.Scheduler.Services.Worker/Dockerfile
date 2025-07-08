FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_sdk:8.0 AS publish
WORKDIR /src
COPY . .
ENV CSPROJ="src/Services/Masa.Scheduler.Services.Worker/Masa.Scheduler.Services.Worker.csproj"
RUN dotnet restore $CSPROJ && dotnet publish $CSPROJ -c Release -o /app/publish

ENV CSPROJ="src/Shells/Masa.Scheduler.Shells.JobShell/Masa.Scheduler.Shells.JobShell.csproj"
RUN dotnet restore $CSPROJ && dotnet publish $CSPROJ -c Release -o /app/publish/JobShell

FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_aspnet:8.0
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Masa.Scheduler.Services.Worker.dll"]

