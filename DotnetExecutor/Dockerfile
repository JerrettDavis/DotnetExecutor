FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DotnetExecutor/DotnetExecutor.csproj", "DotnetExecutor/"]
RUN dotnet restore "DotnetExecutor/DotnetExecutor.csproj"
COPY . .
WORKDIR "/src/DotnetExecutor"
RUN dotnet build "DotnetExecutor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotnetExecutor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotnetExecutor.dll"]
