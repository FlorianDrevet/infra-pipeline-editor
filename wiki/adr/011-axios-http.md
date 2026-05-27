# ADR-011: Axios Over HttpClient

## Status
Accepted

## Context

Angular's built-in `HttpClient` uses Observables (RxJS) for all HTTP operations. Our frontend uses signals for state and promises for async operations. Mixing Observables and Promises creates inconsistency and unnecessary complexity.

Options considered:
1. **Angular HttpClient** — Built-in, Observable-based
2. **Axios** — Promise-based, interceptors, widely used
3. **Fetch API** — Native, no library needed
4. **ky** — Modern fetch wrapper

## Decision

Use **Axios** as the HTTP client library.

## Consequences

### Positive
- Promise-based: consistent with signals + async/await
- Interceptors for auth token attachment
- Request cancellation via AbortController
- Consistent API across browser and Node.js
- Smaller learning curve for developers from other frameworks

### Negative
- Additional dependency (not built-in)
- Loses Angular's testing utilities for HttpClient
- No automatic HttpInterceptor integration
- Must manually handle response typing

### Risks
- Axios maintenance (mitigated: extremely popular, actively maintained)
- Missing Angular-specific features like transfer state (mitigated: not needed for SPA)
