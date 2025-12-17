#colima

colima:
	colima start --arch x86_64 --memory 2 --cpu 1
	docker context use colima
	brew upgrade colima lima
	brew install lima-additional-guestagents
	dotnet dev-certs https --trust

db:
	cd relevo-api && docker compose up -d

backend:
	cd relevo-api/src/Relevo.Web && dotnet run --launch-profile https

frontend:
	cd relevo-frontend && pnpm install && pnpm run setup && pnpm run dev
