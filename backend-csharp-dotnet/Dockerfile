# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy solution file and project files
COPY AngularDotnetBoilerplate.sln ./
COPY src/API/API.csproj ./src/API/
COPY src/Core/Core.csproj ./src/Core/
COPY src/Infrastructure/Infrastructure.csproj ./src/Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY src/ ./src/

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish src/API/API.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 8.0 ASP.NET runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Set the entry point
ENTRYPOINT ["dotnet", "API.dll"]