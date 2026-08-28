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

RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser

ENV ASPNETCORE_URLS=http://+:5261
EXPOSE 5261

ENTRYPOINT ["dotnet", "CRM.WebFrontend.dll"]
