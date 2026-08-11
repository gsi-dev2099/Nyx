# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CRM.ApiHub/CRM.ApiHub.csproj", "CRM.ApiHub/"]
RUN dotnet restore "CRM.ApiHub/CRM.ApiHub.csproj"

COPY . .
WORKDIR "/src/CRM.ApiHub"
RUN dotnet publish "CRM.ApiHub.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:DebugType=None /p:DebugSymbols=false

# Stage 2: Runtime (Alpine no-root security)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5068
EXPOSE 5068

ENTRYPOINT ["dotnet", "CRM.ApiHub.dll"]
