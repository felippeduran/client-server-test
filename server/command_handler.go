package main

import (
	"fmt"
	"technical-test-backend/internal/session"
)

// CommandHandler handles command execution
type CommandHandler struct {
	sessionPool session.Pool
	dal         *DAL
	configs     *ConfigsProvider
}

// NewCommandHandler creates a new command handler
func NewCommandHandler(sessionPool session.Pool, dal *DAL, configs *ConfigsProvider) *CommandHandler {
	return &CommandHandler{
		sessionPool: sessionPool,
		dal:         dal,
		configs:     configs,
	}
}

// HandleCommand executes a game command
func (h *CommandHandler) HandleCommand(sessionID string, command Command) error {
	// Check authentication
	accountID, authenticated := h.sessionPool.GetAccountID(sessionID)
	if !authenticated {
		return fmt.Errorf("connection not authenticated")
	}

	// Get persistent state
	persistentState, err := h.dal.GetPersistentState(accountID)
	if err != nil {
		return fmt.Errorf("failed to get persistent state: %v", err)
	}

	// Get session state
	var sessionState SessionState
	if err := h.sessionPool.GetSessionData(sessionID, &sessionState); err != nil {
		return fmt.Errorf("session not found")
	}

	// Create player state
	playerState := PlayerState{
		Persistent: persistentState,
		Session:    sessionState,
	}

	// Get configs
	configs := h.configs.GetHardcodedConfigs()

	// Execute command
	err = command.Execute(&playerState, configs)
	if err != nil {
		return fmt.Errorf("command execution failed: %v", err)
	}

	// Update persistent state
	err = h.dal.SetPersistentState(accountID, playerState.Persistent)
	if err != nil {
		return fmt.Errorf("failed to save persistent state: %v", err)
	}

	// Update session state
	sessionState.CurrentLevelID = playerState.Session.CurrentLevelID
	if err := h.sessionPool.SetSessionData(sessionID, sessionState); err != nil {
		return fmt.Errorf("failed to save session state: %v", err)
	}

	// Update activity
	h.sessionPool.UpdateActivity(sessionID)

	return nil
}
