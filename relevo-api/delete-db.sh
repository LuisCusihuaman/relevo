#!/bin/bash

# ========================================
# RELEVO Database Cleanup Script
# ========================================
# Removes Oracle XE container and optionally data volume
# Usage: ./delete-db.sh [container_name] [--yes] [--data]

# Default values
CONTAINER_NAME="xe11"
AUTO_CONFIRM="false"
DELETE_DATA="true"  # Delete data volume by default

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --yes|-y)
            AUTO_CONFIRM="true"
            shift
            ;;
        --keep-data|-k)
            DELETE_DATA="false"
            shift
            ;;
        --help|-h)
            echo "============================================="
            echo "🗑️  RELEVO Database Cleanup"
            echo "============================================="
            echo ""
            echo "Usage:"
            echo "  ./delete-db.sh [container_name] [options]"
            echo ""
            echo "Options:"
            echo "  --yes, -y        Skip confirmation prompts"
            echo "  --keep-data, -k  Preserve data volume (default: delete)"
            echo "  --help, -h       Show this help"
            echo ""
            echo "Examples:"
            echo "  ./delete-db.sh                    # Delete xe11 container + data"
            echo "  ./delete-db.sh myoracle          # Delete custom container + data"
            echo "  ./delete-db.sh --keep-data       # Delete container, keep data"
            echo "  ./delete-db.sh --yes             # Skip confirmation"
            echo ""
            exit 0
            ;;
        -*)
            echo "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
        *)
            # If it's not an option and we haven't set container name yet
            if [ "$CONTAINER_NAME" = "xe11" ]; then
                CONTAINER_NAME="$1"
            else
                echo "Unknown argument: $1"
                echo "Use --help for usage information"
                exit 1
            fi
            shift
            ;;
    esac
done

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

# Check if container exists
check_container() {
    if docker ps -a --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"; then
        return 0  # Container exists
    else
        return 1  # Container doesn't exist
    fi
}

# Get container status
get_container_status() {
    docker ps -a --filter name="${CONTAINER_NAME}" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
}

# Stop container if running
stop_container() {
    log_info "Checking if container '${CONTAINER_NAME}' is running..."

    if docker ps --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"; then
        log_warning "Container '${CONTAINER_NAME}' is running. Stopping it..."
        if docker stop "${CONTAINER_NAME}"; then
            log_success "Container '${CONTAINER_NAME}' stopped successfully"
        else
            log_error "Failed to stop container '${CONTAINER_NAME}'"
            return 1
        fi
    else
        log_info "Container '${CONTAINER_NAME}' is not running"
    fi

    return 0
}

# Remove container
remove_container() {
    log_info "Removing container '${CONTAINER_NAME}'..."

    if docker rm "${CONTAINER_NAME}"; then
        log_success "Container '${CONTAINER_NAME}' removed successfully"
        return 0
    else
        log_error "Failed to remove container '${CONTAINER_NAME}'"
        return 1
    fi
}

# Remove data volume
remove_data_volume() {
    local volume_name="${CONTAINER_NAME}_data"
    log_info "Removing data volume '${volume_name}'..."

    if docker volume ls --format "table {{.Name}}" | grep -q "^${volume_name}$"; then
        if docker volume rm "${volume_name}"; then
            log_success "Data volume '${volume_name}' removed successfully"
            return 0
        else
            log_error "Failed to remove data volume '${volume_name}'"
            return 1
        fi
    else
        log_info "Data volume '${volume_name}' does not exist"
        return 0
    fi
}

