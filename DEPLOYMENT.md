# Production Deployment Guide

This guide explains how to deploy the GiftECommerce microservices to production using Docker Swarm.

## Prerequisites

- Linux server with Docker installed (minimum 8GB RAM recommended)
- Docker Swarm initialized
- Portainer installed (optional, for GUI management)
- Nginx Proxy Manager installed
- SQL Server accessible (external)
- Redis deployed separately (external)
- RabbitMQ deployed separately (external)

## Initial Setup

### 1. Initialize Docker Swarm (if not already done)

```bash
docker swarm init
```

### 2. Create the Overlay Network

```bash
# Create the default overlay network (shared across all projects)
docker network create --driver overlay --attachable default_overlay_network
```

**Note:** If the network already exists (from other projects), you can skip this step.

### 3. Set Environment Variables

Create a `.env` file on your server with the following variables:

```bash
# Docker Image Configuration
BACKEND_ENV=api_t1
TAG=202412151230  # Use timestamp from GitHub Actions (format: YYYYMMDDHHMM)

# Optional: Set unique prefixes to avoid conflicts with other projects on the same server
# STACK_NAME=gift-shop-team1
# SERVICE_PREFIX=gift-shop-team1
# VOLUME_PREFIX=gift-shop-team1

# SQL Server Configuration
SQL_SERVER_IP=YOUR_SQL_SERVER_IP
SQL_USER=sa
SQL_PASSWORD=YOUR_STRONG_SQL_PASSWORD
# Or use full connection string:
# SQL_CONNECTION_STRING=Server=YOUR_SQL_SERVER_IP;Database=IdentityDb;User Id=sa;Password=YOUR_STRONG_SQL_PASSWORD;TrustServerCertificate=True

# JWT Configuration
JWT_SECRET_KEY=YOUR_VERY_LONG_SECRET_KEY_HERE
JWT_ISSUER=gift-shop
JWT_AUDIENCE=gift-shop

# Email Configuration (SMTP)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password

# Redis Configuration (external)
REDIS_HOST=YOUR_REDIS_HOST
REDIS_PORT=6379
REDIS_PASSWORD=YOUR_STRONG_REDIS_PASSWORD

# RabbitMQ Configuration (external)
RABBITMQ_HOST=YOUR_RABBITMQ_HOST
RABBITMQ_VIRTUALHOST=/
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=YOUR_STRONG_RABBITMQ_PASSWORD

# Stripe Configuration (for OrderService)
STRIPE_SECRET_KEY=sk_live_your_stripe_secret_key
STRIPE_PUBLISHABLE_KEY=pk_live_your_stripe_publishable_key

# Optional: Custom Port Mappings (to avoid conflicts with other projects)
# GATEWAY_PORT=5000
# IDENTITY_PORT=5001
# CATEGORY_PORT=5002
# INVENTORY_PORT=5003
# CART_PORT=5005
# ORDER_PORT=5006
# OFFER_PORT=5007
# NOTIFICATION_PORT=5008
# PROFILE_PORT=5009
```

### 4. Prepare SQL Server Databases

Ensure your SQL Server has the following databases created:

```sql
CREATE DATABASE IdentityDb;
CREATE DATABASE [Ecommerce-CatalogDb];
CREATE DATABASE [Ecommerce-InventoryDb];
CREATE DATABASE CartServiceDb;
CREATE DATABASE OrderDb;
CREATE DATABASE UserProfileDb;
CREATE DATABASE [Ecommerce-OfferDb];
```

**Note:** Some database names use hyphens and require brackets in SQL Server.

## Deployment Steps

### 1. Set Environment Variables

```bash
# Load environment variables from .env file
export $(cat .env | xargs)

# Or set manually:
export BACKEND_ENV=api_t1
export TAG=202412151230  # Get this from GitHub Actions output

# Optional: Set unique prefixes to avoid conflicts
export STACK_NAME=gift-shop-team1
export SERVICE_PREFIX=gift-shop-team1
export VOLUME_PREFIX=gift-shop-team1
```

### 2. Pull the Latest Images (Optional)

Docker Swarm will pull images automatically, but you can pre-pull them:

```bash
# Set your environment
export BACKEND_ENV=api_t1
export TAG=202412151230

# Pull all images
docker pull giftshop90/giftshop:${BACKEND_ENV}-gateway-${TAG}
docker pull giftshop90/giftshop:${BACKEND_ENV}-identity-${TAG}
docker pull giftshop90/giftshop:${BACKEND_ENV}-category-${TAG}
docker pull giftshop90/giftshop:${BACKEND_ENV}-inventory-${TAG}
docker pull giftshop90/giftshop:${BACKEND_ENV}-cart-${TAG}
docker pull giftshop90/giftshop:${BACKEND_ENV}-order-${TAG}
docker pull giftshop90/giftshop:${BACKEND_ENV}-profile-${TAG}
docker pull giftshop90/giftshop:${BACKEND_ENV}-offer-${TAG}
docker pull giftshop90/giftshop:${BACKEND_ENV}-notification-${TAG}
```

### 3. Deploy the Stack

