package http

import (
	"technical-test-backend/internal/session"
	"technical-test-backend/internal/usecases/commands"
	"technical-test-backend/internal/usecases/configs"
	"technical-test-backend/internal/usecases/players"
)

func CreateHTTPHandler(sessionPool session.Pool, dal players.StateDAL, configs *configs.Provider) *Handler {
	return NewHandler(commands.NewHandler(sessionPool, dal, configs), sessionPool)
}
