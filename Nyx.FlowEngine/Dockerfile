# ==========================================
# Stage 1: Build & Publish
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder
WORKDIR /src

COPY ["Nyx.FlowEngine.csproj", "./"]
RUN dotnet restore "Nyx.FlowEngine.csproj"

COPY . .
RUN dotnet publish "Nyx.FlowEngine.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:DebugType=None \
    /p:DebugSymbols=false \
    /p:Optimize=true

# ==========================================
# Stage 2: Runtime Minimal & Secure (Alpine)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

RUN apk add --no-cache icu-libs tzdata curl
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LANG=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    ASPNETCORE_URLS=http://+:5072

COPY --from=builder --chown=app:app /app/publish .
USER app

EXPOSE 5072

HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5072/health || exit 1

ENTRYPOINT ["dotnet", "Nyx.FlowEngine.dll"]
