# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /app

# Copy project files
COPY AngularDotnetBoilerplate.sln ./
COPY src/Core/Core.csproj ./src/Core/
COPY src/Infrastructure/Infrastructure.csproj ./src/Infrastructure/
COPY src/API/API.csproj ./src/API/

# Restore packages
RUN dotnet restore --disable-parallel --no-cache

# Copy source code
COPY src/ ./src/

# Build application
RUN dotnet build -c Release --no-restore

# Publish application
RUN dotnet publish src/API/API.csproj -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Install timezone data and curl for health checks
RUN apk add --no-cache tzdata curl

WORKDIR /app

# Create logs directory
RUN mkdir -p logs

# Copy published application
COPY --from=build /app/publish .

# Create non-root user
RUN adduser -D -s /bin/sh appuser && chown -R appuser:appuser /app
USER appuser

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV TZ=UTC

EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/api/ping || exit 1

ENTRYPOINT ["dotnet", "API.dll"]