package main

import (
	"crypto/rand"
	"encoding/hex"
	"fmt"
	"sync"
	"time"
)

// SessionPool manages active user sessions
type SessionPool struct {
	sessions      map[string]*Session // sessionID -> session
	accounts      map[string]*Session // accountID -> session (one session per account)
	mutex         sync.RWMutex
	cleanupTicker *time.Ticker
}

// NewSessionPool creates a new session pool
func NewSessionPool() *SessionPool {
	sp := &SessionPool{
		sessions: make(map[string]*Session),
		accounts: make(map[string]*Session),
	}

	// Start cleanup routine
	sp.startCleanupRoutine()

	return sp
}

// CreateSession creates a new session
func (sp *SessionPool) CreateSession() (*Session, error) {
	sessionID, err := generateSessionID()
	if err != nil {
		return nil, fmt.Errorf("failed to generate session ID: %w", err)
	}

	session := &Session{
		ID:            sessionID,
		AccountID:     "",
		Authenticated: false,
		LastActivity:  time.Now(),
		CreatedAt:     time.Now(),
	}

	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	sp.sessions[sessionID] = session
	return session, nil
}

// GetSession retrieves a session by ID
func (sp *SessionPool) GetSession(sessionID string) (*Session, bool) {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()

	session, exists := sp.sessions[sessionID]
	return session, exists
}

// AuthenticateSession authenticates a session with an account
func (sp *SessionPool) AuthenticateSession(sessionID, accountID string) error {
	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	session, exists := sp.sessions[sessionID]
	if !exists {
		return fmt.Errorf("session not found")
	}

	// Check if account is already authenticated in another session
	if existingSession, exists := sp.accounts[accountID]; exists {
		// Remove old session
		delete(sp.sessions, existingSession.ID)
	}

	// Update session
	session.AccountID = accountID
	session.Authenticated = true
	session.LastActivity = time.Now()

	// Update account mapping
	sp.accounts[accountID] = session

	return nil
}

// UpdateActivity updates the last activity time for a session
func (sp *SessionPool) UpdateActivity(sessionID string) error {
	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	session, exists := sp.sessions[sessionID]
	if !exists {
		return fmt.Errorf("session not found")
	}

	session.LastActivity = time.Now()
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

	// Remove from accounts map if this session was authenticated
	if session.Authenticated && session.AccountID != "" {
		delete(sp.accounts, session.AccountID)
	}
}

// GetSessionByAccount retrieves a session by account ID
func (sp *SessionPool) GetSessionByAccount(accountID string) (*Session, bool) {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()

	session, exists := sp.accounts[accountID]
	return session, exists
}

// IsAuthenticated checks if a session is authenticated
func (sp *SessionPool) IsAuthenticated(sessionID string) bool {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()

	session, exists := sp.sessions[sessionID]
	return exists && session.Authenticated
}

// GetAccountID returns the account ID for a session
func (sp *SessionPool) GetAccountID(sessionID string) (string, bool) {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()

	session, exists := sp.sessions[sessionID]
	if !exists || !session.Authenticated {
		return "", false
	}

	return session.AccountID, true
}

// startCleanupRoutine starts the background cleanup routine
func (sp *SessionPool) startCleanupRoutine() {
	sp.cleanupTicker = time.NewTicker(5 * time.Minute)
	go func() {
		for range sp.cleanupTicker.C {
			sp.cleanupExpiredSessions()
		}
	}()
}

// cleanupExpiredSessions removes expired sessions
func (sp *SessionPool) cleanupExpiredSessions() {
	sp.mutex.Lock()
	defer sp.mutex.Unlock()

	now := time.Now()
	var expiredSessions []string

	// Find expired sessions
	for sessionID, session := range sp.sessions {
		// Remove idle sessions (> 30 minutes)
		if now.Sub(session.LastActivity) > 30*time.Minute {
			expiredSessions = append(expiredSessions, sessionID)
			continue
		}

		// Remove old sessions (> 24 hours)
		if now.Sub(session.CreatedAt) > 24*time.Hour {
			expiredSessions = append(expiredSessions, sessionID)
		}
	}

	// Remove expired sessions
	for _, sessionID := range expiredSessions {
		session := sp.sessions[sessionID]
		delete(sp.sessions, sessionID)

		// Remove from accounts map if authenticated
		if session.Authenticated && session.AccountID != "" {
			delete(sp.accounts, session.AccountID)
		}
	}
}

// Stop stops the cleanup routine
func (sp *SessionPool) Stop() {
	if sp.cleanupTicker != nil {
		sp.cleanupTicker.Stop()
	}
}

// GetSessionCount returns the number of active sessions
func (sp *SessionPool) GetSessionCount() int {
	sp.mutex.RLock()
	defer sp.mutex.RUnlock()
	return len(sp.sessions)
}

// generateSessionID generates a random session ID
func generateSessionID() (string, error) {
	bytes := make([]byte, 16)
	if _, err := rand.Read(bytes); err != nil {
		return "", err
	}
	return hex.EncodeToString(bytes), nil
}
