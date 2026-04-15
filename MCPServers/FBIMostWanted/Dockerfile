FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY ["MCPServers/Shared/Shared.csproj", "MCPServers/Shared/"]
COPY ["MCPServers/FBIMostWanted/FBIMostWanted.csproj", "MCPServers/FBIMostWanted/"]
RUN dotnet restore "MCPServers/FBIMostWanted/FBIMostWanted.csproj"

COPY MCPServers/Shared/ MCPServers/Shared/
COPY MCPServers/FBIMostWanted/ MCPServers/FBIMostWanted/

RUN dotnet publish "MCPServers/FBIMostWanted/FBIMostWanted.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
EXPOSE 4547
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FBIMostWanted.dll"]
