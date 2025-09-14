package http

import (
	"technical-test-backend/internal/session"
	"technical-test-backend/internal/usecases/authentication"
	"technical-test-backend/internal/usecases/players"
)

func CreateHTTPHandler(sessionPool session.Pool, dal players.AccountDAL) *Handler {
	return NewHandler(authentication.NewHandler(sessionPool, dal))
}
