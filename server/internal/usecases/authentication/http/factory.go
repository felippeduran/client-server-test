package http

import (
	"technical-test-backend/internal/sessions"
	"technical-test-backend/internal/usecases/authentication"
	"technical-test-backend/internal/usecases/players"
)

func CreateHTTPHandler(sessionPool sessions.Pool, dal players.AccountDAL) *Handler {
	return NewHandler(authentication.NewHandler(dal), sessionPool)
}
