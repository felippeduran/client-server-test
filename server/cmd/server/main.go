package main

import (
	"technical-test-backend/internal/app"
	"technical-test-backend/internal/session/memory"
	"technical-test-backend/internal/usecases/configs"
	"time"
)

func main() {
	app := app.NewHTTP(app.Config{
		SessionPool: memory.SessionPoolConfig{
			TTL: 30 * time.Second,
		},
		GameConfig: configs.ProviderConfig{
			FilePath: "config/game_config.json",
		},
	})

	app.Run()
}
