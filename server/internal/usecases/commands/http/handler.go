package http

import (
	"encoding/json"
	"fmt"
	"net/http"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/core/commands"
	httputils "technical-test-backend/internal/http"
	"technical-test-backend/internal/session"
	usecasescommands "technical-test-backend/internal/usecases/commands"
)

// Handler handles HTTP requests for commands
type Handler struct {
	commandHandler *usecasescommands.Handler
	sessionPool    session.Pool
}

// NewHandler creates a new commands HTTP handler
func NewHandler(commandHandler *usecasescommands.Handler, sessionPool session.Pool) *Handler {
	return &Handler{
		commandHandler: commandHandler,
		sessionPool:    sessionPool,
	}
}

// HandleCommand handles the command endpoint
func (h *Handler) HandleCommand(w http.ResponseWriter, r *http.Request) {
	accountID := r.Header.Get("X-Account-ID")

	// Parse the command from JSON
	var commandArgs usecasescommands.CommandArgs
	if err := httputils.DecodeJSON(r, &commandArgs); err != nil {
		httputils.WriteError(w, http.StatusBadRequest, "invalid request body")
		return
	}

	if commandArgs.Command == "" {
		httputils.WriteError(w, http.StatusBadRequest, "missing command argument")
		return
	}

	if commandArgs.Data == nil {
		httputils.WriteError(w, http.StatusBadRequest, "missing data argument")
		return
	}

	var sessionState core.SessionState
	if err := h.sessionPool.GetSessionData(accountID, &sessionState); err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, "missing session data")
		return
	}

	sessionData := usecasescommands.SessionData{
		AccountID:    accountID,
		SessionState: &sessionState,
	}

	command, err := parseCommand(commandArgs)
	if err != nil {
		httputils.WriteError(w, http.StatusBadRequest, "invalid command data")
		return
	}

	if err := h.commandHandler.Handle(sessionData, command); err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, err.Error())
		return
	}

	if err := h.sessionPool.SetSessionData(accountID, sessionState); err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, "failed to update session data")
		return
	}

	// Return empty response on success (matching Unity client expectations)
	httputils.WriteJSON(w, http.StatusOK, map[string]interface{}{})
}

func parseCommand(args usecasescommands.CommandArgs) (core.Command, error) {

	var command core.Command
	switch args.Command {
	case "BeginLevel":
		command = &commands.BeginLevel{}
	case "EndLevel":
		command = &commands.EndLevel{}
	default:
		return nil, fmt.Errorf("invalid command")
	}

	if err := json.Unmarshal(args.Data, command); err != nil {
		return nil, fmt.Errorf("failed to unmarshal command: %v", err)
	}

	return command, nil
}