# Clean up database tables
cleanup_database() {
    local container_name="$1"

    # Check if container exists and is running
    if ! docker ps --format "table {{.Names}}" | grep -q "^${container_name}$"; then
        log_info "Container '${container_name}' is not running, skipping database cleanup"
        return 0
    fi

    log_info "Cleaning up database tables..."

    # Create cleanup SQL script
    cat > /tmp/relevo-cleanup.sql << 'EOF'
-- RELEVO Database Cleanup Script
-- Drops all tables in reverse dependency order

SET ECHO ON
SET FEEDBACK ON
SET SERVEROUTPUT ON
WHENEVER SQLERROR CONTINUE

DECLARE
    table_count NUMBER;
    sql_stmt VARCHAR2(500);
BEGIN
    -- Drop tables in reverse dependency order
    FOR table_rec IN (
        SELECT table_name
        FROM user_tables
        WHERE table_name IN (
            'HANDOVER_ACTIVITY_LOG',
            'SECTION_TEMPLATES',
            'IPASS_TEMPLATES',
            'HANDOVER_MENTIONS',
            'HANDOVER_MESSAGES',
            'HANDOVER_CONTINGENCY',
            'HANDOVER_CHECKLISTS',
            'USER_SESSIONS',
            'USER_PREFERENCES',
            'USERS',
            'HANDOVER_SYNC_STATUS',
            'HANDOVER_SECTIONS',
            'HANDOVER_PARTICIPANTS',
            'HANDOVER_ACTION_ITEMS',
            'HANDOVERS',
            'CONTRIBUTORS',
            'USER_ASSIGNMENTS',
            'PATIENTS',
            'SHIFTS',
            'UNITS'
        )
        ORDER BY table_name DESC
    ) LOOP
        sql_stmt := 'DROP TABLE ' || table_rec.table_name || ' CASCADE CONSTRAINTS';
        BEGIN
            EXECUTE IMMEDIATE sql_stmt;
            DBMS_OUTPUT.PUT_LINE('Dropped table: ' || table_rec.table_name);
        EXCEPTION
            WHEN OTHERS THEN
                IF SQLCODE != -942 THEN -- Table doesn't exist error
                    DBMS_OUTPUT.PUT_LINE('Error dropping ' || table_rec.table_name || ': ' || SQLERRM);
                END IF;
        END;
    END LOOP;

    -- Drop sequences
    FOR seq_rec IN (
        SELECT sequence_name
        FROM user_sequences
        WHERE sequence_name = 'CONTRIBUTORS_SEQ'
    ) LOOP
        sql_stmt := 'DROP SEQUENCE ' || seq_rec.sequence_name;
        BEGIN
            EXECUTE IMMEDIATE sql_stmt;
            DBMS_OUTPUT.PUT_LINE('Dropped sequence: ' || seq_rec.sequence_name);
        EXCEPTION
            WHEN OTHERS THEN
                IF SQLCODE != -2289 THEN -- Sequence doesn't exist error
                    DBMS_OUTPUT.PUT_LINE('Error dropping sequence ' || seq_rec.sequence_name || ': ' || SQLERRM);
                END IF;
        END;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Database cleanup completed successfully');
END;
/

-- Verify cleanup
SELECT 'Remaining tables: ' || COUNT(*) FROM user_tables WHERE table_name LIKE 'HANDOVER_%' OR table_name LIKE 'USER_%' OR table_name IN ('UNITS', 'SHIFTS', 'PATIENTS', 'CONTRIBUTORS');

EXIT;
EOF

    # Execute cleanup script in container
    if docker exec -i "$container_name" bash -c "
        export ORACLE_HOME=/u01/app/oracle/product/11.2.0/xe
        export ORACLE_SID=XE
        export PATH=\$ORACLE_HOME/bin:\$PATH
        sqlplus -s system/TuPass123 << 'EOF'
        @/tmp/relevo-cleanup.sql
        EXIT;
        EOF
    " > /tmp/cleanup-output.log 2>&1; then
        log_success "Database tables cleaned up successfully"

        # Show cleanup summary
        if grep -q "Database cleanup completed successfully" /tmp/cleanup-output.log; then
            echo "  - All RELEVO tables dropped successfully"
        fi

        # Check for remaining tables
        if grep -q "Remaining tables:" /tmp/cleanup-output.log; then
            remaining=$(grep "Remaining tables:" /tmp/cleanup-output.log | sed 's/.*Remaining tables: *//' | tr -d ' ')
            if [ "$remaining" = "0" ]; then
                echo "  - No RELEVO tables remaining"
            else
                echo "  - $remaining RELEVO tables still exist (may include system tables)"
            fi
        else
            echo "  - Unable to verify remaining tables"
        fi
    else
        log_warning "Database cleanup completed with warnings"
        echo "  Check /tmp/cleanup-output.log for details"
    fi

    # Clean up temporary files
    rm -f /tmp/relevo-cleanup.sql
}

