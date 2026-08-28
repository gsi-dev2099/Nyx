# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CRM.ApiHub/CRM.ApiHub.csproj", "CRM.ApiHub/"]
COPY ["Nyx.FlowEngine/Nyx.FlowEngine.csproj", "Nyx.FlowEngine/"]
COPY ["Nyx.ApprovalEngine/Nyx.ApprovalEngine.csproj", "Nyx.ApprovalEngine/"]
COPY ["Nyx.SlaEngine/Nyx.SlaEngine.csproj", "Nyx.SlaEngine/"]
RUN dotnet restore "CRM.ApiHub/CRM.ApiHub.csproj"

COPY . .
WORKDIR "/src/CRM.ApiHub"
RUN dotnet publish "CRM.ApiHub.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:DebugType=None /p:DebugSymbols=false

# Stage 2: Runtime (Alpine no-root security)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

COPY --from=build /app/publish .
RUN mkdir -p /app/Storage && chown -R appuser:appgroup /app

USER appuser

ENV ASPNETCORE_URLS=http://+:5068
EXPOSE 5068

ENTRYPOINT ["dotnet", "CRM.ApiHub.dll"]