```bash
# Deploy with the production compose file
docker stack deploy -c docker-compose.production.yml ${STACK_NAME:-gift-shop} --with-registry-auth
```

**Note:** The `--with-registry-auth` flag is required if images are in a private registry.

### 4. Verify Deployment

```bash
# Check stack status
docker stack services ${STACK_NAME:-gift-shop}

# Check service logs (with prefix if set)
docker service logs ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-gateway
docker service logs ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-identityservice

# Or check all services
docker stack ps ${STACK_NAME:-gift-shop}
```

## Nginx Proxy Manager Configuration

### 1. Create Proxy Host for API Gateway

- **Domain Names**: `api.yourdomain.com`
- **Forward Hostname/IP**: `${SERVICE_PREFIX:-gift-shop}-gateway` (Docker service name with prefix)
- **Forward Port**: `${GATEWAY_PORT:-5000}` (host port, not container port)
- **Scheme**: `http`
- **Websocket Support**: Enabled

**Note:** Use the host port (default: 5000) not the container port (8080) when configuring Nginx Proxy Manager.

### 2. SSL Certificate

- Use Let's Encrypt to generate SSL certificate
- Enable "Force SSL" and "HTTP/2 Support"

## Service Endpoints

### Internal Service Communication

After deployment, services communicate via service names (with prefix if set):

- **Gateway**: `http://${SERVICE_PREFIX:-gift-shop}-gateway:8080` (internal)
- **Identity Service**: `http://${SERVICE_PREFIX:-gift-shop}-identityservice:8080` (internal)
- **Category Service**: `http://${SERVICE_PREFIX:-gift-shop}-categoryservice:8080` (internal)
- **Inventory Service**: `http://${SERVICE_PREFIX:-gift-shop}-inventoryservice:8080` (internal)
- **Cart Service**: `http://${SERVICE_PREFIX:-gift-shop}-cartservice:8080` (internal)
- **Order Service**: `http://${SERVICE_PREFIX:-gift-shop}-orderservice:8080` (internal)
- **User Profile Service**: `http://${SERVICE_PREFIX:-gift-shop}-userprofileservice:8080` (internal)
- **Offer Service**: `http://${SERVICE_PREFIX:-gift-shop}-offerservice:8080` (internal)
- **Notification Service**: `http://${SERVICE_PREFIX:-gift-shop}-notificationservice:8080` (internal)

### External Access (Host Ports)

Services are exposed on the following host ports (configurable via environment variables):

- **Gateway**: `http://YOUR_SERVER_IP:${GATEWAY_PORT:-5000}`
- **Identity Service**: `http://YOUR_SERVER_IP:${IDENTITY_PORT:-5001}`
- **Category Service**: `http://YOUR_SERVER_IP:${CATEGORY_PORT:-5002}`
- **Inventory Service**: `http://YOUR_SERVER_IP:${INVENTORY_PORT:-5003}`
- **Cart Service**: `http://YOUR_SERVER_IP:${CART_PORT:-5005}`
- **Order Service**: `http://YOUR_SERVER_IP:${ORDER_PORT:-5006}`
- **Offer Service**: `http://YOUR_SERVER_IP:${OFFER_PORT:-5007}`
- **Notification Service**: `http://YOUR_SERVER_IP:${NOTIFICATION_PORT:-5008}`
- **Profile Service**: `http://YOUR_SERVER_IP:${PROFILE_PORT:-5009}`

## Updating Services

### Update All Services

```bash
# Get the new tag from GitHub Actions output
export BACKEND_ENV=api_t1
export TAG=202412152000  # New timestamp

# Redeploy the stack (Docker Swarm will update all services)
docker stack deploy -c docker-compose.production.yml ${STACK_NAME:-gift-shop} --with-registry-auth
```

### Update a Single Service

```bash
# Set environment
export BACKEND_ENV=api_t1
export TAG=202412152000

# Update specific service
docker service update \
  --image giftshop90/giftshop:${BACKEND_ENV}-gateway-${TAG} \
  ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-gateway
```

## Monitoring

### View Service Status

```bash
# List all services
docker service ls

# View stack services
docker stack services ${STACK_NAME:-gift-shop}

# View stack tasks/containers
docker stack ps ${STACK_NAME:-gift-shop}
```

### View Logs

```bash
# Gateway logs
docker service logs ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-gateway --follow

# Identity service logs
docker service logs ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-identityservice --tail 100

# All services in stack
docker stack services ${STACK_NAME:-gift-shop} --format "{{.Name}}" | xargs -I {} docker service logs {} --tail 50
```

### Resource Usage

```bash
docker stats
```

## Troubleshooting

### Service Not Starting

```bash
# Check service status with full details
docker service ps ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-gateway --no-trunc

# Check logs
docker service logs ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-gateway

# Check service inspect
docker service inspect ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-gateway
```

### Network Issues

```bash
# Verify network exists
docker network ls | grep default_overlay_network

# Inspect network
docker network inspect default_overlay_network
```

### Database Connection Issues

