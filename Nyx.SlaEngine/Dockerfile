# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY ["Nyx.SlaEngine/Nyx.SlaEngine.csproj", "Nyx.SlaEngine/"]
RUN dotnet restore "Nyx.SlaEngine/Nyx.SlaEngine.csproj"

COPY . .
WORKDIR "/src/Nyx.SlaEngine"
RUN dotnet publish "Nyx.SlaEngine.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:DebugType=None /p:DebugSymbols=false

# Stage 2: Runtime (Alpine no-root security)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=builder /app/publish .

ENV ASPNETCORE_URLS=http://+:5070
EXPOSE 5070

ENTRYPOINT ["dotnet", "Nyx.SlaEngine.dll"]
