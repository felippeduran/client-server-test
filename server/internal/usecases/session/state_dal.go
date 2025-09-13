package usecases

type StateDAL interface {
	GetSessionData(sessionID string, data interface{}) error
	SetSessionData(sessionID string, data interface{}) error
}
