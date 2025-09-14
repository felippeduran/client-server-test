package commands

import (
	"encoding/json"
	"fmt"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/errors"
	"technical-test-backend/internal/session"
	"technical-test-backend/internal/usecases/configs"
	"technical-test-backend/internal/usecases/players"
	"time"
)

var (
	ErrCommandTimestampTooFar  = errors.New("command timestamp is too far")
	ErrCommandExecutionFailure = errors.New("command execution failed")
)

type SessionData struct {
	AccountID    string
	SessionState *core.SessionState
}

type Config struct {
	MaxTimeDifferenceSeconds float64
}

func (c *Config) MaxTimeDifference() time.Duration {
	return time.Duration(c.MaxTimeDifferenceSeconds) * time.Second
}

type CommandArgs struct {
	Command string          `json:"command"`
	Data    json.RawMessage `json:"data"`
}

type Handler struct {
	config          Config
	sessionPool     session.Pool
	dal             players.StateDAL
	configsProvider *configs.Provider
}

func NewHandler(config Config, sessionPool session.Pool, dal players.StateDAL, configsProvider *configs.Provider) *Handler {
	return &Handler{
		config:          config,
		sessionPool:     sessionPool,
		dal:             dal,
		configsProvider: configsProvider,
	}
}

func (h *Handler) Handle(sessionData SessionData, command core.Command) error {
	if timedCmd, ok := command.(core.TimedCommand); ok {
		now := time.Now().UTC()
		timeDifference := timedCmd.GetTimestamp().Sub(now)
		if timeDifference > h.config.MaxTimeDifference() {
			return ErrCommandTimestampTooFar
		}
	}

	// Get persistent state
	persistentState, err := h.dal.GetPersistentState(sessionData.AccountID)
	if err != nil {
		return fmt.Errorf("failed to get persistent state: %v", err)
	}

	// Create player state
	playerState := core.PlayerState{
		Persistent: &persistentState,
		Session:    sessionData.SessionState,
	}

	// Get configs
	configs, err := h.configsProvider.GetConfigs()
	if err != nil {
		return fmt.Errorf("failed to load configs: %v", err)
	}

	// Execute command
	err = command.Execute(&playerState, configs)
	if err != nil {
		return errors.Wrap(err, ErrCommandExecutionFailure)
	}

	// Update persistent state
	err = h.dal.SetPersistentState(sessionData.AccountID, *playerState.Persistent)
	if err != nil {
		return fmt.Errorf("failed to save persistent state: %v", err)
	}

	// Update session state
	sessionData.SessionState.CurrentLevelID = playerState.Session.CurrentLevelID

	return nil
}
