# AI Demo — Restaurant Reservation Platform (ReserveIt)

A production-grade restaurant reservation platform (OpenTable-like) built with **Clean Architecture .NET 9 + Angular 19 + PostgreSQL**, fully containerized with Docker Compose.

---

## Quick Start (Docker)

```bash
# 1. Copy environment template
cp .env.example .env   # edit JWT_SECRET and POSTGRES_PASSWORD if desired

# 2. Start all services
docker compose up --build

# Services:
#   Frontend  → http://localhost:80
#   Backend   → http://localhost:5000
#   Swagger   → http://localhost:5000/swagger
#   Postgres  → localhost:5432
```

---

## Seeded Demo Accounts

| Role     | Email                   | Password       |
|----------|-------------------------|----------------|
| Admin    | admin@demo.com          | Admin123!      |
| Owner 1  | owner1@demo.com         | Owner123!      |
| Owner 2  | owner2@demo.com         | Owner123!      |
| Customer | alice@demo.com          | Customer123!   |
| Customer | bob@demo.com            | Customer123!   |
| Customer | carol@demo.com          | Customer123!   |

---

## Local Development

### Prerequisites
- .NET 9 SDK
- Node 20 + npm
- Docker (for PostgreSQL)

### Backend

```bash
# Start PostgreSQL
docker run -d --name pg \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=reservationdb \
  -p 5432:5432 postgres:16-alpine

# Run API
cd src/ReservationPlatform.API
dotnet run
# → http://localhost:5000/swagger
```

### Frontend

```bash
cd client
npm install
npm start          # or: npx ng serve
# → http://localhost:4200
```

---

## Architecture

```
ReservationPlatform.Domain          ← entities, enums, interfaces (no dependencies)
ReservationPlatform.Application     ← CQRS handlers, DTOs, validators (MediatR + FluentValidation)
ReservationPlatform.Infrastructure  ← EF Core + PostgreSQL, JWT, BCrypt
ReservationPlatform.API             ← Controllers, middleware, Swagger (ASP.NET Core 9)

client/                             ← Angular 19 SPA (standalone components, signals)
```

### Key Features
- **JWT auth** (15-min access token) + **refresh token rotation** with theft detection
- **Advisory lock** double-booking prevention (`pg_advisory_xact_lock` per table+date)
- **Availability algorithm**: 15-min slot generation with best-fit table selection
- **Role-based access**: Customer / Owner / Admin with functional guards
- **Rate limiting** on auth endpoints
- **Serilog** structured logging (console + file)

---

## API Examples

### Auth
```bash
# Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Jane","lastName":"Doe","email":"jane@example.com","password":"Password1!"}'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"Admin123!"}'
```

### Restaurants
```bash
# List (paginated, filterable)
curl "http://localhost:5000/api/restaurants?city=Boston&page=1&pageSize=10"

# Get restaurant detail
curl "http://localhost:5000/api/restaurants/{id}"

# Check availability
curl "http://localhost:5000/api/restaurants/{id}/availability?date=2026-03-15&partySize=2"
```

### Reservations
```bash
# Create reservation (requires auth)
curl -X POST http://localhost:5000/api/reservations \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "restaurantId": "{id}",
    "tableId": "{tableId}",
    "reservationDate": "2026-03-15",
    "slotTime": "19:00",
    "partySize": 2
  }'

# My reservations
curl "http://localhost:5000/api/reservations?page=1&pageSize=10" \
  -H "Authorization: Bearer {token}"

# Cancel
curl -X PUT http://localhost:5000/api/reservations/{id}/cancel \
  -H "Authorization: Bearer {token}" \
  -d '{"reason": "Change of plans"}'
```

### Admin
```bash
# Approve restaurant
curl -X POST http://localhost:5000/api/admin/restaurants/{id}/approve \
  -H "Authorization: Bearer {adminToken}"
```

---

## Project Structure

```
aidemo/
├── src/
│   ├── ReservationPlatform.Domain/
│   ├── ReservationPlatform.Application/
│   ├── ReservationPlatform.Infrastructure/
│   └── ReservationPlatform.API/
├── client/                    ← Angular 19 frontend
├── docker-compose.yml
├── .env.example
└── README.md
```

---

## Existing Projects

This repo also contains:
- **backend/backend.API/** — RouteBD route finder API (HERE Maps + ASP.NET)
- **frontend/** — RouteBD Angular map app (MapLibre + OpenStreetMap)
