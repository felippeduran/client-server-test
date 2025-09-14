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
		// Get session ID from header
		sessionID := r.Header.Get("X-Session-ID")
		if sessionID == "" {
			WriteError(w, http.StatusUnauthorized, "invalid session id header")
			return
		}

		// if _, err := uuid.Parse(sessionID); err != nil {
		// 	WriteError(w, http.StatusUnauthorized, "invalid session id header")
		// 	return
		// }

		// Verify session exists in session pool
		accountID, authenticated := m.sessionPool.GetAccountID(sessionID)
		if !authenticated {
			WriteError(w, http.StatusUnauthorized, "invalid or expired session")
			return
		}

		// Update session activity
		if err := m.sessionPool.UpdateActivity(sessionID); err != nil {
			// Log the error but don't fail the request
			// The session might have expired between auth check and activity update
			log.Printf("Warning: Failed to update session activity: %v", err)
		}

		// Store session info in request context for handlers to use
		r.Header.Set("X-Account-ID", accountID)

		next(w, r)
	}
}
