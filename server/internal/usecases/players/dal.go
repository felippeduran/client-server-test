package players

import "technical-test-backend/internal/core"

type AccountDAL interface {
	CreateAccount(account Account, state core.PersistentState) error
	GetAccessToken(accountID string) (string, error)
}

type StateDAL interface {
	GetPersistentState(accountID string) (core.PersistentState, error)
	SetPersistentState(accountID string, state core.PersistentState) error
}
