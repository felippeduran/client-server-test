package http

import (
	"technical-test-backend/internal/session"
	"technical-test-backend/internal/usecases/players"
)

func CreateHTTPHandler(sessionPool session.Pool, dal players.StateDAL) *Handler {
	return NewHandler(players.NewStateHandler(sessionPool, dal), sessionPool)
}
