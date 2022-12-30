FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_sdk:6.0.403 AS publish
WORKDIR /src
COPY . .
ENV CSPROJ="src/Services/Masa.Scheduler.Services.Server/Masa.Scheduler.Services.Server.csproj"

RUN dotnet restore $CSPROJ && dotnet publish $CSPROJ -c Release -o /app/publish

FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_aspnet:6.0.0
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Masa.Scheduler.Services.Server.dll"]
