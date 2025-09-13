package worker

import "time"

type Config struct {
	Interval time.Duration
}

type impl struct {
	config Config
	f      func()
	ticker *time.Ticker
}

func New(config Config, f func()) *impl {
	return &impl{
		config: config,
		f:      f,
	}
}

func (w *impl) Start() {
	w.ticker = time.NewTicker(w.config.Interval)
	go func() {
		for range w.ticker.C {
			w.f()
		}
	}()
}

func (w *impl) Stop() {
	if w.ticker != nil {
		w.ticker.Stop()
	}
}
