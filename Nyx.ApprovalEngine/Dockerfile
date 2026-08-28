# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY ["Nyx.ApprovalEngine/Nyx.ApprovalEngine.csproj", "Nyx.ApprovalEngine/"]
RUN dotnet restore "Nyx.ApprovalEngine/Nyx.ApprovalEngine.csproj"

COPY . .
WORKDIR "/src/Nyx.ApprovalEngine"
RUN dotnet publish "Nyx.ApprovalEngine.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:DebugType=None /p:DebugSymbols=false

# Stage 2: Runtime (Alpine no-root security)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

COPY --from=builder /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser

ENV ASPNETCORE_URLS=http://+:5071
EXPOSE 5071

ENTRYPOINT ["dotnet", "Nyx.ApprovalEngine.dll"]
