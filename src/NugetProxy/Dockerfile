FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["src/NugetProxy/NugetProxy.csproj", "src/NugetProxy/"]
COPY ["src/NugetProxy.Core/NugetProxy.Core.csproj", "src/NugetProxy.Core/"]
COPY ["src/NugetProxy.Protocol/NugetProxy.Protocol.csproj", "src/NugetProxy.Protocol/"]
COPY ["src/NugetProxy.Core.Server/NugetProxy.Core.Server.csproj", "src/NugetProxy.Core.Server/"]
COPY ["src/Directory.Build.props", "src/Directory.Build.props"]
RUN dotnet restore "src/NugetProxy/NugetProxy.csproj"
COPY . .
WORKDIR "/src/src/NugetProxy"
RUN dotnet build "NugetProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NugetProxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NugetProxy.dll"]
