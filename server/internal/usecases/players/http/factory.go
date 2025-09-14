package http

import (
	"technical-test-backend/internal/sessions"
	"technical-test-backend/internal/usecases/players"
)

func CreateHTTPHandler(sessionPool sessions.Pool, dal players.StateDAL) *Handler {
	return NewHandler(players.NewStateHandler(dal), sessionPool)
}
