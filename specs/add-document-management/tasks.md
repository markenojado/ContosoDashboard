# Tasks: Document Upload & Management — Phase 2

This file contains Phase 1 completed notes and Phase 2 executable tasks organized per the project template and Task Generation Rules.

**Feature**: Document Upload and Management
**Location**: specs/add-document-management

---

**Phase 1 — Setup (COMPLETED)**
- [ ] T001 Create feature plan and spec (completed) — specs/add-document-management/plan.md
- [ ] T002 Finalize data model (completed) — specs/add-document-management/data-model.md
- [ ] T003 Provide quickstart and environment notes (completed) — specs/add-document-management/quickstart.md
- [ ] T004 Define API contract for files (completed) — specs/add-document-management/contracts/FileService-openapi.md

---

**Phase 2 — Setup**
 - [x] T005 Update `appsettings.Development.json` with `FileStorage:BasePath` entry — ContosoDashboard/appsettings.Development.json
 - [x] T006 Add `Document` and `DocumentShare` entity classes — ContosoDashboard/Models/Document.cs, ContosoDashboard/Models/DocumentShare.cs
 - [x] T007 Add `DbSet<Document>` and `DbSet<DocumentShare>` to `ApplicationDbContext` — ContosoDashboard/Data/ApplicationDbContext.cs
 - [x] T008 Create EF Core migration for document entities and apply to LocalDB — ContosoDashboard/Migrations/ (create via `dotnet ef migrations add AddDocuments` and `dotnet ef database update`)
 - [x] T009 Add seed data for categories and sample documents (development only) — ContosoDashboard/Data/Seed/DocumentSeed.cs
 - [x] T010 Add `IFileStorageService` interface — ContosoDashboard/Services/IFileStorageService.cs
 - [x] T011 Implement `LocalFileStorageService` per storage pattern and config `BasePath` — ContosoDashboard/Services/LocalFileStorageService.cs
 - [x] T012 Add `IVirusScanner` stub and `StubVirusScanner` implementation for training — ContosoDashboard/Services/Scanner/IVirusScanner.cs, ContosoDashboard/Services/Scanner/StubVirusScanner.cs
 - [x] T013 Register services in DI container (`IFileStorageService`, `DocumentService`, `IVirusScanner`) — ContosoDashboard/Program.cs
 - [x] T014 Add configuration model for file storage and bind in startup — ContosoDashboard/Configuration/FileStorageOptions.cs and ContosoDashboard/Program.cs

---

**Phase 2 — Foundational (blocking prerequisites)**
 - [x] T015 [P] Implement `DocumentService` to orchestrate upload/metadata/notifications — ContosoDashboard/Services/DocumentService.cs
 - [x] T016 Implement storage path generator helper (GUID path pattern) — ContosoDashboard/Services/Helpers/FilePathGenerator.cs
 - [x] T017 Implement upload validation helpers (extension whitelist, file size check) — ContosoDashboard/Services/Validation/FileValidation.cs
 - [x] T018 Implement `FilesController` API endpoints per contract: upload, download, delete, get metadata, share — ContosoDashboard/Controllers/FilesController.cs
 - [x] T019 Add authorization checks and policies used by `FilesController` (owner|project member|share|admin) — ContosoDashboard/Authorization/DocumentAuthorizationHandler.cs
 - [x] T020 Create repository-level methods if needed (e.g., DocumentRepository) or use `ApplicationDbContext` directly for persistence — ContosoDashboard/Services/Repositories/DocumentRepository.cs
 - [x] T021 Add logging and audit entries for upload/download/delete/share actions — ContosoDashboard/Services/Logging/DocumentAuditLogger.cs

---

**Phase 2 — User Stories**

**User Story 1 (US1) — Upload a Document (P1)**
 - [x] T022 [US1] Implement upload UI component with multipart upload, metadata form, and progress indicator — ContosoDashboard/Pages/DocumentUpload.razor
 - [x] T023 [US1] Implement server-side upload handler that: validates input, generates file path, saves file via `IFileStorageService`, creates DB record, and returns metadata including `scanStatus` — ContosoDashboard/Controllers/FilesController.cs
 - [x] T024 [US1] Ensure upload flow follows sequence: generate path → save file → insert metadata (implement atomic rollback on disk failure) — ContosoDashboard/Services/DocumentService.cs
 - [x] T025 [US1] Add unit tests for `DocumentService.UploadAsync` covering success, oversized file, unsupported type, disk error cleanup — ContosoDashboard.Tests/DocumentServiceTests.cs
 - [x] T026 [US1] Add integration test for end-to-end upload → metadata verify → download → delete (use test DB and temp filesystem) — ContosoDashboard.IntegrationTests/UploadFlowTests.cs

**User Story 2 (US2) — View & Search Documents (P1)**
 - [x] T027 [US2] Implement `My Documents` and `Project Documents` Blazor pages with sorting and filtering UI — ContosoDashboard/Pages/Documents.razor, ContosoDashboard/Pages/ProjectDocuments.razor
 - [x] T028 [US2] Implement server-side search API and pagination supporting title, description, tags, uploader, project — ContosoDashboard/Controllers/FilesController.cs (search endpoint)
 - [x] T029 [US2] Add `Recent Documents` dashboard widget and wire to user's recent uploads — ContosoDashboard/Shared/Components/RecentDocuments.razor
 - [x] T030 [US2] Add unit tests for search and listing APIs (filtering, authorization) — ContosoDashboard.Tests/FilesControllerSearchTests.cs

