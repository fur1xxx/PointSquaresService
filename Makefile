.PHONY: default
default: help

.PHONY: help
help: ## Get this help.
	@echo Tasks:
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}' $(MAKEFILE_LIST)

.PHONY: build
build: ## Build the solution
	dotnet build

.PHONY: test
test: ## Run the unit tests
	dotnet test

.PHONY: format
format: ## Format code
	dotnet tool restore
	dotnet format
	dotnet csharpier format .

.PHONY: run
run: ## Start MySQL and the API in docker (rebuilds if necessary)
	@docker compose up -d --build --wait

.PHONY: dev-db
dev-db: ## Start only MySQL in docker, to run the API from the IDE
	@docker compose -f docker-compose-dev.yml up -d --wait

.PHONY: down
down: ## Stop and remove all docker containers and their data
	@docker compose -f docker-compose.yml -f docker-compose-dev.yml down --volumes
