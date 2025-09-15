package http

import (
	"errors"
	"log"
	"net/http"
	"technical-test-backend/internal/sessions"

	"github.com/google/uuid"
)

var (
	ErrInvalidSessionID        = errors.New("invalid session id header")
	ErrInvalidOrExpiredSession = errors.New("invalid or expired session")
)

type AuthMiddleware struct {
	sessionPool sessions.Pool
}

func NewAuthMiddleware(sessionPool sessions.Pool) *AuthMiddleware {
	return &AuthMiddleware{
		sessionPool: sessionPool,
	}
}

func (m *AuthMiddleware) Middleware(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		sessionID := r.Header.Get("X-Session-ID")
		if sessionID == "" {
			WriteError(w, http.StatusUnauthorized, ErrInvalidSessionID.Error())
			return
		}

		if _, err := uuid.Parse(sessionID); err != nil {
			WriteError(w, http.StatusUnauthorized, ErrInvalidSessionID.Error())
			return
		}

		accountID, authenticated := m.sessionPool.GetAccountID(sessionID)
		if !authenticated {
			WriteError(w, http.StatusUnauthorized, ErrInvalidOrExpiredSession.Error())
			return
		}

		if err := m.sessionPool.UpdateActivity(sessionID); err != nil {
			log.Printf("Warning: Failed to update session activity: %v", err)
		}

		r.Header.Set("X-Account-ID", accountID)

		next(w, r)
	}
}
