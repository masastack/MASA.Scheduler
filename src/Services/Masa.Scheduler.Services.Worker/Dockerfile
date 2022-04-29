#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Services/Masa.Scheduler.Worker/Masa.Scheduler.Worker.csproj", "src/Services/Masa.Scheduler.Worker/"]
RUN dotnet restore "src/Services/Masa.Scheduler.Worker/Masa.Scheduler.Worker.csproj"
COPY . .
WORKDIR "/src/src/Services/Masa.Scheduler.Worker"
RUN dotnet build "Masa.Scheduler.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Masa.Scheduler.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Masa.Scheduler.Worker.dll"]