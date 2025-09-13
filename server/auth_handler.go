package main

import (
	"fmt"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/session"
	"time"
)

// AuthenticateArgs represents authentication request
type AuthenticateArgs struct {
	AccountID   string `json:"accountId"`
	AccessToken string `json:"accessToken"`
}

// AuthenticateRes represents authentication response
type AuthenticateRes struct {
	SessionID string `json:"sessionId"`
}

// AuthenticationHandler handles authentication requests
type AuthenticationHandler struct {
	sessionPool session.Pool
	dal         *DAL
}

// NewAuthenticationHandler creates a new authentication handler
func NewAuthenticationHandler(sessionPool session.Pool, dal *DAL) *AuthenticationHandler {
	return &AuthenticationHandler{
		sessionPool: sessionPool,
		dal:         dal,
	}
}

// Authenticate handles user authentication
func (h *AuthenticationHandler) Authenticate(args *AuthenticateArgs) (*AuthenticateRes, error) {
	// Validate input
	if args.AccountID == "" {
		return nil, fmt.Errorf("missing account id argument")
	}
	if args.AccessToken == "" {
		return nil, fmt.Errorf("missing access token argument")
	}

	// Check if account exists
	accessToken, err := h.dal.GetAccessToken(args.AccountID)
	if err != nil {
		// Account doesn't exist, create it
		if err.Error() == "account not found" {
			account := Account{
				ID:          args.AccountID,
				AccessToken: args.AccessToken,
			}
			initialState := core.PersistentState{
				Energy: core.Energy{
					CurrentAmount:  1,
					LastRechargeAt: time.Now(),
				},
				LevelProgression: core.LevelProgression{
					CurrentLevel: 1,
					Statistics:   []core.LevelStats{},
				},
			}
			createErr := h.dal.CreateAccount(account, initialState)
			if createErr != nil {
				return nil, fmt.Errorf("failed to create account: %v", createErr)
			}
			accessToken = args.AccessToken
		} else {
			return nil, fmt.Errorf("failed to get account: %v", err)
		}
	}

	// Validate access token
	if accessToken != args.AccessToken {
		return nil, fmt.Errorf("invalid access token")
	}

	// Get session
	sess, err := h.sessionPool.CreateSession(args.AccountID, core.SessionState{})
	if err != nil {
		return nil, fmt.Errorf("failed to create session: %v", err)
	}

	return &AuthenticateRes{
		SessionID: sess.ID,
	}, nil
}
