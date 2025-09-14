package http

import "technical-test-backend/internal/sessions"

func CreateHTTPHandler(sessionPool sessions.Pool) *Handler {
	return NewHandler(sessionPool)
}
