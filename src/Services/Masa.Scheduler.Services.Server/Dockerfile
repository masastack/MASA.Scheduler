FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_sdk:6.0.100 AS publish
WORKDIR /src
COPY . .
ENV CSPROJ="src/Services/Masa.Scheduler.Services.Server/Masa.Scheduler.Services.Server.csproj"
RUN cat /src/src/Contracts/Masa.Scheduler.Contracts.Server/Masa.Scheduler.Contracts.Server.csproj
RUN echo "------"&& cat ./src/Contracts/Masa.Scheduler.Contracts.Server/Masa.Scheduler.Contracts.Server.csproj
RUN  pwd && ls && dotnet restore $CSPROJ && dotnet publish $CSPROJ -c Release -o /app/publish

FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_aspnet:6.0.0
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=https://0.0.0.0:443
EXPOSE 443
ENTRYPOINT ["dotnet", "Masa.Scheduler.Services.Server.dll"]
