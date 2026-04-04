# Dream State

> Tracks the state of the last dream consolidation pass.
> Read by `@dev` at session start to decide whether to trigger `@dream`.

## Gates

| Gate | Value |
|------|-------|
| `lastDreamDate` | 2026-04-04 |
| `sessionsSinceLastDream` | 2 |

## Rules

- **Time gate:** ≥ 24h since `lastDreamDate`
- **Session gate:** `sessionsSinceLastDream` ≥ 5
- **Both gates** must pass to trigger a dream.
- After a dream completes, reset `sessionsSinceLastDream` to 0 and update `lastDreamDate` to today.
