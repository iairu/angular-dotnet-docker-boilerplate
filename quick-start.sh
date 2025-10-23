#!/bin/bash

# Quick Start Script for Angular .NET Docker Demo
# This script handles the PostgreSQL "users table does not exist" error
# by ensuring a clean database state on startup

set -e

echo "=========================================="
echo "🚀 Angular .NET Docker Demo - Quick Start"
echo "=========================================="
echo ""

# Check if docker is available
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed or not in PATH"
    exit 1
fi

# Check if docker compose is available
if ! docker compose version &> /dev/null; then
    echo "❌ Docker Compose is not available"
    exit 1
fi

echo "✅ Docker and Docker Compose are available"
echo ""

# Stop any running containers
echo "🛑 Stopping existing containers..."
docker compose down 2>/dev/null || true
echo ""

# Remove PostgreSQL volume for clean state
echo "🗑️  Removing old PostgreSQL volume..."
docker volume rm angular-dotnet-docker-demo_postgres-data 2>/dev/null || echo "   (Volume doesn't exist - this is fine for first run)"
echo ""

# Build and start containers
echo "🔨 Building and starting containers..."
docker compose up --build -d
echo ""

echo "⏳ Waiting for services to start (30 seconds)..."
sleep 30
echo ""

# Show the status
echo "📊 Container Status:"
docker compose ps
echo ""

# Show recent logs
echo "📋 Recent Backend Logs:"
echo "========================================"
docker compose logs backend --tail 50
echo "========================================"
echo ""

# Check if API is responding
echo "🔍 Checking API health..."
if curl -f http://localhost:8080/health &> /dev/null; then
    echo "✅ API is healthy and responding!"
else
    echo "⚠️  API health check failed (it might need more time)"
fi
echo ""

echo "=========================================="
echo "✅ Quick Start Complete!"
echo "=========================================="
echo ""
echo "📡 API URL:         http://localhost:8080"
echo "📖 Swagger Docs:    http://localhost:8080/swagger"
echo "🔍 Health Check:    http://localhost:8080/health"
echo ""
echo "📝 Useful Commands:"
echo "   View logs:       docker compose logs -f backend"
echo "   Stop services:   docker compose down"
echo "   Restart:         docker compose restart backend"
echo ""
echo "🐛 If you encounter the 'users table does not exist' error:"
echo "   The application will automatically detect and fix it!"
echo "   Check the logs above for migration verification messages."
echo ""
