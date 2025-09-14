package players

import (
	"fmt"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/usecases"
	"time"
)

type GetPlayerStateArgs struct{}

type GetPlayerStateRes struct {
	PlayerState PlayerState `json:"playerState"`
	ServerTime  time.Time   `json:"serverTime"`
}

type PlayerState struct {
	Persistent core.PersistentState `json:"persistent"`
	Session    core.SessionState    `json:"session"`
}

type StateHandler struct {
	dal StateDAL
}

func NewStateHandler(dal StateDAL) *StateHandler {
	return &StateHandler{
		dal: dal,
	}
}

func (h *StateHandler) GetPlayerState(sessionData usecases.SessionData, args *GetPlayerStateArgs) (*GetPlayerStateRes, error) {
	persistentState, err := h.dal.GetPersistentState(sessionData.AccountID)
	if err != nil {
		return nil, fmt.Errorf("failed to get persistent state: %v", err)
	}

	return &GetPlayerStateRes{
		PlayerState: PlayerState{
			Persistent: persistentState,
			Session:    *sessionData.SessionState,
		},
		ServerTime: time.Now().UTC(),
	}, nil
}
