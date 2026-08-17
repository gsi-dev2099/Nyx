# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY ["Nyx.FlowEngine/Nyx.FlowEngine.csproj", "Nyx.FlowEngine/"]
RUN dotnet restore "Nyx.FlowEngine/Nyx.FlowEngine.csproj"

COPY . .
WORKDIR "/src/Nyx.FlowEngine"
RUN dotnet publish "Nyx.FlowEngine.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:DebugType=None /p:DebugSymbols=false

# Stage 2: Runtime (Alpine no-root security)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=builder /app/publish .

ENV ASPNETCORE_URLS=http://+:5072
EXPOSE 5072

ENTRYPOINT ["dotnet", "Nyx.FlowEngine.dll"]
