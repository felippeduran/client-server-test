package errors

import (
	"fmt"

	"github.com/pkg/errors"
)

func New(message string) error {
	return errors.New(message)
}

func Wrapf(err error, format string, args ...interface{}) error {
	return errors.Wrap(err, fmt.Sprintf(format, args...))
}

func Wrap(err error, externalErr error) error {
	return errors.WithStack(fmt.Errorf("%w: %w", err, externalErr))
}

func Errorf(format string, args ...interface{}) error {
	return errors.WithStack(fmt.Errorf(format, args...))
}

func Is(err, target error) bool {
	return errors.Is(err, target)
}

func As(err error, target interface{}) bool {
	return errors.As(err, target)
}
