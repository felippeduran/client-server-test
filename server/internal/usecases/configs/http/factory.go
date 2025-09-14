package http

import "technical-test-backend/internal/usecases/configs"

func CreateHTTPHandler(configsProvider *configs.Provider) *Handler {
	return NewHandler(configs.NewHandler(configsProvider))
}
