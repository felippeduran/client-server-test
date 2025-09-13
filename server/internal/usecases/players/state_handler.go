package players

import (
	"fmt"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/session"
	"time"
)

type GetPlayerStateArgs struct{}

type GetPlayerStateRes struct {
	PlayerState core.PlayerState `json:"playerState"`
	ServerTime  time.Time        `json:"serverTime"`
}

type StateHandler struct {
	sessionPool session.Pool
	dal         StateDAL
}

func NewStateHandler(sessionPool session.Pool, dal StateDAL) *StateHandler {
	return &StateHandler{
		sessionPool: sessionPool,
		dal:         dal,
	}
}

func (h *StateHandler) GetPlayerState(sessionID string, args *GetPlayerStateArgs) (*GetPlayerStateRes, error) {
	accountID, authenticated := h.sessionPool.GetAccountID(sessionID)
	if !authenticated {
		return nil, fmt.Errorf("connection not authenticated")
	}

	persistentState, err := h.dal.GetPersistentState(accountID)
	if err != nil {
		return nil, fmt.Errorf("failed to get persistent state: %v", err)
	}

	var sessionState core.SessionState
	if err := h.sessionPool.GetSessionData(sessionID, &sessionState); err != nil {
		return nil, fmt.Errorf("session not found")
	}

	playerState := core.PlayerState{
		Persistent: persistentState,
		Session:    sessionState,
	}

	h.sessionPool.UpdateActivity(sessionID)

	return &GetPlayerStateRes{
		PlayerState: playerState,
		ServerTime:  time.Now(),
	}, nil
}
