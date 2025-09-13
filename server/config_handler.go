package main

// GetConfigsArgs represents get configs request
type GetConfigsArgs struct{}

// GetConfigsRes represents get configs response
type GetConfigsRes struct {
	Configs Configs `json:"configs"`
}

// ConfigHandler handles configuration requests
type ConfigHandler struct {
	sessionPool *SessionPool
	configs     *ConfigsProvider
}

// NewConfigHandler creates a new config handler
func NewConfigHandler(sessionPool *SessionPool, configs *ConfigsProvider) *ConfigHandler {
	return &ConfigHandler{
		sessionPool: sessionPool,
		configs:     configs,
	}
}

// GetConfigs retrieves game configuration
func (h *ConfigHandler) GetConfigs(sessionID string, args *GetConfigsArgs) (*GetConfigsRes, *Error) {
	// Check authentication
	_, authenticated := h.sessionPool.GetAccountID(sessionID)
	if !authenticated {
		return nil, &Error{Message: "connection not authenticated"}
	}

	// Get configs
	configs := h.configs.GetHardcodedConfigs()

	// Update activity
	h.sessionPool.UpdateActivity(sessionID)

	return &GetConfigsRes{
		Configs: *configs,
	}, nil
}
