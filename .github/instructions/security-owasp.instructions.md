---
description: "Security baseline for API and backend code with OWASP-focused checks. Use when handling authentication, input validation, data access, or external calls."
applyTo: "**/*.{cs,json,yml,yaml}"
---

# Security and OWASP Instructions

## Input Validation

- Treat all external input as untrusted.
- Validate shape, length, and allowed values.
- Reject invalid input with clear but non-sensitive messages.

## Secrets and Configuration

- Never hardcode secrets, tokens, or connection strings.
- Use environment variables or secure secret stores.
- Keep development and production settings separate.

## Error Handling

- Do not return stack traces or internal exception details to clients.
- Log full diagnostic details server-side.
- Return safe error payloads to callers.

## Data Access and Injection Defense

- Prefer parameterized queries and ORM-safe APIs.
- Avoid dynamic SQL string concatenation.
- Validate sort/filter inputs if used in query composition.

## AuthZ and AuthN

- Enforce authorization checks at endpoint/service boundaries.
- Apply least privilege principles for service credentials.

## Logging and Auditing

- Log security-relevant actions (auth failures, forbidden access, high-risk changes).
- Redact sensitive fields from logs.
