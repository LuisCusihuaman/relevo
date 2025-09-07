#!/bin/bash

# ========================================
# UNIFIED RELEVO Database Setup Script
# ========================================
# Supports both Docker and manual Oracle setups
# Usage: ./setup-db.sh                          # Complete setup (default)
#        ./setup-db.sh [docker|init|manual] [container_name] [connection_string]

MODE="${1:-}"
CONTAINER_NAME="${2:-xe11}"
ORACLE_CONN="${3:-system/TuPass123@localhost:1521/XE}"

# Special mode for existing containers
if [ "$1" = "init" ]; then
    MODE="init"
    CONTAINER_NAME="${2:-xe11}"
fi

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

show_help() {
    echo "============================================="
    echo "🗄️  RELEVO Database Setup"
    echo "============================================="
    echo ""
    echo "Usage:"
    echo "  ./setup-db.sh                           # Complete setup (default)"
    echo "  ./setup-db.sh docker [container_name]   # Docker setup only"
    echo "  ./setup-db.sh init [container_name]     # Initialize existing container"
    echo "  ./setup-db.sh manual [connection_string] # Manual Oracle setup"
    echo ""
    echo "Examples:"
    echo "  ./setup-db.sh                           # Complete setup: Docker + schema (default)"
    echo "  ./setup-db.sh docker myoracle          # Docker setup only"
    echo "  ./setup-db.sh init xe11                # Initialize existing container"
    echo "  ./setup-db.sh manual system/pass@host:1521/SID  # Manual setup"
    echo ""
    echo "Current mode: $MODE"
    echo "Container: $CONTAINER_NAME"
    echo "Connection: $ORACLE_CONN"
}

# Docker setup function
setup_docker() {
    local silent_mode="${1:-false}"

    echo "🐳 Setting up RELEVO database with Docker..."
    echo ""

    # Check if container already exists
    if docker ps -a --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"; then
        log_warning "Container '$CONTAINER_NAME' already exists"

        # Check if it's running
        if docker ps --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"; then
            log_info "Container is running, proceeding with initialization..."
        else
            log_info "Starting existing container..."
            docker start "$CONTAINER_NAME"
        fi
    else
        log_info "Creating new Oracle XE container..."
        docker run -d --name "$CONTAINER_NAME" \
          -p 1521:1521 \
          -e ORACLE_PASSWORD=TuPass123 \
          -v "${CONTAINER_NAME}_data:/u01/app/oracle/oradata" \
          gvenzl/oracle-xe:11-slim
    fi

    log_success "Oracle XE container '$CONTAINER_NAME' started!"

    # Only show user instructions if not in silent mode
    if [ "$silent_mode" != "true" ]; then
        echo ""
        echo "⏳ Oracle database initialization takes 3-5 minutes..."
        echo ""
        echo "📋 Next steps:"
        echo "1. Wait 3-5 minutes for Oracle to fully initialize"
        echo "2. Initialize the database:"
        echo "   ./setup-db.sh init $CONTAINER_NAME"
        echo ""
        echo "Or check the container status:"
        echo "  docker logs $CONTAINER_NAME"
        echo ""
        echo "🔄 Once Oracle is ready, run:"
        echo "  ./setup-db.sh init $CONTAINER_NAME"
    fi
}

