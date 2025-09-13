package main

import "fmt"

// CommandHandler handles command execution
type CommandHandler struct {
	sessionPool *SessionPool
	dal         *DAL
	configs     *ConfigsProvider
}

// NewCommandHandler creates a new command handler
func NewCommandHandler(sessionPool *SessionPool, dal *DAL, configs *ConfigsProvider) *CommandHandler {
	return &CommandHandler{
		sessionPool: sessionPool,
		dal:         dal,
		configs:     configs,
	}
}

// HandleCommand executes a game command
func (h *CommandHandler) HandleCommand(sessionID string, command Command) *Error {
	// Check authentication
	accountID, authenticated := h.sessionPool.GetAccountID(sessionID)
	if !authenticated {
		return &Error{Message: "connection not authenticated"}
	}

	// Get persistent state
	persistentState, err := h.dal.GetPersistentState(accountID)
	if err != nil {
		return &Error{Message: fmt.Sprintf("failed to get persistent state: %v", err)}
	}

	// Get session state
	session, exists := h.sessionPool.GetSession(sessionID)
	if !exists {
		return &Error{Message: "session not found"}
	}

	// Create player state
	playerState := PlayerState{
		Persistent: persistentState,
		Session:    SessionState{CurrentLevelID: session.CurrentLevelID},
	}

	// Get configs
	configs := h.configs.GetHardcodedConfigs()

	// Execute command
	err = command.Execute(&playerState, configs)
	if err != nil {
		if gameErr, ok := err.(*Error); ok {
			return gameErr
		}
		return &Error{Message: fmt.Sprintf("command execution failed: %v", err)}
	}

	// Update persistent state
	err = h.dal.SetPersistentState(accountID, playerState.Persistent)
	if err != nil {
		return &Error{Message: fmt.Sprintf("failed to save persistent state: %v", err)}
	}

	// Update session state
	session.CurrentLevelID = playerState.Session.CurrentLevelID

	// Update activity
	h.sessionPool.UpdateActivity(sessionID)

	return nil
}
