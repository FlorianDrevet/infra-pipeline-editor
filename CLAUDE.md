<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **infra-pipeline-editor**. Full GitNexus usage instructions are in `.github/skills/gitnexus-workflow/SKILL.md` — load that skill before any code modification task.

**Quick rules:**
- MUST run `gitnexus_impact(target, "upstream")` before editing any shared symbol
- MUST run `gitnexus_detect_changes()` before committing
- MUST warn user if risk is HIGH or CRITICAL
- If index is stale: `npx gitnexus analyze`
<!-- gitnexus:end -->