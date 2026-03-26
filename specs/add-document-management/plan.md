# Implementation Plan: Document Upload and Management

**Branch**: `add-document-management` | **Date**: 2026-03-26 | **Spec**: specs/add-document-management/spec.md
**Input**: Feature specification from `specs/add-document-management/spec.md`

## Summary

Add document upload and management to the ContosoDashboard Blazor Server app using a local filesystem storage implementation (`LocalFileStorageService`) behind `IFileStorageService`. Feature supports upload, preview, download, metadata editing, delete, share (view/edit), search, and project integration. Training-first: local storage, stubbed virus scanner, offline uploads disallowed.

## Technical Context

**Language/Version**: C# / .NET 8 (ASP.NET Core 8, Blazor Server)
**Primary Dependencies**: ASP.NET Core, Entity Framework Core (LocalDB), xUnit, bUnit (UI tests), AutoMapper (optional)
**Storage**: LocalDB (EF Core) for metadata; local filesystem (`AppData/uploads`) outside `wwwroot` for files
**Testing**: `dotnet test` (xUnit) for unit/integration; minimal integration tests for upload/download flows; acceptance UI tests with bUnit where applicable
**Target Platform**: Windows (training machines), runnable offline
**Project Type**: Web application (Blazor Server) — single project: `ContosoDashboard/`
**Performance Goals**: Upload ≤25MB in <30s typical; list/search pages <2s for up to 500 docs
**Constraints**: Offline-capable (no cloud required), integer `DocumentId`, `Category` as text, `FileType` up to 255 chars
**Scale/Scope**: Training environment scale (hundreds of users/test data), not enterprise scale

## Constitution Check

- Training-First: SATISFIED — feature uses local storage, training stub for scanner, documentation marks non-production parts.
- Offline-First & Cloud Migration: SATISFIED — runs offline; `IFileStorageService` abstraction provided to enable Azure swap later.
- Security-By-Design: SATISFIED — authorization required for all endpoints; files served through controller endpoints; GUID filenames and whitelist validation.
- Test-First Quality Gates: SATISFIED (RECOMMENDATION) — plan requires unit tests and at least one integration test for upload/download; implement tests before merge.
- Simplicity & Observability: SATISFIED — simple data model, explicit sequence (generate path → save file → insert metadata), structured logs recommended.

No constitution gates are violated. Proceed to Phase 0.

## Project Structure

Documentation (feature)
```
specs/add-document-management/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── FileService-openapi.md
```

Source code (changes apply inside existing project)
```
ContosoDashboard/
├── Data/ApplicationDbContext.cs         # add DbSet<Document>, DbSet<DocumentShare>
├── Models/Document.cs                   # new entity
├── Models/DocumentShare.cs              # new entity
├── Services/DocumentService.cs          # orchestrates upload workflow
├── Services/IFileStorageService.cs      # interface
├── Services/LocalFileStorageService.cs   # local implementation
├── Services/Scanner/IVirusScanner.cs    # stub for training
├── Pages/Documents.razor                 # UI pages (My Documents, Project Documents)
├── Pages/DocumentUpload.razor           # upload modal/component
└── Controllers/FilesController.cs       # endpoints to serve files with auth
```

**Structure Decision**: Use the existing single Blazor Server project `ContosoDashboard/` and add models, services, controller and pages in their existing folders. This keeps the codebase simple and aligned with training goals.

## Complexity Tracking

No constitution violations detected; no additional complexity tracking required.

## Summary / Next Steps

- Phase 0: Research decisions documented in `research.md`.
- Phase 1: Data model in `data-model.md`, Quickstart in `quickstart.md`, API contracts in `contracts/FileService-openapi.md`.
- Phase 2: Generate tasks and tests via `/speckit.tasks` after plan review.

## Background Jobs: Async Virus Scanning (Production Design)

Design a background job pipeline for asynchronous virus/malware scanning using Azure Functions + Azure Queue Storage. Purpose: offload potentially long-running scanning work from request/response path, enable retries, and isolate scanner credentials.

Architecture summary:
- Upload flow (synchronous): API receives file → validate/whitelist → generate GUID `FilePath` → save file to local/prod storage → insert metadata record with `ScanStatus = "Pending"` (enum: Pending, Queued, Scanning, Clean, Infected, Error) → enqueue a scan message to Azure Queue Storage → return 201 Created with metadata including `scanStatus: "Queued"`.
- Background worker: Azure Function with Queue Trigger listens to scan queue → function downloads file (from blob storage or secured file share) → calls the configured scanner engine (third-party service or scanner image) → updates metadata record with final `ScanStatus` and `ScanReport` (summary), and emits audit log + notification if infected.

Message schema (Queue message JSON):
{
    "documentId": int,
    "filePath": string,
    "uploadedBy": int,
    "contentType": string,
    "queuedAt": "2026-03-26T...Z"
}

Operational details:
- Retries: Azure Functions built-in retry on transient failures; implement idempotent scanning and update logic. Use message visibility/backoff and a dead-letter queue for repeated failures.
- Atomicity: metadata created before enqueueing ensures worker can always find DB record. If enqueue fails, mark metadata `ScanStatus = "Error"` and surface to admin UI.
- Security: Function uses a managed identity to access storage and DB; queue access controlled via SAS or RBAC; do not embed secrets in messages.
- Monitoring: emit Application Insights traces and metrics (scan durations, failure rates, infected counts); create alerts for repeated failures or infected-file spikes.
- Cost/Prod notes: In training, keep stubbed scanner (no Function). For production, use Azure Blob Storage for files and Azure Functions for scanning; LocalFileStorageService remains for offline training.

Developer notes / API contract impact:
- `Upload` endpoint must enqueue a scan message and return `scanStatus` in the response metadata.
- Add `ScanStatus` and optionally `ScanReport` fields to the `Document` metadata schema and to `GetMetadata` responses.

