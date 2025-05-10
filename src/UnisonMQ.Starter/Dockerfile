FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UnisonMQ.Starter/UnisonMQ.Starter.csproj", "src/UnisonMQ.Starter/"]
COPY ["src/UnisonMQ.Server/UnisonMQ.Server.csproj", "src/UnisonMQ.Server/"]
COPY ["src/UnisonMQ.Abstractions/UnisonMQ.Abstractions.csproj", "src/UnisonMQ.Abstractions/"]
COPY ["src/UnisonMQ.Operations/UnisonMQ.Operations.csproj", "src/UnisonMQ.Operations/"]
COPY ["src/UnisonMQ.Queues/UnisonMQ.Queues.csproj", "src/UnisonMQ.Queues/"]
RUN dotnet restore "src/UnisonMQ.Starter/UnisonMQ.Starter.csproj"
COPY . .
WORKDIR "/src/src/UnisonMQ.Starter"
RUN dotnet build "UnisonMQ.Starter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UnisonMQ.Starter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5888
EXPOSE 5889
ENTRYPOINT ["dotnet", "UnisonMQ.Starter.dll"]
