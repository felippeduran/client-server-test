package sessions

import "time"

type Session struct {
	ID           string    `json:"id"`
	AccountID    string    `json:"accountId"`
	LastActivity time.Time `json:"lastActivity"`
	CreatedAt    time.Time `json:"createdAt"`
}

type Pool interface {
	CreateSession(accountID string, data interface{}) (Session, error)
	GetSession(sessionID string) (Session, bool)
	GetSessionData(accountID string, data interface{}) error
	SetSessionData(accountID string, data interface{}) error
	UpdateActivity(sessionID string) error
	RemoveSession(sessionID string)
	GetAccountID(sessionID string) (string, bool)
}

type Data interface {
	GetSessionData(sessionID string, data interface{}) error
	SetSessionData(sessionID string, data interface{}) error
}
