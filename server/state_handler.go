package main

import (
	"fmt"
	"time"
)

// GetPlayerStateArgs represents get player state request
type GetPlayerStateArgs struct{}

// GetPlayerStateRes represents get player state response
type GetPlayerStateRes struct {
	PlayerState PlayerState `json:"playerState"`
	ServerTime  time.Time   `json:"serverTime"`
}

// PlayerStateHandler handles player state requests
type PlayerStateHandler struct {
	sessionPool *SessionPool
	dal         *DAL
}

// NewPlayerStateHandler creates a new player state handler
func NewPlayerStateHandler(sessionPool *SessionPool, dal *DAL) *PlayerStateHandler {
	return &PlayerStateHandler{
		sessionPool: sessionPool,
		dal:         dal,
	}
}

// GetPlayerState retrieves the current player state
func (h *PlayerStateHandler) GetPlayerState(sessionID string, args *GetPlayerStateArgs) (*GetPlayerStateRes, *Error) {
	// Check authentication
	accountID, authenticated := h.sessionPool.GetAccountID(sessionID)
	if !authenticated {
		return nil, &Error{Message: "connection not authenticated"}
	}

	// Get persistent state
	persistentState, err := h.dal.GetPersistentState(accountID)
	if err != nil {
		return nil, &Error{Message: fmt.Sprintf("failed to get persistent state: %v", err)}
	}

	// Get session state
	session, exists := h.sessionPool.GetSession(sessionID)
	if !exists {
		return nil, &Error{Message: "session not found"}
	}

	// Create player state
	playerState := PlayerState{
		Persistent: persistentState,
		Session:    SessionState{CurrentLevelID: session.CurrentLevelID},
	}

	// Update activity
	h.sessionPool.UpdateActivity(sessionID)

	return &GetPlayerStateRes{
		PlayerState: playerState,
		ServerTime:  time.Now(),
	}, nil
}
