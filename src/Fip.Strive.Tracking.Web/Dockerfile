# No `# syntax=` directive: it would pull a frontend image from Docker Hub, and nothing else here
# needs Docker Hub at all — both stages come from mcr.microsoft.com.

# Build from the repository root, not from this directory — the project inherits
# Directory.Build.props, Directory.Packages.props and global.json from src/:
#
#   docker build -f src/Fip.Strive.Tracking.Web/Dockerfile -t strive-tracking .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# The manifests first and on their own, so editing a razor file does not invalidate the
# restore layer and re-download every package.
COPY src/global.json src/NuGet.config src/Directory.Build.props src/Directory.Packages.props ./
COPY src/Fip.Strive.Tracking.Application/Fip.Strive.Tracking.Application.csproj Fip.Strive.Tracking.Application/
COPY src/Fip.Strive.Tracking.Web/Fip.Strive.Tracking.Web.csproj Fip.Strive.Tracking.Web/
RUN dotnet restore Fip.Strive.Tracking.Web/Fip.Strive.Tracking.Web.csproj

COPY src/Fip.Strive.Tracking.Application/ Fip.Strive.Tracking.Application/
COPY src/Fip.Strive.Tracking.Web/ Fip.Strive.Tracking.Web/
RUN dotnet publish Fip.Strive.Tracking.Web/Fip.Strive.Tracking.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Both directories are written to at runtime and everything under /app is owned by root, so
# they have to exist and belong to the app user before it drops its privileges. A named volume
# inherits this ownership; a bind mount does not, and has to be chowned on the host.
RUN mkdir -p /data /app/logs \
    && chown -R $APP_UID:$APP_UID /data /app/logs

# Everything environment-specific reaches the app as an environment variable, never as a file:
# appsettings.Development.json is kept out of the build context by .dockerignore, and the
# appsettings.json that does ship carries only application defaults — no paths, no secrets. Both
# Access__PasswordHash and Access__ApiKey are supplied at `docker run`, never baked in.
#
# The database lands in the volume. Everything the app knows is in that one file — copy it away
# and that is your backup.
ENV Tracking__DataDirectory=/data \
    ASPNETCORE_HTTP_PORTS=8080

VOLUME /data
EXPOSE 8080

# Readiness lives at /health. No HEALTHCHECK here: the runtime image ships neither curl nor
# wget, and installing one just to poll an endpoint the orchestrator can poll itself is a poor
# trade.
USER $APP_UID
ENTRYPOINT ["dotnet", "Fip.Strive.Tracking.Web.dll"]
