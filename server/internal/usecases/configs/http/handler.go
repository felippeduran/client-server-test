package http

import (
	"net/http"
	httputils "technical-test-backend/internal/http"
	usecasesconfigs "technical-test-backend/internal/usecases/configs"
)

type Handler struct {
	configHandler *usecasesconfigs.Handler
}

func NewHandler(configHandler *usecasesconfigs.Handler) *Handler {
	return &Handler{
		configHandler: configHandler,
	}
}

func (h *Handler) HandleGetConfigs(w http.ResponseWriter, r *http.Request) {
	var args usecasesconfigs.GetConfigsArgs
	if err := httputils.DecodeJSON(r, &args); err != nil {
		httputils.WriteError(w, http.StatusBadRequest, "Invalid request body")
		return
	}

	res, err := h.configHandler.GetConfigs(&args)
	if err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, err.Error())
		return
	}

	httputils.WriteJSON(w, http.StatusOK, res)
}
