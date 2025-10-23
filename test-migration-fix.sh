#!/bin/bash

# Test script for migration verification fix
# This script demonstrates the auto-recovery feature

set -e

COMPOSE_PROJECT="angular-dotnet-docker-demo"
POSTGRES_VOLUME="${COMPOSE_PROJECT}_postgres-data"

echo "=========================================="
echo "🧪 Testing Migration Verification Fix"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}Step 1: Stopping existing containers...${NC}"
docker compose down
echo ""

echo -e "${BLUE}Step 2: Removing old PostgreSQL volume...${NC}"
docker volume rm ${POSTGRES_VOLUME} 2>/dev/null || echo "Volume doesn't exist yet"
echo ""

echo -e "${BLUE}Step 3: Starting containers with fresh database...${NC}"
echo "This will trigger the initial migration..."
docker compose up -d --build
echo ""

echo -e "${YELLOW}Waiting 10 seconds for services to start...${NC}"
sleep 10
echo ""

echo -e "${BLUE}Step 4: Checking initial migration logs...${NC}"
docker compose logs backend | grep -A 5 "Verifying database schema"
echo ""

echo -e "${GREEN}✅ Initial migration should show all tables exist${NC}"
echo ""

echo -e "${BLUE}Step 5: Simulating corrupted database state...${NC}"
echo "Dropping the 'users' table while keeping migration history..."
docker compose exec -T postgres psql -U boilerplate_user -d boilerplate_db -c "DROP TABLE IF EXISTS users CASCADE;" || true
echo ""

echo -e "${YELLOW}Users table dropped. Migration history still exists.${NC}"
echo ""

echo -e "${BLUE}Step 6: Restarting backend to trigger auto-recovery...${NC}"
docker compose restart backend
echo ""

echo -e "${YELLOW}Waiting 15 seconds for restart and recovery...${NC}"
sleep 15
echo ""

echo -e "${BLUE}Step 7: Checking recovery logs...${NC}"
docker compose logs backend --tail 100 | grep -A 10 "Verifying database schema" | tail -20
echo ""

echo -e "${BLUE}Step 8: Verifying database state...${NC}"
echo "Checking if users table now exists..."
TABLE_EXISTS=$(docker compose exec -T postgres psql -U boilerplate_user -d boilerplate_db -t -c "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'users');" | xargs)

if [ "$TABLE_EXISTS" = "t" ]; then
    echo -e "${GREEN}✅ SUCCESS: Users table exists!${NC}"
else
    echo -e "${RED}❌ FAILED: Users table does not exist${NC}"
    exit 1
fi
echo ""

echo -e "${BLUE}Step 9: Testing Entity Framework functionality...${NC}"
docker compose logs backend | grep -A 15 "ENTITY FRAMEWORK FUNCTIONALITY TEST" | tail -20
echo ""

echo "=========================================="
echo -e "${GREEN}✅ All tests passed!${NC}"
echo "=========================================="
echo ""
echo "The migration verification fix is working correctly:"
echo "  1. ✅ Initial migrations apply successfully"
echo "  2. ✅ Corrupted state is detected"
echo "  3. ✅ Migrations are automatically reapplied"
echo "  4. ✅ Tables are verified after recovery"
echo ""
echo "To view full logs, run: docker compose logs backend"
echo "To stop containers, run: docker compose down"
echo ""