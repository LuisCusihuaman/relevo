# 🐳 RELEVO Docker Setup Guide

This guide covers different ways to set up Oracle database for RELEVO using Docker containers.

## 📋 Docker Setup Options

### Option 1: Unified Script (Recommended) ⭐

**One-command setup:**
```bash
cd relevo-api
./setup-db.sh docker

# Or with custom container name
./setup-db.sh docker myoracle
```

**Done!** 🎉

**Note:** This replaces the old separate `docker-init.sh` script.

---

### Option 2: Manual Container Initialization

**1. Start Container:**
```bash
docker run -d --name xe11 \
  -p 1521:1521 \
  -e ORACLE_PASSWORD=TuPass123 \
  -v xe11_data:/u01/app/oracle/oradata \
  gvenzl/oracle-xe:11-slim
```

**2. Wait for Oracle to be ready:**
```bash
# Check if Oracle is ready
docker exec xe11 bash -c "echo 'SELECT 1 FROM DUAL;' | sqlplus -s system/TuPass123"
```

**3. Copy and execute SQL file:**
```bash
# Copy SQL file to container
docker cp database-schema.sql xe11:/tmp/

# Execute SQL file
docker exec -i xe11 bash -c "
sqlplus -s system/TuPass123 << EOF
@tmp/database-schema.sql
EXIT;
EOF
"
```

**4. Verify:**
```bash
docker exec -i xe11 bash -c "
sqlplus -s system/TuPass123 << EOF
SELECT 'UNITS: ' || COUNT(*) FROM UNITS
UNION ALL
SELECT 'PATIENTS: ' || COUNT(*) FROM PATIENTS
UNION ALL
SELECT 'SHIFTS: ' || COUNT(*) FROM SHIFTS;
EOF
"
```

---

### Option 3: Interactive Container Session

**1. Start and enter container:**
```bash
docker run -it --name xe11 \
  -p 1521:1521 \
  -e ORACLE_PASSWORD=TuPass123 \
  -v xe11_data:/u01/app/oracle/oradata \
  gvenzl/oracle-xe:11-slim \
  bash
```

**2. Inside container, run SQL manually:**
```bash
# Copy SQL file (from host)
docker cp database-schema.sql xe11:/tmp/

# Connect to SQL*Plus
sqlplus system/TuPass123

# Execute SQL file
SQL> @tmp/database-schema.sql

# Verify
SQL> SELECT COUNT(*) FROM UNITS;
SQL> SELECT COUNT(*) FROM PATIENTS;
SQL> EXIT;
```

---

## 🔧 Configuration

### Update Application Settings

After database initialization, update your `appsettings.json`:

```json
{
  "UseOracle": true,
  "Oracle": {
    "ConnectionString": "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE"
  }
}
```

Or for development, update `appsettings.Development.json`:
```json
{
  "UseOracle": true,
  "Oracle": {
    "ConnectionString": "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE"
  }
}
```

### Test Connection

```bash
# Start the application
cd src/Relevo.Web
dotnet run --launch-profile https

# Test endpoints
curl -k "https://localhost:57679/setup/units"
curl -k "https://localhost:57679/units/unit-1/patients"
```

---

## 📊 Database Content

The initialization creates:

- **🏥 3 Units**: UCI, Pediatría General, Pediatría Especializada
- **👥 8 Patients** with authentic Argentine names and Spanish medical data
- **⏰ 2 Shifts**: Mañana (07:00-15:00), Noche (19:00-07:00)
- **👨‍⚕️ 3 Medical Staff** with Argentine contact information
- **📋 3 Handover Sessions** with Spanish content and action items

### Sample Data in Spanish:
- **Diagnoses**: Exacerbación de Asma, Neumonía, Cuidados postoperatorios
- **Medications**: Salbutamol, Prednisona, Antibióticos
- **Medical Notes**: All in Spanish with proper medical terminology

---

## 🐛 Troubleshooting

### Container Won't Start
```bash
# Check Docker logs
docker logs xe11

# Remove and restart
docker rm -f xe11
docker run -d --name xe11 \
  -p 1521:1521 \
  -e ORACLE_PASSWORD=TuPass123 \
  -v xe11_data:/u01/app/oracle/oradata \
  gvenzl/oracle-xe:11-slim
```

### Oracle Not Ready
```bash
# Wait longer for Oracle initialization
sleep 60

# Check Oracle status
docker exec xe11 ps aux | grep oracle
```

### Connection Issues
```bash
# Test basic connectivity
docker exec xe11 bash -c "echo 'SELECT 1 FROM DUAL;' | sqlplus -s system/TuPass123"

# Check listener status
docker exec xe11 lsnrctl status
```

### SQL Execution Errors
```bash
# Check SQL syntax
docker exec xe11 sqlplus -s system/TuPass123 << EOF
SELECT * FROM USER_ERRORS;
EOF

# Re-run initialization
./docker-init.sh xe11
```

---

## 📋 Available Scripts

| Script | Purpose | Usage |
|--------|---------|--------|
| `setup-db.sh` | Unified setup script | `./setup-db.sh docker [container_name]` |
| `database-schema.sql` | Oracle schema and seed data | Referenced by setup script (in src/Relevo.Web/) |

---

## 🎯 Quick Start (Copy & Paste)

```bash
# One-command setup (does everything!)
cd relevo-api
./setup-db.sh docker

# Configure application (add to appsettings.json)
# {
#   "UseOracle": true,
#   "Oracle": {
#     "ConnectionString": "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE"
#   }
# }

# Start application
cd src/Relevo.Web
dotnet run --launch-profile https

# Test
curl -k "https://localhost:57679/setup/units"
```

---

## 💡 Why This Approach?

**✅ Advantages:**
- **Simple**: One command to set up everything
- **Reliable**: Handles Oracle startup timing automatically
- **Verified**: Tests connection before proceeding
- **Comprehensive**: Includes verification and error handling
- **Flexible**: Works with custom container names and configurations

**❌ Limitations:**
- Oracle XE 11g is old (2013), consider newer versions for production
- Requires Docker and sufficient resources
- Container-based (not suitable for all deployment scenarios)

---

**🎉 You're all set! The RELEVO database will be ready with authentic Argentine/Spanish medical data in minutes.**
