FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["src/BaGet/BaGet.csproj", "src/BaGet/"]
COPY ["src/BaGet.Core/BaGet.Core.csproj", "src/BaGet.Core/"]
COPY ["src/BaGet.Protocol/BaGet.Protocol.csproj", "src/BaGet.Protocol/"]
COPY ["src/BaGet.Core.Server/BaGet.Core.Server.csproj", "src/BaGet.Core.Server/"]
COPY ["src/Directory.Build.props", "src/Directory.Build.props"]
RUN dotnet restore "src/BaGet/BaGet.csproj"
COPY . .
WORKDIR "/src/src/BaGet"
RUN dotnet build "BaGet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BaGet.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BaGet.dll"]
