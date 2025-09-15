package http

import (
	"net/http"
	"technical-test-backend/internal/errors"
	httputils "technical-test-backend/internal/http"
	"technical-test-backend/internal/sessions"
	"technical-test-backend/internal/usecases/authentication"

	"github.com/google/uuid"
)

var (
	ErrInvalidAccountID   = errors.New("invalid account id")
	ErrInvalidAccessToken = errors.New("invalid access token")
	ErrInvalidRequestBody = errors.New("invalid request body")
)

type Handler struct {
	authHandler *authentication.Handler
	sessionPool sessions.Pool
}

func NewHandler(authHandler *authentication.Handler, sessionPool sessions.Pool) *Handler {
	return &Handler{
		authHandler: authHandler,
		sessionPool: sessionPool,
	}
}

func (h *Handler) HandleAuthenticate(w http.ResponseWriter, r *http.Request) {
	var args authentication.AuthenticateArgs
	if err := httputils.DecodeJSON(r, &args); err != nil {
		httputils.WriteError(w, http.StatusBadRequest, ErrInvalidRequestBody.Error())
		return
	}

	if args.AccountID == "" {
		httputils.WriteError(w, http.StatusBadRequest, ErrInvalidAccountID.Error())
		return
	}

	if _, err := uuid.Parse(args.AccountID); err != nil {
		httputils.WriteError(w, http.StatusBadRequest, ErrInvalidAccountID.Error())
		return
	}

	if args.AccessToken == "" {
		httputils.WriteError(w, http.StatusBadRequest, ErrInvalidAccessToken.Error())
		return
	}

	if _, err := uuid.Parse(args.AccessToken); err != nil {
		httputils.WriteError(w, http.StatusBadRequest, ErrInvalidAccessToken.Error())
		return
	}

	res, err := h.authHandler.Authenticate(&args)
	if err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, err.Error())
		return
	}

	sess, err := h.sessionPool.CreateSession(args.AccountID, nil)
	if err != nil {
		httputils.WriteError(w, http.StatusInternalServerError, err.Error())
		return
	}

	w.Header().Set("X-Session-ID", sess.ID)

	httputils.WriteJSON(w, http.StatusOK, res)
}
