package memory

import (
	"fmt"
	"sync"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/usecases/players"
)

type DAL struct {
	accounts map[string]AccountData
	mutex    sync.RWMutex
}

type AccountData struct {
	Account         players.Account      `json:"account"`
	PersistentState core.PersistentState `json:"persistentState"`
}

func NewDAL() *DAL {
	return &DAL{
		accounts: make(map[string]AccountData),
	}
}

func (d *DAL) CreateAccount(account players.Account, state core.PersistentState) error {
	d.mutex.Lock()
	defer d.mutex.Unlock()

	if _, exists := d.accounts[account.ID]; exists {
		return fmt.Errorf("account already exists")
	}

	accountData := AccountData{
		Account:         account,
		PersistentState: state,
	}

	d.accounts[account.ID] = accountData
	return nil
}

func (d *DAL) GetAccessToken(accountID string) (string, error) {
	d.mutex.RLock()
	defer d.mutex.RUnlock()

	accountData, exists := d.accounts[accountID]
	if !exists {
		return "", fmt.Errorf("account not found")
	}

	return accountData.Account.AccessToken, nil
}

func (d *DAL) GetPersistentState(accountID string) (core.PersistentState, error) {
	d.mutex.RLock()
	defer d.mutex.RUnlock()

	accountData, exists := d.accounts[accountID]
	if !exists {
		return core.PersistentState{}, fmt.Errorf("account not found")
	}

	return accountData.PersistentState, nil
}

func (d *DAL) SetPersistentState(accountID string, state core.PersistentState) error {
	d.mutex.Lock()
	defer d.mutex.Unlock()

	accountData, exists := d.accounts[accountID]
	if !exists {
		return fmt.Errorf("account not found")
	}

	accountData.PersistentState = state
	return nil
}

func (d *DAL) GetAccountCount() int {
	d.mutex.RLock()
	defer d.mutex.RUnlock()

	return len(d.accounts)
}
