package app

import (
	"context"
	"fmt"
	"log"
	"net/http"
	httputils "technical-test-backend/internal/http"
	"technical-test-backend/internal/session/memory"
	authenticationhttp "technical-test-backend/internal/usecases/authentication/http"
	commandshttp "technical-test-backend/internal/usecases/commands/http"
	"technical-test-backend/internal/usecases/configs"
	configshttp "technical-test-backend/internal/usecases/configs/http"
	playersmemory "technical-test-backend/internal/usecases/players/dal/memory"
	playershttp "technical-test-backend/internal/usecases/players/http"
	"time"
)

type Config struct {
	Port        int
	SessionPool memory.SessionPoolConfig
	GameConfig  configs.ProviderConfig
}

type HTTP struct {
	config Config
	server *http.Server
}

func NewHTTP(config Config) *HTTP {
	return &HTTP{
		config: config,
	}
}

func (a *HTTP) Run() {
	sessionPool, close := memory.CreateSessionPool(a.config.SessionPool)
	defer close()

	accountsDal := playersmemory.NewDAL()
	configsProvider := configs.NewProvider(a.config.GameConfig)

	// Create handlers
	authHandler := authenticationhttp.CreateHTTPHandler(sessionPool, accountsDal)
	stateHandler := playershttp.CreateHTTPHandler(sessionPool, accountsDal)
	configHandler := configshttp.CreateHTTPHandler(configsProvider)
	commandHandler := commandshttp.CreateHTTPHandler(sessionPool, accountsDal, configsProvider)

	mux := http.NewServeMux()
	mux.HandleFunc("GET /health", handleHealth)
	mux.HandleFunc("POST /AuthenticationHandler/Authenticate", authHandler.HandleAuthenticate)

	authMiddleware := httputils.NewAuthMiddleware(sessionPool)
	mux.HandleFunc("POST /InitializationHandler/GetPlayerState", authMiddleware.Middleware(stateHandler.HandleGetPlayerState))
	mux.HandleFunc("POST /InitializationHandler/GetConfigs", authMiddleware.Middleware(configHandler.HandleGetConfigs))
	mux.HandleFunc("POST /CommandHandler/HandleCommand", authMiddleware.Middleware(commandHandler.HandleCommand))

	a.server = &http.Server{
		Addr:    fmt.Sprintf(":%d", a.config.Port),
		Handler: mux,
	}

	if err := a.server.ListenAndServe(); err != http.ErrServerClosed {
		log.Fatalf("Failed to start server: %v", err)
	}
}

func (a *HTTP) Stop(ctx context.Context) error {
	return a.server.Shutdown(context.Background())
}

func handleHealth(w http.ResponseWriter, r *http.Request) {
	response := map[string]interface{}{
		"status":    "ok",
		"timestamp": time.Now(),
	}

	httputils.WriteJSON(w, http.StatusOK, response)
}