# Initialize existing container function
init_container() {
    echo "🔄 Initializing existing RELEVO container '$CONTAINER_NAME'..."
    echo ""

    # Check if container exists and is running
    if ! docker ps --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"; then
        if docker ps -a --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"; then
            log_warning "Container '$CONTAINER_NAME' exists but is not running"
            log_info "Starting container..."
            docker start "$CONTAINER_NAME"
            sleep 10
        else
            log_error "Container '$CONTAINER_NAME' does not exist"
            echo "Create it first with:"
            echo "  docker run -d --name $CONTAINER_NAME -p 1521:1521 -e ORACLE_PASSWORD=TuPass123 -v ${CONTAINER_NAME}_data:/u01/app/oracle/oradata gvenzl/oracle-xe:11-slim"
            exit 1
        fi
    fi

    # Test connection
    log_info "Testing Oracle connection..."
    if ! docker exec "$CONTAINER_NAME" bash -c "
        export ORACLE_HOME=/u01/app/oracle/product/11.2.0/xe
        export ORACLE_SID=XE
        export PATH=\$ORACLE_HOME/bin:\$PATH
        echo 'SELECT 1 FROM DUAL;' | sqlplus -s system/TuPass123 2>/dev/null | grep -q '1'
    " >/dev/null 2>&1; then
        log_error "Cannot connect to Oracle database in container '$CONTAINER_NAME'"
        echo "The container might still be initializing. Try waiting a few more minutes."
        echo "Check container logs: docker logs $CONTAINER_NAME"
        exit 1
    fi

    log_success "Oracle connection successful!"

    # Copy and execute SQL file
    log_info "Copying database schema..."
    docker cp "src/Relevo.Web/database-schema.sql" "$CONTAINER_NAME:/tmp/"

    log_info "Executing database initialization..."

    # Retry schema initialization up to 3 times in case of connection issues
    local retry_count=0
    local max_retries=3

    while [ $retry_count -lt $max_retries ]; do
        log_info "Schema initialization attempt $((retry_count + 1))/$max_retries..."

        if docker exec -i "$CONTAINER_NAME" bash -c "
export ORACLE_HOME=/u01/app/oracle/product/11.2.0/xe
export ORACLE_SID=XE
export PATH=\$ORACLE_HOME/bin:\$PATH

echo 'Executing database schema...'
sqlplus -s system/TuPass123@localhost:1521/XE << EOF
SET ECHO ON
SET FEEDBACK ON
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE
@/tmp/database-schema.sql
EXIT;
EOF
"; then
            schema_success=true
            break
        else
            retry_count=$((retry_count + 1))
            if [ $retry_count -lt $max_retries ]; then
                log_warning "Schema initialization failed, retrying in 10 seconds..."
                sleep 10
            fi
        fi
    done

    if [ "$schema_success" != "true" ]; then
        log_error "Schema initialization failed after $max_retries attempts"
        return 1
    fi

    # Only continue if schema was successful
    if [ "$schema_success" = "true" ]; then
        log_success "Database initialization completed!"

        # Verify
        echo ""
        log_info "Verification - Table counts:"
        if docker exec -i "$CONTAINER_NAME" bash -c "
export ORACLE_HOME=/u01/app/oracle/product/11.2.0/xe
export ORACLE_SID=XE
export PATH=\$ORACLE_HOME/bin:\$PATH
sqlplus -s system/TuPass123@localhost:1521/XE << 'EOF'
SET PAGESIZE 0
SET FEEDBACK OFF
SELECT 'UNITS: ' || COUNT(*) FROM UNITS
UNION ALL
SELECT 'PATIENTS: ' || COUNT(*) FROM PATIENTS
UNION ALL
SELECT 'SHIFTS: ' || COUNT(*) FROM SHIFTS;
EXIT;
EOF
"; then
            echo "Verification completed successfully"
        else
            echo "Note: Verification may show errors if tables don't exist yet"
        fi

        echo ""
        log_success "🎉 RELEVO database is ready!"
        echo ""
        echo "📝 Next steps:"
        echo "1. Update appsettings.json:"
        echo '   { "UseOracle": true, "Oracle": { "ConnectionString": "User Id=system;Password=TuPass123;Data Source=localhost:1521/XE" } }'
        echo "2. Run: cd src/Relevo.Web && dotnet run --launch-profile https"
    fi
}

