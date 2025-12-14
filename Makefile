.PHONY: help setup colima-start docker-context install-deps trust-certs db-up backend-up frontend-up frontend-setup all

# Default target
help:
	@echo "Available targets:"
	@echo "  make all              - Setup and start all services in separate terminals"
	@echo "  make setup            - Initial setup (install deps, trust certs)"
	@echo "  make colima-start     - Start Colima"
	@echo "  make docker-context   - Switch Docker context to Colima"
	@echo "  make install-deps     - Install/upgrade Colima and Lima dependencies"
	@echo "  make trust-certs      - Trust .NET development certificates"
	@echo "  make db-up            - Start Oracle database in new terminal"
	@echo "  make backend-up       - Start backend API in new terminal"
	@echo "  make frontend-up      - Start frontend in new terminal"
	@echo "  make frontend-setup   - Setup frontend (install deps, run setup)"

# Complete setup: install dependencies and trust certificates
setup: install-deps trust-certs
	@echo "✅ Initial setup complete!"

# Install/upgrade Colima and Lima dependencies
install-deps:
	@echo "📦 Installing/upgrading Colima and Lima..."
	@brew upgrade colima lima
	@echo "📦 Installing lima-additional-guestagents..."
	@brew install lima-additional-guestagents || brew upgrade lima-additional-guestagents
	@echo "✅ Dependencies installed/upgraded"

# Start Colima with x86_64 architecture
colima-start:
	@echo "🚀 Starting Colima..."
	@colima start --arch x86_64 --memory 2 --cpu 1
	@echo "✅ Colima started successfully"

# Switch Docker context to Colima
docker-context:
	@echo "🐳 Switching Docker context to Colima..."
	@docker context use colima
	@echo "✅ Docker context switched to Colima"

# Trust .NET development certificates
trust-certs:
	@echo "🔒 Trusting .NET development certificates..."
	@dotnet dev-certs https --trust
	@echo "✅ .NET certificates trusted"

# Start Oracle database in new terminal
db-up:
	@echo "🗄️  Opening database terminal..."
	@osascript -e 'tell application "Terminal" to do script "cd $(PWD)/relevo-api && docker compose up -d && echo \"✅ Database started! Press Ctrl+C to close this terminal.\" && read"'
	@echo "✅ Database terminal opened"

# Start backend API in new terminal
backend-up:
	@echo "🔧 Opening backend terminal..."
	@osascript -e 'tell application "Terminal" to do script "cd $(PWD)/relevo-api/src/Relevo.Web && dotnet run --launch-profile https"'
	@echo "✅ Backend terminal opened"

# Setup frontend (first time only)
frontend-setup:
	@echo "📦 Setting up frontend..."
	@cd relevo-frontend && pnpm install
	@cd relevo-frontend && pnpm run setup
	@echo "✅ Frontend setup complete!"

# Start frontend in new terminal
frontend-up:
	@echo "🚀 Opening frontend terminal..."
	@osascript -e 'tell application "Terminal" to do script "cd $(PWD)/relevo-frontend && pnpm install && pnpm run dev"'
	@echo "✅ Frontend terminal opened"

# Complete setup and start all services in separate terminals
all: setup colima-start docker-context db-up
	@sleep 5
	@echo "⏳ Waiting for database to be ready..."
	@sleep 10
	@$(MAKE) backend-up
	@sleep 2
	@$(MAKE) frontend-up
	@echo ""
	@echo "✅ All services started in separate terminals!"
	@echo ""
	@echo "📊 Services:"
	@echo "   - Database: localhost:1521"
	@echo "   - Backend API: Check terminal for URL (usually https://localhost:57679)"
	@echo "   - Frontend: http://localhost:5173"
	@echo ""
	@echo "💡 To stop services:"
	@echo "   - Close each terminal window"
	@echo "   - Or run: make db-down"
	@echo "   - Or run: make colima-stop"

# Stop database
db-down:
	@echo "🛑 Stopping database..."
	@cd relevo-api && docker compose down
	@echo "✅ Database stopped"

# Stop Colima
colima-stop:
	@echo "🛑 Stopping Colima..."
	@colima stop
	@echo "✅ Colima stopped"

