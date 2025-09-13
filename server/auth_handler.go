package main

import (
	"fmt"
	"time"
)

// AuthenticateArgs represents authentication request
type AuthenticateArgs struct {
	AccountID   string `json:"accountId"`
	AccessToken string `json:"accessToken"`
}

// AuthenticateRes represents authentication response
type AuthenticateRes struct{}

// AuthenticationHandler handles authentication requests
type AuthenticationHandler struct {
	sessionPool *SessionPool
	dal         *DAL
}

// NewAuthenticationHandler creates a new authentication handler
func NewAuthenticationHandler(sessionPool *SessionPool, dal *DAL) *AuthenticationHandler {
	return &AuthenticationHandler{
		sessionPool: sessionPool,
		dal:         dal,
	}
}

// Authenticate handles user authentication
func (h *AuthenticationHandler) Authenticate(sessionID string, args *AuthenticateArgs) (*AuthenticateRes, *Error) {
	// Validate input
	if args.AccountID == "" {
		return nil, &Error{Message: "missing account id argument"}
	}
	if args.AccessToken == "" {
		return nil, &Error{Message: "missing access token argument"}
	}

	// Get session
	session, exists := h.sessionPool.GetSession(sessionID)
	if !exists {
		return nil, &Error{Message: "session not found"}
	}

	// Check if session is already authenticated with a different account
	if session.Authenticated && session.AccountID != args.AccountID {
		return nil, &Error{Message: "connection already assigned to another account"}
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
			initialState := PersistentState{
				Energy: Energy{
					CurrentAmount:  1,
					LastRechargeAt: time.Now(),
				},
				LevelProgression: LevelProgression{
					CurrentLevel: 1,
					Statistics:   []LevelStats{},
				},
			}
			createErr := h.dal.CreateAccount(account, initialState)
			if createErr != nil {
				return nil, &Error{Message: fmt.Sprintf("failed to create account: %v", createErr)}
			}
			accessToken = args.AccessToken
		} else {
			return nil, &Error{Message: fmt.Sprintf("failed to get account: %v", err)}
		}
	}

	// Validate access token
	if accessToken != args.AccessToken {
		return nil, &Error{Message: "Invalid access token"}
	}

	// Authenticate session
	authErr := h.sessionPool.AuthenticateSession(sessionID, args.AccountID)
	if authErr != nil {
		return nil, &Error{Message: fmt.Sprintf("failed to authenticate session: %v", authErr)}
	}

	return &AuthenticateRes{}, nil
}
