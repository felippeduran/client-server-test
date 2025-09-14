package authentication

import (
	"fmt"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/usecases/players"
	"time"
)

type AuthenticateArgs struct {
	AccountID   string `json:"accountId"`
	AccessToken string `json:"accessToken"`
}

type AuthenticateRes struct{}

type Handler struct {
	dal players.AccountDAL
}

func NewHandler(dal players.AccountDAL) *Handler {
	return &Handler{
		dal: dal,
	}
}

func (h *Handler) Authenticate(args *AuthenticateArgs) (*AuthenticateRes, error) {
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

	return &AuthenticateRes{}, nil
}

func createInitialState() core.PersistentState {
	return core.PersistentState{
		Energy: core.Energy{
			CurrentAmount:  5,
			LastRechargeAt: time.Now().UTC(),
		},
		LevelProgression: core.LevelProgression{
			CurrentLevel: 1,
			Statistics:   []core.LevelStats{},
		},
	}
}
