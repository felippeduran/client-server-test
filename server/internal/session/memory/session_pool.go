package memory

import (
	"encoding/json"
	"sync"
	"technical-test-backend/internal/errors"
	"technical-test-backend/internal/session"
	"time"

	"github.com/google/uuid"
)

var (
	ErrSessionNotFound       = errors.New("session not found")
	ErrSessionExpired        = errors.New("session expired")
	ErrFailedToMarshalData   = errors.New("failed to marshal data")
	ErrFailedToUnmarshalData = errors.New("failed to unmarshal data")
)

type SessionPoolConfig struct {
	TTL time.Duration
}

// SessionPool manages active user sessions
type SessionPool struct {
	sessions              map[string]session.Session
	sessionsData          map[string]json.RawMessage
	accountIDsBySessionID map[string]string
	sessionIDsByAccountID map[string]string
	mutex                 sync.RWMutex
	config                SessionPoolConfig
}

// NewSessionPool creates a new session pool
func NewSessionPool(config SessionPoolConfig) *SessionPool {
	sp := &SessionPool{
		sessions:              make(map[string]session.Session),
		accountIDsBySessionID: make(map[string]string),
		sessionIDsByAccountID: make(map[string]string),
		sessionsData:          make(map[string]json.RawMessage),
		config:                config,
	}

	return sp
}

// CreateSession creates a new session
func (sp *SessionPool) CreateSession(accountID string, data interface{}) (session.Session, error) {
	sess := session.Session{
		ID:           uuid.New().String(),
		AccountID:    accountID,
		LastActivity: time.Now(),
		CreatedAt:    time.Now(),
	}

	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	sp.sessions[sess.ID] = sess
	sp.accountIDsBySessionID[sess.ID] = accountID
	sp.sessionIDsByAccountID[accountID] = sess.ID

	rawData, err := json.Marshal(data)
	if err != nil {
		return session.Session{}, errors.Wrap(err, ErrFailedToMarshalData)
	}

	sp.sessionsData[sess.ID] = rawData

	return sess, nil
}

// GetSession retrieves a session by ID
func (sp *SessionPool) GetSession(sessionID string) (session.Session, bool) {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()

	sess, exists := sp.sessions[sessionID]
	if !exists {
		return session.Session{}, false
	}

	if time.Now().After(sess.LastActivity.Add(sp.config.TTL)) {
		return session.Session{}, false
	}

	return sess, true
}

func (sp *SessionPool) GetSessionData(accountID string, data interface{}) error {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()

	sessionID, exists := sp.sessionIDsByAccountID[accountID]
	if !exists {
		return ErrSessionNotFound
	}

	savedData, exists := sp.sessionsData[sessionID]
	if !exists {
		return ErrSessionNotFound
	}

	err := json.Unmarshal(savedData, data)
	if err != nil {
		return errors.Wrap(err, ErrFailedToUnmarshalData)
	}

	return nil
}

func (sp *SessionPool) SetSessionData(accountID string, data interface{}) error {
	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	rawData, err := json.Marshal(data)
	if err != nil {
		return errors.Wrap(err, ErrFailedToMarshalData)
	}

	sessionID, exists := sp.sessionIDsByAccountID[accountID]
	if !exists {
		return ErrSessionNotFound
	}

	sp.sessionsData[sessionID] = rawData
	return nil
}

// UpdateActivity updates the last activity time for a session
func (sp *SessionPool) UpdateActivity(sessionID string) error {
	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	session, exists := sp.sessions[sessionID]
	if !exists {
		return ErrSessionNotFound
	}

	if time.Now().After(session.LastActivity.Add(sp.config.TTL)) {
		return ErrSessionExpired
	}

	session.LastActivity = time.Now()
	sp.sessions[sessionID] = session
	return nil
}

// RemoveSession removes a session
func (sp *SessionPool) RemoveSession(sessionID string) {
	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	session, exists := sp.sessions[sessionID]
	if !exists {
		return
	}

	// Remove from sessions map
	delete(sp.sessions, sessionID)
	delete(sp.sessionsData, sessionID)
	delete(sp.sessionIDsByAccountID, session.AccountID)
	delete(sp.accountIDsBySessionID, sessionID)
}

// GetAccountID returns the account ID for a session
func (sp *SessionPool) GetAccountID(sessionID string) (string, bool) {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()

	sess, exists := sp.sessions[sessionID]
	if !exists {
		return "", false
	}

	if time.Now().After(sess.LastActivity.Add(sp.config.TTL)) {
		return "", false
	}

	return sess.AccountID, true
}

// CleanupExpiredSessions removes expired sessions
func (sp *SessionPool) CleanupExpiredSessions() {
	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	now := time.Now()
	var expiredSessions []string

	// Find expired sessions
	for sessionID, session := range sp.sessions {
		if now.After(session.LastActivity.Add(sp.config.TTL)) {
			expiredSessions = append(expiredSessions, sessionID)
		}
	}

	// Remove expired sessions
	for _, sessionID := range expiredSessions {
		session := sp.sessions[sessionID]
		delete(sp.sessions, sessionID)
		delete(sp.sessionsData, sessionID)
		delete(sp.sessionIDsByAccountID, session.AccountID)
		delete(sp.accountIDsBySessionID, sessionID)
	}
}

// GetSessionCount returns the number of active sessions
func (sp *SessionPool) GetSessionCount() int {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()
	return len(sp.sessions)
}
