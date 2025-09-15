//go:build unit
// +build unit

package memory

import (
	"fmt"
	"sync"
	"testing"
	"time"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

type TestData struct {
	Level int
	Score float64
}

func TestCreateSession(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 30 * time.Minute,
	}
	sp := NewSessionPool(config)

	t.Run("successful creation", func(t *testing.T) {
		accountID := "test-account-123"
		testData := map[string]interface{}{"level": 1}

		session, err := sp.CreateSession(accountID, testData)

		require.NoError(t, err)
		assert.NotEmpty(t, session.ID)
		assert.Equal(t, accountID, session.AccountID)
		assert.True(t, time.Since(session.CreatedAt) < time.Second)
		assert.True(t, time.Since(session.LastActivity) < time.Second)
	})

	t.Run("marshal error", func(t *testing.T) {
		accountID := "test-account-456"
		invalidData := make(chan int)

		session, err := sp.CreateSession(accountID, invalidData)

		assert.ErrorIs(t, err, ErrFailedToMarshalData)
		assert.Empty(t, session.ID)
	})

	t.Run("empty account ID", func(t *testing.T) {
		_, err := sp.CreateSession("", map[string]interface{}{"level": 1})

		require.NoError(t, err)
	})
}

func TestGetSession(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 100 * time.Millisecond,
	}
	sp := NewSessionPool(config)

	t.Run("existing session", func(t *testing.T) {
		accountID := "test-account-123"
		testData := map[string]interface{}{"level": 1}

		session, err := sp.CreateSession(accountID, testData)
		require.NoError(t, err)

		retrievedSession, exists := sp.GetSession(session.ID)

		assert.True(t, exists)
		assert.Equal(t, session, retrievedSession)
	})

	t.Run("non-existent session", func(t *testing.T) {
		nonExistentID := uuid.New().String()

		session, exists := sp.GetSession(nonExistentID)

		assert.False(t, exists)
		assert.Empty(t, session.ID)
	})

	t.Run("expired session", func(t *testing.T) {
		accountID := "test-account-expired"
		testData := map[string]interface{}{"level": 1}

		session, err := sp.CreateSession(accountID, testData)
		require.NoError(t, err)

		time.Sleep(150 * time.Millisecond)

		retrievedSession, exists := sp.GetSession(session.ID)

		assert.False(t, exists)
		assert.Empty(t, retrievedSession.ID)
	})

	t.Run("with nil data", func(t *testing.T) {
		accountID := "test-account-nil"
		session, err := sp.CreateSession(accountID, nil)

		require.NoError(t, err)
		assert.NotEmpty(t, session.ID)

		var retrievedData TestData
		err = sp.GetSessionData(accountID, &retrievedData)
		require.NoError(t, err)
		assert.NotNil(t, retrievedData)
	})
}

func TestGetSessionData(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 30 * time.Minute,
	}
	sp := NewSessionPool(config)

	t.Run("successful retrieval", func(t *testing.T) {
		accountID := "test-account-123"
		expectedData := TestData{
			Level: 5,
			Score: 2500,
		}

		_, err := sp.CreateSession(accountID, expectedData)
		require.NoError(t, err)

		var retrievedData TestData
		err = sp.GetSessionData(accountID, &retrievedData)

		require.NoError(t, err)
		assert.Equal(t, expectedData, retrievedData)
	})

	t.Run("non-existent account", func(t *testing.T) {
		nonExistentAccountID := "non-existent-account"

		var data map[string]interface{}
		err := sp.GetSessionData(nonExistentAccountID, &data)

		assert.ErrorIs(t, err, ErrSessionNotFound)
	})

	t.Run("unmarshal error", func(t *testing.T) {
		accountID := "test-account-unmarshal"
		testData := map[string]interface{}{"level": 1}

		_, err := sp.CreateSession(accountID, testData)
		require.NoError(t, err)

		var wrongType int
		err = sp.GetSessionData(accountID, &wrongType)

		assert.ErrorIs(t, err, ErrFailedToUnmarshalData)
	})
}

