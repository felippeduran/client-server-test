package memory

import (
	"encoding/json"
	"log"
	"sync"
	"technical-test-backend/internal/errors"
	"technical-test-backend/internal/sessions"
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

type SessionPool struct {
	sessions              map[string]sessions.Session
	sessionsData          map[string]json.RawMessage
	accountIDsBySessionID map[string]string
	sessionIDsByAccountID map[string]string
	mutex                 sync.RWMutex
	config                SessionPoolConfig
}

func NewSessionPool(config SessionPoolConfig) *SessionPool {
	sp := &SessionPool{
		sessions:              make(map[string]sessions.Session),
		accountIDsBySessionID: make(map[string]string),
		sessionIDsByAccountID: make(map[string]string),
		sessionsData:          make(map[string]json.RawMessage),
		config:                config,
	}

	return sp
}

func (sp *SessionPool) CreateSession(accountID string, data interface{}) (sessions.Session, error) {
	sess := sessions.Session{
		ID:           uuid.New().String(),
		AccountID:    accountID,
		LastActivity: time.Now(),
		CreatedAt:    time.Now(),
	}

	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	if existingSessionID, exists := sp.sessionIDsByAccountID[accountID]; exists {
		log.Printf("Removed existing session: %s", existingSessionID)
		sp.removeSessionImpl(existingSessionID)
	}

	sp.sessions[sess.ID] = sess
	sp.accountIDsBySessionID[sess.ID] = accountID
	sp.sessionIDsByAccountID[accountID] = sess.ID

	rawData, err := json.Marshal(data)
	if err != nil {
		return sessions.Session{}, errors.Wrap(err, ErrFailedToMarshalData)
	}

	sp.sessionsData[sess.ID] = rawData

	log.Printf("Created session: %s", sess.ID)

	return sess, nil
}

func (sp *SessionPool) GetSession(sessionID string) (sessions.Session, bool) {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()

	sess, exists := sp.sessions[sessionID]
	if !exists {
		return sessions.Session{}, false
	}

	if time.Now().After(sess.LastActivity.Add(sp.config.TTL)) {
		return sessions.Session{}, false
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

func (sp *SessionPool) RemoveSession(sessionID string) {
	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	if _, exists := sp.sessions[sessionID]; !exists {
		return
	}

	sp.removeSessionImpl(sessionID)
}

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

func (sp *SessionPool) CleanupExpiredSessions() {
	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	now := time.Now()
	var expiredSessions []string

	for sessionID, session := range sp.sessions {
		if now.After(session.LastActivity.Add(sp.config.TTL)) {
			expiredSessions = append(expiredSessions, sessionID)
		}
	}

	for _, sessionID := range expiredSessions {
		sp.removeSessionImpl(sessionID)
		log.Printf("Removed expired session: %s", sessionID)
	}
}

func (sp *SessionPool) removeSessionImpl(sessionID string) {
	session := sp.sessions[sessionID]
	delete(sp.sessions, sessionID)
	delete(sp.sessionsData, sessionID)
	delete(sp.sessionIDsByAccountID, session.AccountID)
	delete(sp.accountIDsBySessionID, sessionID)
}

func (sp *SessionPool) GetSessionCount() int {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()
	return len(sp.sessions)
}
