.DEFAULT_GOAL := help

.PHONY: help up down restart logs api infra

help:
	@echo "Usage:"
	@echo "  make up       Start infrastructure + API"
	@echo "  make infra    Start PostgreSQL and Elsa Studio only"
	@echo "  make api      Start the .NET backend only"
	@echo "  make down     Stop all containers"
	@echo "  make down     Stop all containers and remove volumes"
	@echo "  make restart  Restart containers then run the API"
	@echo "  make logs     Tail container logs"

up: infra api

infra:
	docker compose up -d
	@echo "Waiting for PostgreSQL to be ready..."
	@docker compose exec postgres sh -c 'until pg_isready -U elsa -d elsa; do sleep 1; done' 2>/dev/null || sleep 3
	@echo "Infrastructure ready."

api:
	cd backend && dotnet run

down:
	docker compose down

down:
	docker compose down -v

restart:
	docker compose restart
	cd backend && dotnet run

logs:
	docker compose logs -f
