package commands

import (
	"encoding/json"
	"fmt"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/session"
	"technical-test-backend/internal/usecases/configs"
	"technical-test-backend/internal/usecases/players"
)

type SessionData struct {
	AccountID    string
	SessionState *core.SessionState
}

type CommandArgs struct {
	Command string          `json:"command"`
	Data    json.RawMessage `json:"data"`
}

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

func (h *Handler) Handle(sessionData SessionData, command core.Command) error {
	// Get persistent state
	persistentState, err := h.dal.GetPersistentState(sessionData.AccountID)
	if err != nil {
		return fmt.Errorf("failed to get persistent state: %v", err)
	}

	// Create player state
	playerState := core.PlayerState{
		Persistent: &persistentState,
		Session:    sessionData.SessionState,
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
	err = h.dal.SetPersistentState(sessionData.AccountID, *playerState.Persistent)
	if err != nil {
		return fmt.Errorf("failed to save persistent state: %v", err)
	}

	// Update session state
	sessionData.SessionState.CurrentLevelID = playerState.Session.CurrentLevelID

	return nil
}
