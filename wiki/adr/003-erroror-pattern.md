# ADR-003: ErrorOr Over Exceptions

## Status
Accepted

## Context

Business operations can fail for expected reasons (resource not found, validation failure, access denied). Using exceptions for these cases is:
- Expensive (stack unwinding)
- Non-local (control flow jumps unpredictably)
- Hard to compose (try/catch nesting)
- Loses error type information

## Decision

Use the **ErrorOr** library as the return type for all business operations.

- All handlers return `ErrorOr<TResponse>`
- Domain errors are defined as static `Error` factories
- No exceptions for business logic
- Exceptions reserved for truly unexpected infrastructure failures

## Consequences

### Positive
- Explicit error handling at every call site
- Errors have typed codes and descriptions
- Easy to map to HTTP status codes
- Composable (chain operations, short-circuit)
- Performance: no exception overhead for expected failures

### Negative
- More verbose than simply throwing
- Every caller must check `IsError`
- Can't use standard try/catch patterns

### Risks
- Developers might still throw exceptions out of habit (mitigated: code review)
