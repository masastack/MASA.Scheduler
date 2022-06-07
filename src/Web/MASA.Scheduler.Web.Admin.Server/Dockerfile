#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Web/Masa.Scheduler.Web.Admin.Server/Masa.Scheduler.Web.Admin.Server.csproj", "src/Web/Masa.Scheduler.Web.Admin.Server/"]
COPY ["src/Web/Masa.Scheduler.Web.Admin/Masa.Scheduler.Web.Admin.csproj", "src/Web/Masa.Scheduler.Web.Admin/"]
COPY ["src/ApiGateways/Caller/Masa.Scheduler.ApiGateways.Caller/Masa.Scheduler.ApiGateways.Caller.csproj", "src/ApiGateways/Caller/Masa.Scheduler.ApiGateways.Caller/"]
COPY ["src/Contracts/Masa.Scheduler.Contracts.Server/Masa.Scheduler.Contracts.Server.csproj", "src/Contracts/Masa.Scheduler.Contracts.Server/"]
RUN dotnet restore "src/Web/Masa.Scheduler.Web.Admin.Server/Masa.Scheduler.Web.Admin.Server.csproj"
COPY . .
WORKDIR "/src/src/Web/Masa.Scheduler.Web.Admin.Server"
RUN dotnet build "Masa.Scheduler.Web.Admin.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Masa.Scheduler.Web.Admin.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Masa.Scheduler.Web.Admin.Server.dll"]