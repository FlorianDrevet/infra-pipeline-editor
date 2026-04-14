#!/bin/bash
# =============================================================================
# InfraFlowSculptor — Create GitHub Labels for Audit Issues
# Run: chmod +x scripts/create-audit-labels.sh && ./scripts/create-audit-labels.sh
# Requires: gh CLI authenticated (gh auth login)
# =============================================================================

set -euo pipefail

REPO="FlorianDrevet/infra-pipeline-editor"

echo "🏷️  Creating labels for audit issues..."

# Severity labels
gh label create "severity: critical"  --color "B60205" --description "Must fix immediately — security, data integrity, or crash risk"    --repo "$REPO" --force
gh label create "severity: high"      --color "D93F0B" --description "Fix within Sprint 1 — significant quality or performance risk"      --repo "$REPO" --force
gh label create "severity: medium"    --color "FBCA04" --description "Fix within Sprint 2-3 — code quality, consistency, maintainability" --repo "$REPO" --force
gh label create "severity: low"       --color "0E8A16" --description "Backlog — minor improvements, naming, style"                       --repo "$REPO" --force

# Category labels
gh label create "area: security"      --color "E11D48" --description "Security vulnerability or hardening"             --repo "$REPO" --force
gh label create "area: database"      --color "7C3AED" --description "EF Core, migrations, indexes, constraints"       --repo "$REPO" --force
gh label create "area: domain"        --color "2563EB" --description "DDD aggregates, value objects, domain model"     --repo "$REPO" --force
gh label create "area: application"   --color "0891B2" --description "CQRS handlers, validators, behaviors"            --repo "$REPO" --force
gh label create "area: api"           --color "059669" --description "Endpoints, contracts, OpenAPI, middleware"        --repo "$REPO" --force
gh label create "area: generation"    --color "D97706" --description "Bicep & Pipeline generation engines"             --repo "$REPO" --force
gh label create "area: infrastructure" --color "6B7280" --description "Repositories, DI, external services"            --repo "$REPO" --force
gh label create "area: testing"       --color "EC4899" --description "Unit tests, integration tests, coverage"         --repo "$REPO" --force

# Type labels
gh label create "type: bug"           --color "D73A4A" --description "Something isn't working correctly"               --repo "$REPO" --force
gh label create "type: refactor"      --color "A2EEEF" --description "Code improvement without behavior change"        --repo "$REPO" --force
gh label create "type: performance"   --color "F9D0C4" --description "Performance improvement"                         --repo "$REPO" --force
gh label create "type: tech-debt"     --color "C5DEF5" --description "Technical debt reduction"                        --repo "$REPO" --force

# Phase labels (from audit plan)
gh label create "phase: 0-foundation" --color "1D1D1D" --description "Phase 0 — Test infrastructure (before any refactoring)" --repo "$REPO" --force
gh label create "phase: 1-security"   --color "B60205" --description "Phase 1 — Critical security fixes (Sprint 1)"           --repo "$REPO" --force
gh label create "phase: 2-database"   --color "7C3AED" --description "Phase 2 — Database hardening (Sprint 1-2)"              --repo "$REPO" --force
gh label create "phase: 3-domain"     --color "2563EB" --description "Phase 3 — Domain model fixes (Sprint 2)"                --repo "$REPO" --force
gh label create "phase: 4-application" --color "0891B2" --description "Phase 4 — Application layer fixes (Sprint 2-3)"        --repo "$REPO" --force
gh label create "phase: 5-api"        --color "059669" --description "Phase 5 — API & Contracts (Sprint 3)"                   --repo "$REPO" --force
gh label create "phase: 6-generation" --color "D97706" --description "Phase 6 — Bicep/Pipeline engines (Sprint 3-4)"          --repo "$REPO" --force
gh label create "phase: 7-quality"    --color "6B7280" --description "Phase 7 — Global quality (Sprint 4+)"                   --repo "$REPO" --force

# Audit reference
gh label create "audit: 2026-04"      --color "BFD4F2" --description "From code audit of April 2026"                  --repo "$REPO" --force

echo "✅ All labels created successfully!"
