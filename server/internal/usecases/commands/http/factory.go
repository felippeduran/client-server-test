package http

import (
	"technical-test-backend/internal/sessions"
	"technical-test-backend/internal/usecases/commands"
	"technical-test-backend/internal/usecases/configs"
	"technical-test-backend/internal/usecases/players"
)

func CreateHTTPHandler(config commands.Config, sessionsData sessions.Data, dal players.StateDAL, configsProvider *configs.Provider) *Handler {
	return NewHandler(commands.NewHandler(config, dal, configsProvider), sessionsData)
}
