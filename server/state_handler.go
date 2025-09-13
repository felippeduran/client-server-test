package main

import (
	"fmt"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/session"
	"time"
)

// GetPlayerStateArgs represents get player state request
type GetPlayerStateArgs struct{}

// GetPlayerStateRes represents get player state response
type GetPlayerStateRes struct {
	PlayerState core.PlayerState `json:"playerState"`
	ServerTime  time.Time        `json:"serverTime"`
}

// PlayerStateHandler handles player state requests
type PlayerStateHandler struct {
	sessionPool session.Pool
	dal         *DAL
}

// NewPlayerStateHandler creates a new player state handler
func NewPlayerStateHandler(sessionPool session.Pool, dal *DAL) *PlayerStateHandler {
	return &PlayerStateHandler{
		sessionPool: sessionPool,
		dal:         dal,
	}
}

// GetPlayerState retrieves the current player state
func (h *PlayerStateHandler) GetPlayerState(sessionID string, args *GetPlayerStateArgs) (*GetPlayerStateRes, error) {
	// Check authentication
	accountID, authenticated := h.sessionPool.GetAccountID(sessionID)
	if !authenticated {
		return nil, fmt.Errorf("connection not authenticated")
	}

	// Get persistent state
	persistentState, err := h.dal.GetPersistentState(accountID)
	if err != nil {
		return nil, fmt.Errorf("failed to get persistent state: %v", err)
	}

	// Get session state
	var sessionState core.SessionState
	if err := h.sessionPool.GetSessionData(sessionID, &sessionState); err != nil {
		return nil, fmt.Errorf("session not found")
	}

	// Create player state
	playerState := core.PlayerState{
		Persistent: persistentState,
		Session:    sessionState,
	}

	// Update activity
	h.sessionPool.UpdateActivity(sessionID)

	return &GetPlayerStateRes{
		PlayerState: playerState,
		ServerTime:  time.Now(),
	}, nil
}
