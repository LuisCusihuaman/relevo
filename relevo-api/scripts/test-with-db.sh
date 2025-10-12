#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

docker compose down -v
docker compose up -d

# wait for XE ready
until docker exec xe11 bash -c "echo 'exit' | sqlplus -s -L RELEVO_APP/TuPass123@//localhost:1521/XE" >/dev/null 2>&1; do
  echo "Waiting for Oracle..."; sleep 5
done

dotnet build
dotnet test


