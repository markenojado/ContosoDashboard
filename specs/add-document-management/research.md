# Phase 0 — Research Decisions

Decision: Offline uploads are disallowed in training

Rationale: Training environment must remain simple and reproducible. Disallowing offline uploads avoids complex conflict resolution and sync mechanics. UI shows clear message when offline and blocks upload actions.

Alternatives considered:
- Local queue + background sync: More realistic but increases complexity and test surface; rejected for training timeline.
- Allow local-only storage with manual sync: introduces edge cases and test burden; rejected.

Decision: Sharing permission model — `view` and `edit` (two levels)

Rationale: Two-level model covers common collaboration needs, aligns with spec clarification, keeps UI simple.

Alternatives considered:
- Role-based ACLs or expiring links: more flexible but out-of-scope for this release.

Decision: Virus/malware scanner — stubbed scanner in training

Rationale: Ensures training runs without external dependencies. Provide `IVirusScanner` interface with `StubVirusScanner` that flags nothing by default. Document clearly that production must integrate a real scanner.

Alternatives considered:
- Integrate an external scanning service in training: adds external dependency and network variability; rejected.

Notes:
- All decisions respect the Constitution: training-first, offline-capable (but uploads blocked while offline), security-by-design (scanner stub documented), and migration-ready via interfaces.
