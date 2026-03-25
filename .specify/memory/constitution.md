<!--
Sync Impact Report
- Version change: unspecified → 0.1.0
- Modified principles: (new) Training-First; Offline-First & Cloud Migration; Security-By-Design; Test-First Quality Gates; Simplicity & Observability
- Added sections: Constraints & Compliance; Development Workflow
- Removed sections: none
- Templates requiring updates: .specify/templates/plan-template.md ✅ reviewed (Constitution Check present); .specify/templates/spec-template.md ✅ reviewed; .specify/templates/tasks-template.md ✅ reviewed; .specify/templates/commands/*.md ⚠ pending (no commands folder)
- Follow-up TODOs: TODO(RATIFICATION_DATE) if project prefers a historical ratification date instead of initial adoption
-->

# ContosoDashboard Constitution

## Core Principles

### 1. Training-First (NON-NEGOTIABLE)
The project exists primarily for training and educational use. All code, examples, and shortcuts MUST prioritize clear pedagogy, reproducibility, and offline operation over production optimizations. Rationale: preserves the repository's primary purpose and prevents accidental production assumptions.

### 2. Offline-First & Cloud Migration
Implementations MUST be fully runnable offline (LocalDB, filesystem). Infrastructure abstractions (interfaces) MUST exist so cloud implementations (Azure SQL, Blob Storage, Entra ID) can be swapped in without changing business logic. Rationale: enables low-friction training while retaining a clear migration path.

### 3. Security-By-Design (Training Context)
Security controls relevant to teaching (IDOR prevention, role-based checks, authorization attributes) MUST be implemented and documented. Any security simplification for training MUST be explicitly documented and labeled as non-production. Rationale: teach secure patterns while clearly marking trade-offs.

### 4. Test-First Quality Gates
(Key requirement) Core features and data models MUST include unit tests and at least one integration-level verification for critical flows (authentication, project membership, task ownership). Test-first practices are STRONGLY RECOMMENDED: tests that exercise acceptance criteria MUST be present before merging significant feature changes. Rationale: ensures examples remain verifiable and reproducible for students.

### 5. Simplicity & Observability
Design choices MUST favor clarity and minimal surface area. Instrumentation (structured logging, clear error messages) SHOULD be present to aid learning and debugging. Performance optimizations are permissible only when they do not reduce code clarity for learners. Rationale: keeps the codebase approachable for students.

## Constraints & Compliance
- Technology baseline: ASP.NET Core 8.0 + Blazor Server + EF Core using LocalDB for training.
- No external cloud services are required for training runs. Any external integration added MUST be optional and disabled by default.
- Licensing: Code is provided for training use only (see LICENSE-CODE). Contributors MUST not claim production readiness for training artifacts.

## Development Workflow
- Branching: feature work MUST use feature branches named `feature/[short-description]` or `[JIRA-123]-short-description`.
- PRs: Every pull request MUST include a description referencing which constitution principles it affects and link to related spec/plan files.
- Reviews: Significant changes (new services, schema changes, authentication/authorization updates) MUST have at least two approvals, including one maintainer.
- Tests & Gates: CI MUST run unit and integration tests; merges to main are blocked if critical tests fail.

## Governance
Amendments: Proposed amendments MUST be submitted as a specification document under `.specify/` and a pull request. A proposed amendment becomes ratified when merged and approved by at least two maintainers or approvers listed in the project metadata.

Versioning: This constitution follows semantic versioning for governance changes:
- MAJOR: Backward-incompatible principle removals or redefinitions.
- MINOR: Addition of new principles or material expansions.
- PATCH: Clarifications, wording fixes, or non-substantive edits.

Compliance reviews: Major or minor amendments MUST include a short migration or compliance checklist that describes how to bring existing plans, specs, and templates into alignment.

**Version**: 0.1.0 | **Ratified**: 2026-03-25 | **Last Amended**: 2026-03-25
