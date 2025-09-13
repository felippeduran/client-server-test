package main

import (
	"fmt"
	"log"
	"technical-test-backend/internal/configs"
	"technical-test-backend/internal/core/commands"
	sessionmemory "technical-test-backend/internal/session/memory"
	"technical-test-backend/internal/usecases/authentication"
	usecasescommands "technical-test-backend/internal/usecases/commands"
	usecasesconfigs "technical-test-backend/internal/usecases/configs"
	"technical-test-backend/internal/usecases/players"
	playersmemory "technical-test-backend/internal/usecases/players/dal/memory"
	"time"
)

// Example usage of the core functionality
func main() {
	// Create dependencies
	sessionPool, close := sessionmemory.CreateSessionPool(sessionmemory.SessionPoolConfig{
		TTL: 30 * time.Second,
	})
	defer close()

	accountsDal := playersmemory.NewDAL()
	configs := configs.NewProvider(configs.ProviderConfig{
		ConfigFilePath: "config/game_config.json",
	})

	// Create handlers
	authHandler := authentication.NewHandler(sessionPool, accountsDal)
	stateHandler := players.NewStateHandler(sessionPool, accountsDal)
	configHandler := usecasesconfigs.NewHandler(sessionPool, configs)
	commandHandler := usecasescommands.NewHandler(sessionPool, accountsDal, configs)

	// Example: Authenticate
	authArgs := &authentication.AuthenticateArgs{
		AccountID:   "test-account-123",
		AccessToken: "test-token-456",
	}

	authRes, authErr := authHandler.Authenticate(authArgs)
	if authErr != nil {
		log.Fatalf("Authentication failed: %v", authErr)
	}
	fmt.Printf("Authentication successful: %+v\n", authRes)

	// Example: Get player state
	playerStateRes, playerStateErr := stateHandler.GetPlayerState(authRes.SessionID, &players.GetPlayerStateArgs{})
	if playerStateErr != nil {
		log.Fatalf("Get player state failed: %v", playerStateErr)
	}
	fmt.Printf("Player state: Level %d, Energy %d\n",
		playerStateRes.PlayerState.Persistent.LevelProgression.CurrentLevel,
		playerStateRes.PlayerState.Persistent.Energy.CurrentAmount)

	// Example: Get configs
	configsRes, configsErr := configHandler.GetConfigs(authRes.SessionID, &usecasesconfigs.GetConfigsArgs{})
	if configsErr != nil {
		log.Fatalf("Get configs failed: %v", configsErr)
	}
	fmt.Printf("Configs loaded: %d levels, Max Energy %d\n",
		len(configsRes.Configs.Levels),
		configsRes.Configs.Energy.MaxEnergy)

	// Example: Begin level command
	beginLevelCmd := &commands.BeginLevel{
		LevelID: 1,
		Now:     time.Now(),
	}

	beginErr := commandHandler.Handle(authRes.SessionID, beginLevelCmd)
	if beginErr != nil {
		log.Fatalf("Begin level command failed: %v", beginErr)
	}
	fmt.Println("Begin level command executed successfully")

	// Example: End level command
	endLevelCmd := &commands.EndLevel{
		Success: true,
		Score:   100,
	}

	endErr := commandHandler.Handle(authRes.SessionID, endLevelCmd)
	if endErr != nil {
		log.Fatalf("End level command failed: %v", endErr)
	}
	fmt.Println("End level command executed successfully")

	// Example: Get updated player state
	updatedPlayerStateRes, updatedPlayerStateErr := stateHandler.GetPlayerState(authRes.SessionID, &players.GetPlayerStateArgs{})
	if updatedPlayerStateErr != nil {
		log.Fatalf("Get updated player state failed: %v", updatedPlayerStateErr)
	}
	fmt.Printf("Updated player state: Level %d, Energy %d\n",
		updatedPlayerStateRes.PlayerState.Persistent.LevelProgression.CurrentLevel,
		updatedPlayerStateRes.PlayerState.Persistent.Energy.CurrentAmount)

	// Example: Check session pool stats
	fmt.Printf("Session pool stats: %d active sessions, %d accounts\n", sessionPool.GetSessionCount(), accountsDal.GetAccountCount())

	fmt.Println("Server stopped")
}
