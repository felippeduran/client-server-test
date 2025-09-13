package commands

import (
	"fmt"
	"technical-test-backend/internal/configs"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/session"
	"technical-test-backend/internal/usecases/players"
)

type Handler struct {
	sessionPool session.Pool
	dal         players.StateDAL
	configs     *configs.Provider
}

func NewHandler(sessionPool session.Pool, dal players.StateDAL, configs *configs.Provider) *Handler {
	return &Handler{
		sessionPool: sessionPool,
		dal:         dal,
		configs:     configs,
	}
}

func (h *Handler) Handle(sessionID string, command core.Command) error {
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
	var sessionState core.SessionState
	if err := h.sessionPool.GetSessionData(sessionID, &sessionState); err != nil {
		return fmt.Errorf("session not found")
	}

	// Create player state
	playerState := core.PlayerState{
		Persistent: persistentState,
		Session:    sessionState,
	}

	// Get configs
	configs, err := h.configs.GetConfigs()
	if err != nil {
		return fmt.Errorf("failed to load configs: %v", err)
	}

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
