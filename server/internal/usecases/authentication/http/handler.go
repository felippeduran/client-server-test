package http

import (
	"net/http"
	httputils "technical-test-backend/internal/http"
	"technical-test-backend/internal/usecases/authentication"
)

// Handler handles HTTP requests for authentication
type Handler struct {
	authHandler *authentication.Handler
}

// NewHandler creates a new authentication HTTP handler
func NewHandler(authHandler *authentication.Handler) *Handler {
	return &Handler{
		authHandler: authHandler,
	}
}

// HandleAuthenticate handles the authentication endpoint
func (h *Handler) HandleAuthenticate(w http.ResponseWriter, r *http.Request) {
	var args authentication.AuthenticateArgs
	if err := httputils.DecodeJSON(r, &args); err != nil {
		httputils.WriteError(w, http.StatusBadRequest, "invalid request body")
		return
	}

	if args.AccountID == "" {
		httputils.WriteError(w, http.StatusBadRequest, "invalid account id")
		return
	}
	if args.AccessToken == "" {
		httputils.WriteError(w, http.StatusBadRequest, "invalid access token")
		return
	}

	res, err := h.authHandler.Authenticate(&args)
	if err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, err.Error())
		return
	}

	httputils.WriteJSON(w, http.StatusOK, res)
}
