# No `# syntax=` directive: it would pull a frontend image from Docker Hub, and nothing else here
# needs Docker Hub at all — both stages come from mcr.microsoft.com.

# Build from the repository root, not from this directory — the project inherits
# Directory.Build.props, Directory.Packages.props and global.json from src/:
#
#   docker build -f src/Fip.Strive.Web/Dockerfile -t strive .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# The manifests first and on their own, so editing a razor file does not invalidate the
# restore layer and re-download every package. Versions are pinned centrally by
# Directory.Packages.props; there are no lock files, for the reasons recorded under A5 in
# docs/review/repository-review-2026-08.md.
COPY src/global.json src/NuGet.config src/Directory.Build.props src/Directory.Packages.props ./
COPY src/Fip.Strive.Application/Fip.Strive.Application.csproj Fip.Strive.Application/
COPY src/Fip.Strive.Web/Fip.Strive.Web.csproj Fip.Strive.Web/
RUN dotnet restore Fip.Strive.Web/Fip.Strive.Web.csproj

COPY src/Fip.Strive.Application/ Fip.Strive.Application/
COPY src/Fip.Strive.Web/ Fip.Strive.Web/
RUN dotnet publish Fip.Strive.Web/Fip.Strive.Web.csproj \
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
# appsettings.json that does ship carries only application defaults — no paths, no secrets.
#
# ConnectionStrings__strive is required and deliberately not defaulted: the app has no sensible
# fallback for "which Postgres", and a wrong default is worse than a missing one.
#
# /data holds blobs/ and incoming/. That is the content-addressed store — the archives themselves
# are discarded after import, so this directory *is* the data.
ENV Storage__DataDirectory=/data \
    ASPNETCORE_HTTP_PORTS=8080

VOLUME /data
EXPOSE 8080

# This app has no authentication, by the decision recorded in the Readme: it is a LAN tool and every
# page, including the upload form, is open to whoever can reach the port. Publish it accordingly.
#
# Readiness lives at /health and queries Postgres rather than reporting on the process. No HEALTHCHECK
# here: the runtime image ships neither curl nor wget, and installing one just to poll an endpoint
# the orchestrator can poll itself is a poor trade.
USER $APP_UID
ENTRYPOINT ["dotnet", "Fip.Strive.Web.dll"]