# Clean up Docker images (optional)
cleanup_images() {
    log_info "Cleaning up unused Docker images..."

    if docker image prune -f > /dev/null 2>&1; then
        log_success "Docker images cleaned up"
    else
        log_warning "Could not clean up Docker images (this is usually fine)"
    fi
}

# Confirmation prompt
confirm_deletion() {
    if [ "$AUTO_CONFIRM" = "true" ]; then
        return 0
    fi

    echo ""
    echo "============================================="
    echo "🗑️  RELEVO Database Cleanup Confirmation"
    echo "============================================="
    echo ""
    echo "The following will be removed:"
    echo "• Container: ${CONTAINER_NAME}"

    if [ "$DELETE_DATA" = "true" ]; then
        echo "• Data Volume: ${CONTAINER_NAME}_data"
        echo ""
        log_warning "⚠️  WARNING: All database data will be permanently lost!"
        log_info "Use --keep-data to preserve the data volume if needed."
    else
        echo ""
        log_info "Data volume will be preserved (--keep-data specified)"
    fi

    echo ""
    read -p "Are you sure you want to continue? (y/N): " -n 1 -r
    echo ""

    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_info "Operation cancelled by user"
        exit 0
    fi
}

# Main execution
main() {
    echo "============================================="
    echo "🗑️  RELEVO Database Cleanup"
    echo "============================================="
    echo ""
    log_info "Target container: ${CONTAINER_NAME}"

    # Check if container exists
    if ! check_container; then
        log_error "Container '${CONTAINER_NAME}' does not exist"
        echo ""
        log_info "Available containers:"
        docker ps -a --format "table {{.Names}}\t{{.Image}}\t{{.Status}}" | grep -E "(oracle|xe)" || echo "  No Oracle containers found"
        echo ""
        log_info "Use: ./delete-db.sh [container_name]"
        exit 1
    fi

    # Show current status
    echo ""
    log_info "Current container status:"
    get_container_status
    echo ""

    # Confirm deletion
    confirm_deletion

    echo ""
    log_info "Starting cleanup process..."

    # Clean up database tables first (while container is still running)
    if ! cleanup_database "$CONTAINER_NAME"; then
        log_warning "Database cleanup failed, but continuing with container removal..."
    fi

    # Stop container
    if ! stop_container; then
        exit 1
    fi

    # Remove container
    if ! remove_container; then
        exit 1
    fi

    # Remove data volume if requested
    if [ "$DELETE_DATA" = "true" ]; then
        if ! remove_data_volume; then
            log_warning "Container was removed but data volume removal failed"
        fi
    fi

    # Clean up images
    cleanup_images

    echo ""
    log_success "🎉 Database cleanup completed successfully!"
    echo ""
    echo "📝 Next steps:"
    echo "• To recreate the database: ./setup-db.sh"
    echo "• To start fresh: ./setup-db.sh --yes"
    echo ""

    if [ "$DELETE_DATA" = "true" ]; then
        log_success "Data volume '${CONTAINER_NAME}_data' was also removed"
    else
        log_info "Note: Data volume '${CONTAINER_NAME}_data' was preserved"
        log_info "Use './delete-db.sh ${CONTAINER_NAME}' (without --keep-data) to remove it next time"
    fi
}

# Run main function
main "$@"
