package http

import (
	"net/http"
	httputils "technical-test-backend/internal/http"
	"technical-test-backend/internal/sessions"
)

type Handler struct {
	sessionPool sessions.Pool
}

func NewHandler(sessionPool sessions.Pool) *Handler {
	return &Handler{
		sessionPool: sessionPool,
	}
}

func (h *Handler) HandleHeartbeat(w http.ResponseWriter, r *http.Request) {
	sessionID := r.Header.Get("X-Session-ID")

	if err := h.sessionPool.UpdateActivity(sessionID); err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, err.Error())
		return
	}

	httputils.WriteJSON(w, http.StatusOK, map[string]interface{}{})
}
