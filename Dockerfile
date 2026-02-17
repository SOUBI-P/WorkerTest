# =========================
# STAGE 1 : BUILD
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build

WORKDIR /src

COPY bin/Debug/net8.0/publish/linux-arm/* .

# =========================
# STAGE 2 : RUNTIME
# =========================
FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim

WORKDIR /app

COPY --from=build /src .

ENTRYPOINT ["dotnet", "/app/WorkerTest.dll"]