# Manual setup function
setup_manual() {
    echo "🔧 Setting up RELEVO database manually..."
    echo "Connection: $ORACLE_CONN"
    echo ""

    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    SQL_SCRIPT="$SCRIPT_DIR/src/Relevo.Web/database-schema.sql"

    # Check if sqlplus is available
    if ! command -v sqlplus &> /dev/null; then
        log_error "sqlplus not found in PATH"
        echo "Please install Oracle client tools or use Docker mode:"
        echo "  ./setup-db.sh docker"
        exit 1
    fi

    # Check if SQL script exists
    if [ ! -f "$SQL_SCRIPT" ]; then
        log_error "SQL script not found: $SQL_SCRIPT"
        exit 1
    fi

    # Test connection
    log_info "Testing Oracle connection..."
    if ! sqlplus -S "$ORACLE_CONN" << EOF >/dev/null 2>&1
SELECT 1 FROM DUAL;
EXIT;
EOF
    then
        log_error "Cannot connect to Oracle database: $ORACLE_CONN"
        echo ""
        echo "Please check:"
        echo "  1. Oracle database is running"
        echo "  2. Connection string is correct"
        echo "  3. Oracle client tools are properly configured"
        exit 1
    fi

    log_success "Oracle connection successful"

    # Execute SQL
    log_info "Executing database initialization..."
    if sqlplus -S "$ORACLE_CONN" @"$SQL_SCRIPT" > /tmp/relevo-setup.log 2>&1; then
        log_success "Database initialization completed!"

        # Quick verification
        log_info "Verification - checking tables..."
        sqlplus -S "$ORACLE_CONN" << 'EOF'
SET PAGESIZE 0
SET FEEDBACK OFF
SELECT 'UNITS: ' || COUNT(*) FROM UNITS
UNION ALL
SELECT 'PATIENTS: ' || COUNT(*) FROM PATIENTS
UNION ALL
SELECT 'SHIFTS: ' || COUNT(*) FROM SHIFTS;
EXIT;
EOF

        echo ""
        log_success "🎉 RELEVO database is ready!"
        echo ""
        echo "📝 Next steps:"
        echo "1. Update appsettings.json:"
        echo '   { "UseOracle": true, "Oracle": { "ConnectionString": "'$ORACLE_CONN'" } }'
        echo "2. Run: dotnet run --launch-profile https"
        echo "3. Log file: /tmp/relevo-setup.log"
    else
        log_error "Database initialization failed!"
        echo "Check the log file: /tmp/relevo-setup.log"
        exit 1
    fi
}

# Complete setup function (docker + init)
setup_complete() {
    echo "🚀 Starting complete RELEVO database setup..."
    echo ""

    # Step 1: Setup Docker container (silent mode)
    setup_docker true

    echo ""
    echo "⏳ Waiting for Oracle to be fully ready (this may take 4-6 minutes)..."
    echo "   We'll check every 15 seconds until Oracle is ready."
    echo ""

    # Step 2: Wait for Oracle to be ready
    local max_attempts=40  # 40 attempts = ~10 minutes (more time for safety)
    local attempt=1

    while [ $attempt -le $max_attempts ]; do
        echo -ne "\r   Checking Oracle status (attempt $attempt/$max_attempts)..."

        if docker exec "$CONTAINER_NAME" bash -c "
            export ORACLE_HOME=/u01/app/oracle/product/11.2.0/xe
            export ORACLE_SID=XE
            export PATH=\$ORACLE_HOME/bin:\$PATH
            # Try comprehensive check first (ALL_USERS requires full service)
            if echo 'SELECT COUNT(*) FROM ALL_USERS;' | sqlplus -s system/TuPass123@localhost:1521/XE 2>/dev/null | grep -q '^[0-9]'; then
                exit 0
            else
                # Fallback to basic connectivity check
                echo 'SELECT 1 FROM DUAL;' | sqlplus -s system/TuPass123 2>/dev/null | grep -q '1'
            fi
        " >/dev/null 2>&1; then
            echo "" # New line after the progress indicator
            log_success "Oracle is ready!"

            # Give 10 seconds for Oracle to fully stabilize before schema initialization
            log_info "Waiting 10 seconds for Oracle to fully stabilize..."
            sleep 10
            break
        else
            if [ $attempt -eq $max_attempts ]; then
                echo "" # New line after the progress indicator
                log_error "Oracle failed to start after $max_attempts attempts"
                log_info "You can try running the initialization manually:"
                log_info "  ./setup-db.sh init $CONTAINER_NAME"
                log_info "Or check container logs: docker logs $CONTAINER_NAME"
                exit 1
            fi

            sleep 15
            ((attempt++))
        fi
    done

    echo ""
    echo "🔄 Now initializing database schema..."
    echo ""

    # Step 3: Initialize database
    init_container
}

# Main logic
case "$MODE" in
    "docker")
        setup_docker
        ;;
    "init")
        init_container
        ;;
    "manual")
        setup_manual
        ;;
    "help"|"-h"|"--help")
        show_help
        exit 0
        ;;
    "")
        # Default behavior: complete setup
        setup_complete
        ;;
    *)
        log_error "Invalid mode: $MODE"
        echo ""
        show_help
        exit 1
        ;;
esac