**User Story 3 (US3) — Share Document (P2)**
- [ ] T031 [US3] Implement `DocumentShare` persistence, `Share` endpoint, and in-app notification side-effect — ContosoDashboard/Models/DocumentShare.cs, ContosoDashboard/Controllers/FilesController.cs (share action), ContosoDashboard/Services/DocumentService.cs
- [ ] T032 [US3] Implement frontend UI for sharing and `Shared with Me` view — ContosoDashboard/Pages/SharedWithMe.razor, ContosoDashboard/Pages/DocumentShareModal.razor
- [ ] T033 [US3] Add unit tests for share permissions (`view` vs `edit`) and notification emission — ContosoDashboard.Tests/DocumentShareTests.cs

---

**Phase 2 — Optional Production / Background (Parallelizable)**
- [ ] T034 [P] Create Azure Function project for queue-triggered virus scanning and metadata updates — ContosoDashboard.Functions/DocumentScanner/DocumentScannerFunction.cs (optional; production readiness)
- [ ] T035 [P] Add Azure Queue enqueuing on upload (production branch toggle) and message schema per plan — ContosoDashboard/Services/DocumentService.cs, specs/add-document-management/contracts/FileService-openapi.md
- [ ] T036 [P] Implement `AzureBlobStorageService` skeleton for future swap (optional) — ContosoDashboard/Services/AzureBlobStorageService.cs

---

**Phase 2 — Tests, CI, and Deployment**
- [ ] T037 Add unit test project(s) and place tests: ContosoDashboard.Tests (xUnit) and ContosoDashboard.IntegrationTests — /tests/ or ContosoDashboard.Tests/ directory
- [ ] T038 Add CI workflow steps to run `dotnet restore`, `dotnet build`, `dotnet ef database update` (migrations), `dotnet test`, and optional integration tests — .github/workflows/ci.yml or azure-pipelines.yml
- [ ] T039 Add pipeline step to create ephemeral `AppData/uploads` path and set permissions during CI (use temp folder on runners) — .github/workflows/ci.yml or azure-pipelines.yml
- [ ] T040 Add tests to validate scanStatus lifecycle with stubbed scanner in CI for training builds — ContosoDashboard.Tests/ScannerTests.cs

---

**Final Phase — Polish & Cross-Cutting Concerns**
- [ ] T041 Add acceptance test checklist and manual QA steps to `specs/add-document-management/quickstart.md` — specs/add-document-management/quickstart.md
- [ ] T042 Add admin reporting queries and lightweight endpoint for audit reports — ContosoDashboard/Controllers/Admin/DocumentReportsController.cs
- [ ] T043 Add documentation for swapping `IFileStorageService` implementation to Azure Blob (migration guide) — specs/add-document-management/research.md
- [ ] T044 Ensure all new files are documented with XML doc comments and update README with feature instructions — README.md and specs/add-document-management/README.md

---

**Dependencies & Execution Order**
- T005 → T006,T007 → T008 → T009
- T010,T011,T012,T014 → T013 (DI registration after implementations)
- T015→T016,T017 → T018 (service and helper implementations before controller)
- US1 (T022→T023→T024→T025→T026) requires Foundational tasks T015–T018
- US2 (T027→T028→T029→T030) depends on T007 (DB) and T015 (DocumentService)
- US3 (T031→T032→T033) depends on T006 (DocumentShare model), T015 (DocumentService), and T018 (FilesController)
- Optional Azure tasks (T034,T035,T036) are parallelizable and do not block local training implementation (mark `[P]`)

**Parallel Execution Examples**
- While T008 (migrations) is running, developers can implement UI components (`T022`, `T027`) in parallel. (Example: T008 || T022,T027)
- Implement unit tests (T025,T030,T033) in parallel with service and controller implementation (T015,T018). (Example: T015 || T025)
- Azure Function work (T034) can proceed in parallel with local storage implementation (T011). (Example: T011 || T034)

**Implementation Strategy (MVP First)**
- Deliver MVP for training environment: Local storage (`LocalFileStorageService`), stubbed scanner, upload/download, My Documents view, and search. (MVP tasks: T005–T026, T027–T030)
- Next deliver sharing and notifications (T031–T033).
- Optional production features (Azure scanning & blob storage) implemented later and flagged optional (T034–T036).

**Validation & Independent Test Criteria (per User Story)**
- US1 (T022–T026): End-to-end upload test passes: file saved to `AppData/uploads/{userId}/{projectId|personal}/{guid}.{ext}`, DB record created, and download returns same bytes.
- US2 (T027–T030): Search returns only authorized documents and response time < 2s in test harness.
- US3 (T031–T033): Share creates `DocumentShare` record and recipient receives in-app notification.

---

**Notes**
- Azure pieces are optional for training; included for production readiness and marked `[P]`.
- Use exact file paths shown above when implementing to keep PRs small and reviewable.
