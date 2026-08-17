# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CRM.WebFrontend/CRM.WebFrontend.csproj", "CRM.WebFrontend/"]
COPY ["CRM.WebFrontend.Client/CRM.WebFrontend.Client.csproj", "CRM.WebFrontend.Client/"]
RUN dotnet restore "CRM.WebFrontend/CRM.WebFrontend.csproj"

COPY . .
WORKDIR "/src/CRM.WebFrontend"
RUN dotnet publish "CRM.WebFrontend.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:DebugType=None /p:DebugSymbols=false

# Stage 2: Runtime (Alpine no-root security)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5261
EXPOSE 5261

ENTRYPOINT ["dotnet", "CRM.WebFrontend.dll"]
