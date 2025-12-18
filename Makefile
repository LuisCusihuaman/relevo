#colima

colima:
	colima start --arch x86_64 --memory 2 --cpu 1
	docker context use colima
	brew upgrade colima lima
	brew install lima-additional-guestagents
	dotnet dev-certs https --trust

db:
	cd relevo-api && docker compose up -d
	@echo "Esperando a que Oracle esté listo..."
	@sleep 10
	@echo "Inicializando base de datos..."
	@cd relevo-api && docker exec -i xe11 sqlplus -s RELEVO_APP/TuPass123@localhost:1521/XE < src/Relevo.Infrastructure/Data/Sql/02-indexes.sql 2>/dev/null || true
	@cd relevo-api && docker exec -i xe11 sqlplus -s RELEVO_APP/TuPass123@localhost:1521/XE < src/Relevo.Infrastructure/Data/Sql/04-seed-basic.sql
	@echo "Base de datos inicializada correctamente"

db-init:
	@echo "Inicializando base de datos..."
	@cd relevo-api && docker exec -i xe11 sqlplus -s RELEVO_APP/TuPass123@localhost:1521/XE < src/Relevo.Infrastructure/Data/Sql/02-indexes.sql 2>/dev/null || true
	@cd relevo-api && docker exec -i xe11 sqlplus -s RELEVO_APP/TuPass123@localhost:1521/XE < src/Relevo.Infrastructure/Data/Sql/04-seed-basic.sql
	@echo "Base de datos inicializada correctamente"

backend:
	cd relevo-api/src/Relevo.Web && dotnet run --launch-profile https

frontend:
	cd relevo-frontend && pnpm install && pnpm run setup && pnpm run dev
