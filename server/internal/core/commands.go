package core

import (
	"time"
)

// Command interfaces
type Command interface {
	Execute(state *PlayerState, configs Configs) error
}

type TimedCommand interface {
	Command
	GetTimestamp() time.Time
}
