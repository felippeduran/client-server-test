package http

import (
	"technical-test-backend/internal/session"
	"technical-test-backend/internal/usecases/commands"
	"technical-test-backend/internal/usecases/configs"
	"technical-test-backend/internal/usecases/players"
)

func CreateHTTPHandler(config commands.Config, sessionPool session.Pool, dal players.StateDAL, configsProvider *configs.Provider) *Handler {
	return NewHandler(commands.NewHandler(config, sessionPool, dal, configsProvider), sessionPool)
}
