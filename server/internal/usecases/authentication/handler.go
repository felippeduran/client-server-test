package authentication

import (
	"fmt"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/session"
	"technical-test-backend/internal/usecases/players"
	"time"
)

type AuthenticateArgs struct {
	AccountID   string `json:"accountId"`
	AccessToken string `json:"accessToken"`
}

type AuthenticateRes struct {
	SessionID string `json:"sessionId"`
}

type Handler struct {
	sessionPool session.Pool
	dal         players.AccountDAL
}

func NewHandler(sessionPool session.Pool, dal players.AccountDAL) *Handler {
	return &Handler{
		sessionPool: sessionPool,
		dal:         dal,
	}
}

func (h *Handler) Authenticate(args *AuthenticateArgs) (*AuthenticateRes, error) {
	if args.AccountID == "" {
		return nil, fmt.Errorf("missing account id argument")
	}
	if args.AccessToken == "" {
		return nil, fmt.Errorf("missing access token argument")
	}

	accessToken, err := h.dal.GetAccessToken(args.AccountID)
	if err != nil {
		if err.Error() == "account not found" {
			account := players.Account{
				ID:          args.AccountID,
				AccessToken: args.AccessToken,
			}
			if err := h.dal.CreateAccount(account, createInitialState()); err != nil {
				return nil, fmt.Errorf("failed to create account: %v", err)
			}
			accessToken = args.AccessToken
		} else {
			return nil, fmt.Errorf("failed to get account: %v", err)
		}
	}

	if accessToken != args.AccessToken {
		return nil, fmt.Errorf("invalid access token")
	}

	sess, err := h.sessionPool.CreateSession(args.AccountID, core.SessionState{})
	if err != nil {
		return nil, fmt.Errorf("failed to create session: %v", err)
	}

	return &AuthenticateRes{
		SessionID: sess.ID,
	}, nil
}

func createInitialState() core.PersistentState {
	return core.PersistentState{
		Energy: core.Energy{
			CurrentAmount:  1,
			LastRechargeAt: time.Now(),
		},
		LevelProgression: core.LevelProgression{
			CurrentLevel: 1,
			Statistics:   []core.LevelStats{},
		},
	}
}
