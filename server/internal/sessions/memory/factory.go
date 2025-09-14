package memory

import "technical-test-backend/internal/worker"

func CreateSessionPool(config SessionPoolConfig) (*SessionPool, func()) {
	sp := NewSessionPool(config)
	cleanupWorker := worker.New(worker.Config{
		Interval: config.TTL,
	}, func() {
		sp.CleanupExpiredSessions()
	})

	cleanupWorker.Start()

	return sp, func() {
		cleanupWorker.Stop()
	}
}
