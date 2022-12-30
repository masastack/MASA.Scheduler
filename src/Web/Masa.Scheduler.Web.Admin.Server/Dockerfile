FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_sdk:6.0.403 AS publish
WORKDIR /src
COPY . .
ENV CSPROJ="src/Web/Masa.Scheduler.Web.Admin.Server/Masa.Scheduler.Web.Admin.Server.csproj"
RUN dotnet restore $CSPROJ && dotnet publish $CSPROJ -c Release -o /app/publish

FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_aspnet:6.0.0
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=https://0.0.0.0:443
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "Masa.Scheduler.Web.Admin.Server.dll"]

