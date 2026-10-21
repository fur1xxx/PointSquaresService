ARG DOTNET_VERSION="10.0"

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
RUN apt-get -y update && \
    apt-get install -y curl && \
    apt-get clean && \
    rm -r /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props .editorconfig global.json ./
COPY source/Common/Common.csproj source/Common/
COPY source/Domain.Core/Domain.Core.csproj source/Domain.Core/
COPY source/Domain.Handlers/Domain.Handlers.csproj source/Domain.Handlers/
COPY source/Persistence.Core/Persistence.Core.csproj source/Persistence.Core/
COPY source/PointSquaresService/PointSquaresService.csproj source/PointSquaresService/
RUN dotnet restore source/PointSquaresService/PointSquaresService.csproj

COPY source/ source/
RUN dotnet publish source/PointSquaresService/PointSquaresService.csproj -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PointSquaresService.dll"]
