# Dream State

> Tracks the state of the last dream consolidation pass.
> Read by `@dev` at session start to decide whether to trigger `@dream`.

## Gates

| Gate | Value |
|------|-------|
| `lastDreamDate` | 2026-04-27 |
| `sessionsSinceLastDream` | 3 |

## Rules

- **Time gate:** ≥ 24h since `lastDreamDate`
- **Session gate:** `sessionsSinceLastDream` ≥ 5
- **Both gates** must pass to trigger a dream.
- `@dev` must serialize dream runs with an exclusive lock at `$env:TEMP\infra-pipeline-editor-dream-lock` before invoking `@dream`.
- If that lock already exists, another agent owns the current dream cycle and the current agent must skip `@dream`.
- Any duplicate `@dream` run that observes `lastDreamDate` = today and `sessionsSinceLastDream` = 0 must exit without modifying other memory files.
- After a dream completes, reset `sessionsSinceLastDream` to 0 and update `lastDreamDate` to today.
