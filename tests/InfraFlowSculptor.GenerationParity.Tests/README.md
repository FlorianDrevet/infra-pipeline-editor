# Generation Parity Tests

Byte-for-byte golden-file safety net for the Bicep and Pipeline generation engines.
Used to lock current engine output during the **V2 multi-repo topology** refactor
(steps A2–A3–D1). Any output drift fails these tests — verify the drift is
intentional before regenerating goldens.

## Run

```powershell
dotnet test .\tests\InfraFlowSculptor.GenerationParity.Tests\
```

## Regenerate goldens (intentional drift only)

```powershell
dotnet test .\tests\InfraFlowSculptor.GenerationParity.Tests\ -p:DefineConstants=REGENERATE_GOLDENS
```

When this symbol is defined, `GoldenComparer.AssertMatchesGolden` wipes and
rewrites `Fixtures/<fixture>/golden/` in the **source tree** (resolved via
`[CallerFilePath]`), then returns without asserting. Review the diff, commit,
then run tests without the symbol to confirm green.

## Conventions

- Newlines are normalized to `LF` before comparison.
- Fixtures are built in code (see `FixtureBuilders.cs`) — no DB, no DI, no Domain.
- Golden file paths mirror the engine output exactly (e.g. `common/functions.bicep`,
  `api/main.bicep`, `variables/dev.yml`).