func TestSetSessionData(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 30 * time.Minute,
	}
	sp := NewSessionPool(config)

	t.Run("successful update", func(t *testing.T) {
		accountID := "test-account-123"
		initialData := TestData{
			Level: 1,
			Score: 100,
		}
		expectedData := TestData{
			Level: 2,
			Score: 100,
		}

		_, err := sp.CreateSession(accountID, initialData)
		require.NoError(t, err)

		err = sp.SetSessionData(accountID, expectedData)
		require.NoError(t, err)

		var retrievedData TestData
		err = sp.GetSessionData(accountID, &retrievedData)
		require.NoError(t, err)
		assert.Equal(t, expectedData, retrievedData)
	})

	t.Run("non-existent account", func(t *testing.T) {
		nonExistentAccountID := "non-existent-account"
		testData := map[string]interface{}{"level": 1}

		err := sp.SetSessionData(nonExistentAccountID, testData)

		assert.ErrorIs(t, err, ErrSessionNotFound)
	})

	t.Run("marshal error", func(t *testing.T) {
		accountID := "test-account-marshal"
		_, err := sp.CreateSession(accountID, map[string]interface{}{"level": 1})
		require.NoError(t, err)

		invalidData := make(chan int)
		err = sp.SetSessionData(accountID, invalidData)

		assert.ErrorIs(t, err, ErrFailedToMarshalData)
	})
}

func TestUpdateActivity(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 100 * time.Millisecond,
	}
	sp := NewSessionPool(config)

	t.Run("successful update", func(t *testing.T) {
		accountID := "test-account-123"
		testData := map[string]interface{}{"level": 1}

		session, err := sp.CreateSession(accountID, testData)
		require.NoError(t, err)

		originalActivity := session.LastActivity

		time.Sleep(10 * time.Millisecond)

		err = sp.UpdateActivity(session.ID)
		require.NoError(t, err)

		updatedSession, exists := sp.GetSession(session.ID)
		require.True(t, exists)
		assert.True(t, updatedSession.LastActivity.After(originalActivity))
	})

	t.Run("non-existent session", func(t *testing.T) {
		nonExistentID := uuid.New().String()

		err := sp.UpdateActivity(nonExistentID)

		assert.ErrorIs(t, err, ErrSessionNotFound)
	})

	t.Run("expired session", func(t *testing.T) {
		accountID := "test-account-expired"
		testData := map[string]interface{}{"level": 1}

		session, err := sp.CreateSession(accountID, testData)
		require.NoError(t, err)

		time.Sleep(150 * time.Millisecond)

		err = sp.UpdateActivity(session.ID)

		assert.ErrorIs(t, err, ErrSessionExpired)
	})
}

func TestRemoveSession(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 30 * time.Minute,
	}
	sp := NewSessionPool(config)

	t.Run("successful removal", func(t *testing.T) {
		accountID := "test-account-123"
		testData := map[string]interface{}{"level": 1}

		session, err := sp.CreateSession(accountID, testData)
		require.NoError(t, err)

		_, exists := sp.GetSession(session.ID)
		assert.True(t, exists)

		sp.RemoveSession(session.ID)

		_, exists = sp.GetSession(session.ID)
		assert.False(t, exists)

		err = sp.GetSessionData(accountID, &testData)
		assert.ErrorIs(t, err, ErrSessionNotFound)

		_, exists = sp.GetAccountID(session.ID)
		assert.False(t, exists)
	})

	t.Run("non-existent session", func(t *testing.T) {
		nonExistentID := uuid.New().String()

		assert.NotPanics(t, func() {
			sp.RemoveSession(nonExistentID)
		})
	})
}

func TestGetAccountID(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 100 * time.Millisecond,
	}
	sp := NewSessionPool(config)

	t.Run("existing session", func(t *testing.T) {
		accountID := "test-account-123"
		testData := map[string]interface{}{"level": 1}

		session, err := sp.CreateSession(accountID, testData)
		require.NoError(t, err)

		retrievedAccountID, exists := sp.GetAccountID(session.ID)

		assert.True(t, exists)
		assert.Equal(t, accountID, retrievedAccountID)
	})

	t.Run("non-existent session", func(t *testing.T) {
		nonExistentID := uuid.New().String()

		accountID, exists := sp.GetAccountID(nonExistentID)

		assert.False(t, exists)
		assert.Empty(t, accountID)
	})

	t.Run("expired session", func(t *testing.T) {
		accountID := "test-account-expired"
		testData := map[string]interface{}{"level": 1}

		session, err := sp.CreateSession(accountID, testData)
		require.NoError(t, err)

		time.Sleep(150 * time.Millisecond)

		retrievedAccountID, exists := sp.GetAccountID(session.ID)

		assert.False(t, exists)
		assert.Empty(t, retrievedAccountID)
	})
}