- Verify SQL Server is accessible from Docker containers
- Check connection string in environment variables
- Ensure SQL Server allows remote connections
- Verify firewall rules allow port 1433
- Test connection: `docker run --rm mcr.microsoft.com/mssql-tools:latest /opt/mssql-tools/bin/sqlcmd -S YOUR_SQL_SERVER_IP -U sa -P 'YOUR_PASSWORD'`

### Redis Connection Issues

- Verify Redis is accessible from Docker containers
- Check Redis password in environment variables
- Verify firewall rules allow port 6379
- Test connection: `docker run --rm redis:alpine redis-cli -h YOUR_REDIS_HOST -p 6379 -a 'YOUR_PASSWORD' ping`

### RabbitMQ Connection Issues

- Verify RabbitMQ is accessible from Docker containers
- Check RabbitMQ credentials in environment variables
- Verify firewall rules allow ports 5672 (AMQP) and 15672 (Management UI)
- Management UI: `http://YOUR_RABBITMQ_HOST:15672`

## Scaling Services

### Scale a Service

```bash
# Scale gateway to 3 replicas
docker service scale ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-gateway=3
```

**Note:** Current configuration uses 1 replica per service (optimized for 8GB RAM server). Increase replicas only if you have sufficient resources.

### Update Replicas in Compose File

Edit `docker-compose.production.yml` (change `replicas: 1` to desired number) and redeploy:

```bash
docker stack deploy -c docker-compose.production.yml ${STACK_NAME:-gift-shop}
```

## Backup and Restore

### Backup Uploads Volume

```bash
# Backup uploads volume
docker run --rm \
  -v ${VOLUME_PREFIX:-gift-shop}-uploads:/data \
  -v $(pwd):/backup \
  alpine tar czf /backup/uploads-backup-$(date +%Y%m%d).tar.gz /data
```

### Backup SQL Server Databases

Use SQL Server backup tools or scripts:

```bash
# Example using sqlcmd
docker run --rm mcr.microsoft.com/mssql-tools:latest \
  /opt/mssql-tools/bin/sqlcmd \
  -S YOUR_SQL_SERVER_IP -U sa -P 'YOUR_PASSWORD' \
  -Q "BACKUP DATABASE IdentityDb TO DISK='/backup/IdentityDb.bak'"
```

**Note:** Redis and RabbitMQ are deployed separately and should be backed up using their respective backup procedures.

## Security Best Practices

1. **Use Docker Secrets** for sensitive data instead of environment variables
2. **Enable SSL/TLS** via Nginx Proxy Manager
3. **Use strong passwords** for all services (SQL Server, Redis, RabbitMQ)
4. **Limit network exposure** - only expose Gateway via Nginx Proxy Manager
5. **Regular updates** - keep images updated with latest security patches
6. **Monitor logs** for suspicious activity
7. **Use unique prefixes** when deploying multiple projects to avoid conflicts
8. **Resource limits** - configured to prevent resource exhaustion
9. **Firewall rules** - restrict access to only necessary ports
10. **Environment variables** - never commit `.env` files to version control

## Portainer Integration

If using Portainer:

1. Access Portainer UI
2. Navigate to "Stacks"
3. Click "Add Stack"
4. Name: `${STACK_NAME:-gift-shop}`
5. Upload `docker-compose.production.yml` file
6. Set environment variables in Portainer or use `.env` file
7. Deploy stack
8. Monitor services from Portainer dashboard

**Note:** Make sure to set `BACKEND_ENV` and `TAG` environment variables in Portainer before deploying.

## Multi-Project Deployment (Avoiding Conflicts)

When deploying multiple projects on the same server:

### Set Unique Prefixes

```bash
# Project 1 (Team 1)
export STACK_NAME=gift-shop-team1
export SERVICE_PREFIX=gift-shop-team1
export VOLUME_PREFIX=gift-shop-team1
export GATEWAY_PORT=5000
export IDENTITY_PORT=5001
# ... set other ports

# Project 2 (Team 2)
export STACK_NAME=gift-shop-team2
export SERVICE_PREFIX=gift-shop-team2
export VOLUME_PREFIX=gift-shop-team2
export GATEWAY_PORT=6000
export IDENTITY_PORT=6001
# ... set other ports
```

### Benefits

- **No service name conflicts** - Each project has unique service names
- **No volume conflicts** - Each project has unique volume names
- **No stack name conflicts** - Each deployment has unique stack name
- **Port isolation** - Each project uses different host ports

## Resource Allocation

Current configuration (optimized for 8GB RAM server):

- **Gateway**: 1G memory limit, 512M reservation, 1.0 CPU limit
- **Other services**: 512M memory limit, 256M reservation, 0.5 CPU limit each
- **Total memory limits**: ~5GB (leaves ~3GB for OS and other services)
- **Total CPU limits**: ~5.0 CPUs

Adjust resource limits in `docker-compose.production.yml` if needed.

## Support

For issues or questions, check:
- Service logs: `docker service logs ${STACK_NAME:-gift-shop}_${SERVICE_PREFIX:-gift-shop}-<service-name>`
- Stack status: `docker stack ps ${STACK_NAME:-gift-shop}`
- Network connectivity: `docker network inspect default_overlay_network`
- GitHub Actions: Check workflow output for deployment tag information

