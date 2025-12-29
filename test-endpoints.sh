#!/bin/bash

# Gift E-Commerce API Testing Script - Health Checks Only
# Server IP: http://72.61.102.216

SERVER_IP="72.61.102.216"

# Service ports (from docker-compose.production.yml)
GATEWAY_PORT="5000"
IDENTITY_PORT="5001"
CATEGORY_PORT="5002"
INVENTORY_PORT="5003"
CART_PORT="5005"
ORDER_PORT="5006"
OFFER_PORT="5007"
NOTIFICATION_PORT="5008"
PROFILE_PORT="5009"

echo "=========================================="
echo "Gift E-Commerce API - Health Checks"
echo "=========================================="

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to test health endpoint
test_health() {
    local name=$1
    local url=$2
    
    echo -e "${YELLOW}Testing: ${name}${NC}"
    echo "URL: ${url}"
    
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X GET \
        -H "Content-Type: application/json" \
        "${url}")
    
    http_code=$(echo "$response" | grep "HTTP_CODE" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE/d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo -e "${GREEN}✓ Success (HTTP ${http_code})${NC}"
    else
        echo -e "${RED}✗ Failed (HTTP ${http_code})${NC}"
    fi
    echo "Response: $body"
    echo "---"
}

# Test all service health endpoints (all services from docker-compose.production.yml)
test_health "API Gateway" "http://${SERVER_IP}:${GATEWAY_PORT}/health"
test_health "Identity Service" "http://${SERVER_IP}:${IDENTITY_PORT}/health"
test_health "Category Service" "http://${SERVER_IP}:${CATEGORY_PORT}/health"
test_health "Inventory Service" "http://${SERVER_IP}:${INVENTORY_PORT}/health"
test_health "Cart Service" "http://${SERVER_IP}:${CART_PORT}/health"
test_health "Order Service" "http://${SERVER_IP}:${ORDER_PORT}/health"
test_health "Offer Service" "http://${SERVER_IP}:${OFFER_PORT}/health"
test_health "Notification Service" "http://${SERVER_IP}:${NOTIFICATION_PORT}/health"
test_health "User Profile Service" "http://${SERVER_IP}:${PROFILE_PORT}/health"

echo "=========================================="
echo "Health Check Testing Complete!"
echo "=========================================="