func TestCleanupExpiredSessions(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 50 * time.Millisecond,
	}
	sp := NewSessionPool(config)

	accountID1 := "test-account-1"
	accountID2 := "test-account-2"
	accountID3 := "test-account-3"

	session1, err := sp.CreateSession(accountID1, map[string]interface{}{"level": 1})
	require.NoError(t, err)

	session2, err := sp.CreateSession(accountID2, map[string]interface{}{"level": 2})
	require.NoError(t, err)

	time.Sleep(100 * time.Millisecond)

	session3, err := sp.CreateSession(accountID3, map[string]interface{}{"level": 3})
	require.NoError(t, err)

	sp.CleanupExpiredSessions()

	assert.Equal(t, 1, sp.GetSessionCount())

	_, exists := sp.GetSession(session1.ID)
	assert.False(t, exists)

	_, exists = sp.GetSession(session2.ID)
	assert.False(t, exists)

	_, exists = sp.GetSession(session3.ID)
	assert.True(t, exists)
}

func TestGetSessionCount(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 30 * time.Minute,
	}
	sp := NewSessionPool(config)

	t.Run("empty pool", func(t *testing.T) {
		count := sp.GetSessionCount()
		assert.Equal(t, 0, count)
	})

	t.Run("with sessions", func(t *testing.T) {
		accountIDs := []string{"account-1", "account-2", "account-3"}
		for _, accountID := range accountIDs {
			_, err := sp.CreateSession(accountID, map[string]interface{}{"level": 1})
			require.NoError(t, err)
		}

		count := sp.GetSessionCount()
		assert.Equal(t, len(accountIDs), count)
	})

	t.Run("after removal", func(t *testing.T) {
		accountID := "test-account-removal"
		session, err := sp.CreateSession(accountID, map[string]interface{}{"level": 1})
		require.NoError(t, err)

		initialCount := sp.GetSessionCount()

		sp.RemoveSession(session.ID)

		finalCount := sp.GetSessionCount()
		assert.Equal(t, initialCount-1, finalCount)
	})
}

func TestConcurrentAccess(t *testing.T) {
	config := SessionPoolConfig{
		TTL: 30 * time.Minute,
	}
	sp := NewSessionPool(config)

	t.Run("concurrent session creation", func(t *testing.T) {
		const numGoroutines = 100
		var wg sync.WaitGroup
		errors := make(chan error, numGoroutines)

		for i := range numGoroutines {
			wg.Add(1)
			go func(accountID string) {
				defer wg.Done()
				_, err := sp.CreateSession(accountID, map[string]interface{}{"level": 1})
				errors <- err
			}(fmt.Sprintf("account-%d", i))
		}

		wg.Wait()
		close(errors)

		for err := range errors {
			assert.NoError(t, err)
		}

		assert.Equal(t, numGoroutines, sp.GetSessionCount())
	})

	t.Run("concurrent read and write operations", func(t *testing.T) {
		accountID := "concurrent-test-account"
		session, err := sp.CreateSession(accountID, map[string]interface{}{"level": 1})
		require.NoError(t, err)

		const numOperations = 50
		var wg sync.WaitGroup
		errors := make(chan error, numOperations*2)

		for range numOperations {
			wg.Add(1)
			go func() {
				defer wg.Done()
				_, exists := sp.GetSession(session.ID)
				if !exists {
					errors <- fmt.Errorf("session not found during read")
				} else {
					errors <- nil
				}
			}()
		}

		for i := range numOperations {
			wg.Add(1)
			go func(level int) {
				defer wg.Done()
				err := sp.SetSessionData(accountID, map[string]interface{}{"level": level})
				errors <- err
			}(i)
		}

		wg.Wait()
		close(errors)

		for err := range errors {
			assert.NoError(t, err)
		}
	})
}
