package app

import (
	"context"
	"fmt"
	"log"
	"net/http"
	httputils "technical-test-backend/internal/http"
	"technical-test-backend/internal/sessions/memory"
	authenticationhttp "technical-test-backend/internal/usecases/authentication/http"
	"technical-test-backend/internal/usecases/commands"
	commandshttp "technical-test-backend/internal/usecases/commands/http"
	"technical-test-backend/internal/usecases/configs"
	configshttp "technical-test-backend/internal/usecases/configs/http"
	heartbeathttp "technical-test-backend/internal/usecases/heartbeat/http"
	playersmemory "technical-test-backend/internal/usecases/players/dal/memory"
	playershttp "technical-test-backend/internal/usecases/players/http"
	"time"
)

type Config struct {
	Port           int
	SessionPool    memory.SessionPoolConfig
	ConfigProvider configs.ProviderConfig
	Commands       commands.Config
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
	configsProvider := configs.NewProvider(a.config.ConfigProvider)

	authHandler := authenticationhttp.CreateHTTPHandler(sessionPool, accountsDal)
	stateHandler := playershttp.CreateHTTPHandler(sessionPool, accountsDal)
	configHandler := configshttp.CreateHTTPHandler(configsProvider)
	commandHandler := commandshttp.CreateHTTPHandler(a.config.Commands, sessionPool, accountsDal, configsProvider)
	heartbeatHandler := heartbeathttp.CreateHTTPHandler(sessionPool)

	mux := http.NewServeMux()
	mux.HandleFunc("GET /health", httputils.LogMiddleware(handleHealth))
	mux.HandleFunc("POST /AuthenticationHandler/Authenticate", httputils.LogMiddleware(authHandler.HandleAuthenticate))

	authMiddleware := httputils.NewAuthMiddleware(sessionPool)
	mux.HandleFunc("POST /InitializationHandler/GetPlayerState", httputils.LogMiddleware(authMiddleware.Middleware(stateHandler.HandleGetPlayerState)))
	mux.HandleFunc("POST /InitializationHandler/GetConfigs", httputils.LogMiddleware(authMiddleware.Middleware(configHandler.HandleGetConfigs)))
	mux.HandleFunc("POST /CommandHandler/HandleCommand", httputils.LogMiddleware(authMiddleware.Middleware(commandHandler.HandleCommand)))
	mux.HandleFunc("POST /HeartbeatHandler/Heartbeat", httputils.LogMiddleware(authMiddleware.Middleware(heartbeatHandler.HandleHeartbeat)))

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
