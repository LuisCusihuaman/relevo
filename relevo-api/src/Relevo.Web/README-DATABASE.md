# 🗄️ RELEVO Database Setup Guide

This guide explains how to set up and initialize the database for the RELEVO application.

## 📋 Overview

The RELEVO application supports two database modes:
- **SQLite** (Default) - For development and testing
- **Oracle** - For production environments

## 🗂️ Database Files

### 📄 `database-schema.sql`
- Contains the complete Oracle database schema
- Includes all tables, indexes, and seed data
- **Language**: Spanish (Argentine medical terminology)
- **Content**: 8 patients, 3 units, 2 shifts, handovers, and action items

### 🐚 `init-database.sh`
- Shell script for manual database initialization
- Works with Oracle databases
- Can be used for development, testing, and CI/CD

## 🚀 Quick Start

### For Development (SQLite - Default)
```bash
# The app automatically uses SQLite with built-in test data
cd src/Relevo.Web
dotnet run --launch-profile https
```

### For Docker Oracle Setup (Recommended) ⭐
```bash
# One-liner setup (container + database + data)
cd relevo-api
./setup-db.sh docker

# Or with custom container name
./setup-db.sh docker myoracle

# Then configure and run
cd src/Relevo.Web
dotnet run --launch-profile https
```

### For Manual Oracle Setup
```bash
# Direct Oracle connection
cd relevo-api
./setup-db.sh manual system/TuPass123@localhost:1521/XE

# Then configure and run
cd src/Relevo.Web
dotnet run --launch-profile https
```

### For Interactive Setup
```bash
# See all options
cd relevo-api
./setup-db.sh help
```

## 📊 Database Schema

### Tables Created:
- **UNITS** - Hospital units (UCI, Pediatría General, Pediatría Especializada)
- **SHIFTS** - Work shifts (Mañana, Noche)
- **PATIENTS** - Patient information with medical data
- **CONTRIBUTORS** - Medical staff information
- **HANDOVERS** - Patient handover sessions
- **HANDOVER_ACTION_ITEMS** - Action items for handovers

### Sample Data (Spanish):
- **8 Patients** with authentic Argentine names
- **Medical diagnoses** in Spanish
- **Hospital staff** with Argentine contact information
- **Complete handover workflows** with Spanish content

## 🔧 Manual Database Operations

### Initialize Database
```bash
# Default Oracle connection
./init-database.sh

# Custom connection string
./init-database.sh myuser/mypassword@myhost:1521/SID
```

### Check Database Status
The script will:
- Verify Oracle client tools are installed
- Check if SQL script exists
- Execute all SQL statements
- Report success/failure

### Troubleshooting
```bash
# Check Oracle connectivity
sqlplus system/oracle@localhost:1521/XE

# Verify script permissions
chmod +x init-database.sh

# Check Oracle environment
echo $ORACLE_HOME
echo $LD_LIBRARY_PATH
```

## ⚙️ Configuration

### appsettings.json
```json
{
  "UseOracle": false,           // Set to true for Oracle
  "UseOracleForSetup": false,   // Alternative Oracle flag
  "Oracle": {
    "ConnectionString": "User Id=system;Password=oracle;Data Source=localhost:1521/XE"
  }
}
```

### Development Settings
For development, use `appsettings.Development.json`:
```json
{
  "UseOracle": true,
  "Oracle": {
    "ConnectionString": "User Id=system;Password=oracle;Data Source=localhost:1521/XE"
  }
}
```

## 📈 Data Content

### 🏥 Units (3 total)
- **UCI** - Unidad de Cuidados Intensivos
- **Pediatría General** - General Pediatrics
- **Pediatría Especializada** - Specialized Pediatrics

### 👥 Patients (8 total)
- **Unit 1 (UCI)**: 3 patients
- **Unit 2 (Pediatría General)**: 3 patients
- **Unit 3 (Pediatría Especializada)**: 2 patients

### ⏰ Shifts (2 total)
- **Mañana**: 07:00 - 15:00
- **Noche**: 19:00 - 07:00

### 👨‍⚕️ Medical Staff (3 doctors)
- Dra. María García
- Dr. Carlos López
- Dra. Ana Martínez

## 🔄 Migration from SQLite to Oracle

1. **Backup current data** (if any)
2. **Run database initialization**:
   ```bash
   ./init-database.sh
   ```
3. **Update configuration**:
   ```json
   {
     "UseOracle": true
   }
   ```
4. **Restart application**
5. **Verify data migration**

## 🐛 Troubleshooting

### Common Issues:

**"sqlplus not found"**
```bash
# Install Oracle Instant Client
# macOS: brew install instantclient-sqlplus
# Ubuntu: apt-get install oracle-instantclient-sqlplus
```

**"ORA-12541: TNS:no listener"**
- Ensure Oracle database is running
- Check connection string
- Verify TNS listener is active

**"Table already exists"**
- The script handles existing tables gracefully
- For clean reinstall, drop tables first or use different schema

**Permission Issues**
```bash
# Make script executable
chmod +x init-database.sh

# Check file permissions
ls -la init-database.sh
```

## 📋 Verification Queries

After initialization, verify data:
```sql
-- Check table counts
SELECT 'UNITS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM UNITS
UNION ALL
SELECT 'PATIENTS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM PATIENTS
UNION ALL
SELECT 'SHIFTS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM SHIFTS;
```

## 🎯 Best Practices

1. **Always backup** before running initialization
2. **Test in development** before production deployment
3. **Use environment-specific** connection strings
4. **Monitor logs** during initialization
5. **Verify data integrity** after setup

## 📋 Available Scripts

| Script | Purpose | Usage |
|--------|---------|--------|
| `setup-db.sh` | **Unified setup script** ⭐ | `./setup-db.sh [docker|manual] [options]` |
| `database-schema.sql` | Oracle schema and seed data | Referenced by setup script (in src/Relevo.Web/) |

---

**Note**: The `database-schema.sql` contains Spanish/Argentine medical data suitable for healthcare applications in Argentina and Spanish-speaking countries.
