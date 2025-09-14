# Makefile for Game Server

# Variables
MAIN_PATH=./cmd/server
TEST_PATH=./...

# Go parameters
GOCMD=go
GOTEST=$(GOCMD) test

# Run the server
.PHONY: run
run:
	@echo "Starting server..."
	cd $(MAIN_PATH) && $(GOCMD) run .

# Run tests
.PHONY: test
test:
	@echo "Running tests..."
	$(GOTEST) -v -count=1 -tags=unit,integration $(TEST_PATH)

# Lint code
.PHONY: lint
lint:
	@echo "Linting code..."
	@if command -v golangci-lint >/dev/null 2>&1; then \
		golangci-lint run; \
	else \
		echo "golangci-lint not found. Install it with: go install github.com/golangci/golangci-lint/cmd/golangci-lint@latest"; \
	fi

# Help
.PHONY: help
help:
	@echo "Available commands:"
	@echo "  run    - Run the server"
	@echo "  test   - Run all tests"
	@echo "  lint   - Lint the code"
	@echo "  help   - Show this help message"

# Default target
.DEFAULT_GOAL := help
