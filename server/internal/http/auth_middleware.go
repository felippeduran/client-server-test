package http

import (
	"log"
	"net/http"
	"technical-test-backend/internal/sessions"
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
			WriteError(w, http.StatusUnauthorized, "invalid session id header")
			return
		}

		// if _, err := uuid.Parse(sessionID); err != nil {
		// 	WriteError(w, http.StatusUnauthorized, "invalid session id header")
		// 	return
		// }

		accountID, authenticated := m.sessionPool.GetAccountID(sessionID)
		if !authenticated {
			WriteError(w, http.StatusUnauthorized, "invalid or expired session")
			return
		}

		if err := m.sessionPool.UpdateActivity(sessionID); err != nil {
			log.Printf("Warning: Failed to update session activity: %v", err)
		}

		r.Header.Set("X-Account-ID", accountID)

		next(w, r)
	}
}
