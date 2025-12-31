# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["src/ApiGateway/ApiGateway.csproj", "src/ApiGateway/"]
COPY ["src/Shared.Application/Shared.Application.csproj", "src/Shared.Application/"]
COPY ["src/Shared.Core/Shared.Core.csproj", "src/Shared.Core/"]
COPY ["src/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared.Infrastructure/"]
RUN dotnet restore "src/ApiGateway/ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/ApiGateway"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY src/ApiGateway/wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
