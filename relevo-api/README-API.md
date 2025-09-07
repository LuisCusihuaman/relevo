# RELEVO API

## Database Setup

For database setup, use the unified script in the API root:

```bash
# Docker setup (recommended)
./setup-db.sh docker

# Manual Oracle setup
./setup-db.sh manual system/TuPass123@localhost:1521/XE

# See all options
./setup-db.sh help
```

## Running the Application

After database setup:

```bash
cd src/Relevo.Web
dotnet run --launch-profile https
```

## Database Schema

The database schema and seed data is located in `src/Relevo.Web/database-schema.sql`.

## Documentation

- [Database Setup Guide](src/Relevo.Web/README-DATABASE.md)
- [Docker Setup Guide](src/Relevo.Web/README-DOCKER.md)
