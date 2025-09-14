package usecases

import "technical-test-backend/internal/core"

type SessionData struct {
	AccountID    string
	SessionState *core.SessionState
}
