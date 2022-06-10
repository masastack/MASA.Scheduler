#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Services/Masa.Scheduler.Services.Worker/Masa.Scheduler.Services.Worker.csproj", "src/Services/Masa.Scheduler.Services.Worker/"]
COPY ["src/Contracts/Masa.Scheduler.Contracts.Server/Masa.Scheduler.Contracts.Server.csproj", "src/Contracts/Masa.Scheduler.Contracts.Server/"]
RUN dotnet restore "src/Services/Masa.Scheduler.Services.Worker/Masa.Scheduler.Services.Worker.csproj"
COPY . .
WORKDIR "/src/src/Services/Masa.Scheduler.Services.Worker"
RUN dotnet build "Masa.Scheduler.Services.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Masa.Scheduler.Services.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Masa.Scheduler.Services.Worker.dll"]