#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Web/MASA.Scheduler.Admin.Server/Masa.Scheduler.Admin.Server.csproj", "src/Web/MASA.Scheduler.Admin.Server/"]
COPY ["src/Web/MASA.Scheduler.Admin/Masa.Scheduler.Admin.csproj", "src/Web/MASA.Scheduler.Admin/"]
RUN dotnet restore "src/Web/MASA.Scheduler.Admin.Server/Masa.Scheduler.Admin.Server.csproj"
COPY . .
WORKDIR "/src/src/Web/MASA.Scheduler.Admin.Server"
RUN dotnet build "Masa.Scheduler.Admin.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Masa.Scheduler.Admin.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Masa.Scheduler.Admin.Server.dll"]