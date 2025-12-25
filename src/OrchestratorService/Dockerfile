# Use the official .NET runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0-preview AS base
WORKDIR /app

# Use the SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["src/OrchestratorService/OrchestratorService.csproj", "src/OrchestratorService/"]
COPY ["src/Shared.Kernel/Shared.Kernel.csproj", "src/Shared.Kernel/"]
RUN dotnet restore "src/OrchestratorService/OrchestratorService.csproj"
COPY . .
WORKDIR "/src/src/OrchestratorService"
RUN dotnet build "OrchestratorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrchestratorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrchestratorService.dll"]
