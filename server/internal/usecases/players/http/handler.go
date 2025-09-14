package http

import (
	"net/http"
	"technical-test-backend/internal/core"
	httputils "technical-test-backend/internal/http"
	"technical-test-backend/internal/sessions"
	"technical-test-backend/internal/usecases"
	"technical-test-backend/internal/usecases/players"
)

type Handler struct {
	stateHandler *players.StateHandler
	sessionPool  sessions.Pool
}

func NewHandler(stateHandler *players.StateHandler, sessionPool sessions.Pool) *Handler {
	return &Handler{
		stateHandler: stateHandler,
		sessionPool:  sessionPool,
	}
}

func (h *Handler) HandleGetPlayerState(w http.ResponseWriter, r *http.Request) {
	accountID := r.Header.Get("X-Account-ID")

	var sessionState core.SessionState
	if err := h.sessionPool.GetSessionData(accountID, &sessionState); err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, "missing session data")
		return
	}

	var args players.GetPlayerStateArgs
	if err := httputils.DecodeJSON(r, &args); err != nil {
		httputils.WriteError(w, http.StatusBadRequest, "invalid request body")
		return
	}

	sessionData := usecases.SessionData{
		AccountID:    accountID,
		SessionState: &sessionState,
	}

	res, err := h.stateHandler.GetPlayerState(sessionData, &args)
	if err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, err.Error())
		return
	}

	httputils.WriteJSON(w, http.StatusOK, res)
}
