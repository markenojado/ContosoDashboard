# Feature Specification: Document Upload and Management

**Feature Branch**: `add-document-management`
**Created**: 2026-03-25
**Status**: Draft
**Input**: User description: "Full stakeholder document attached below (original filename: StakeholderDocs/document-upload-and-management-feature.md)"

---

## Stakeholder Document (source)

(Full contents of StakeholderDocs/document-upload-and-management-feature.md)

# Document Upload and Management Feature - Requirements

## Overview

Contoso Corporation needs to add document upload and management capabilities to the ContosoDashboard application. This feature will enable employees to upload work-related documents, organize them by category and project, and share them with team members.

## Business Need

Currently, Contoso employees store work documents in various locations (local drives, email attachments, shared drives), leading to:

- Difficulty locating important documents when needed
- Security risks from uncontrolled document sharing
- Lack of visibility into which documents are associated with specific projects or tasks

The document upload and management feature addresses these issues by providing a centralized, secure location for work-related documents within the dashboard application that employees already use daily.

## Target Users

All Contoso employees who use the ContosoDashboard application will have access to document management features, with permissions based on their existing roles:

- **Employees**: Upload personal documents and documents for projects they're assigned to
- **Team Leads**: Upload documents and view/manage documents uploaded by their team members
- **Project Managers**: Upload documents and manage all documents associated with their projects
- **Administrators**: Full access to all documents for audit and compliance purposes

## Core Requirements

### 1. Document Upload

**File Selection and Upload**

- Users must be able to select one or more files from their computer to upload
- Supported file types: PDF, Microsoft Office documents (Word, Excel, PowerPoint), text files, and images (JPEG, PNG)
- Maximum file size: 25 MB per file
- Users should see a progress indicator during upload
- System should display success or error messages after upload completes

**Document Metadata**

- When uploading, users must provide:
	- Document title (required)
	- Description (optional)
	- Category selection from predefined list (required): Project Documents, Team Resources, Personal Files, Reports, Presentations, Other
	- Associated project (optional - if the document relates to a specific project)
	- Tags for easier searching (optional - users can add custom tags)
- System should automatically capture:
	- Upload date and time
	- Uploaded by (user name)
	- File size
	- File type (MIME type, e.g., "application/pdf" - field must accommodate 255 characters for Office documents)

**Validation and Security**

- System must scan uploaded files for viruses and malware before storage
- System must reject files that exceed size limits with clear error messages
- System must reject unsupported file types
- Uploaded files must be stored securely with appropriate access controls

**Implementation Notes for Local File Storage**

**Offline Storage Pattern:**
- Store files in a dedicated directory outside `wwwroot` for security (e.g., `AppData/uploads`)
- Generate unique file paths BEFORE database insertion to prevent duplicate key violations
- Recommended pattern: `{userId}/{projectId or "personal"}/{uniqueId}.{extension}` where uniqueId is a GUID
- **Upload sequence: Generate unique path → Save file to disk → Save metadata to database**
- **This prevents orphaned database records if file save fails**
- **This prevents duplicate key errors from empty or non-unique file paths**

**Security Considerations:**
- Files stored outside `wwwroot` require controller endpoints to serve them (enables authorization checks)
- Validate file extensions against whitelist before saving
- Use GUID-based filenames to prevent path traversal attacks
- Never use user-supplied filenames directly in file paths
- Implement authorization checks in download endpoint to prevent unauthorized access

**Azure Migration Design:**
- Create `IFileStorageService` interface with methods: `UploadAsync()`, `DeleteAsync()`, `DownloadAsync()`, `GetUrlAsync()`
- Local implementation (`LocalFileStorageService`) uses `System.IO.File` operations
- Future `AzureBlobStorageService` implementation will use Azure.Storage.Blobs SDK
- Same path pattern works for Azure blob names: `{userId}/{projectId}/{guid}.{ext}`
- Swap implementations via dependency injection configuration
- No changes to business logic, UI, or database schema required for migration

### 2. Document Organization and Browsing

**My Documents View**

- Users must be able to view a list of all documents they have uploaded
- The view should display: document title, category, upload date, file size, associated project
- Users should be able to sort documents by: title, upload date, category, file size
- Users should be able to filter documents by: category, associated project, date range

**Project Documents View**

- When viewing a specific project, users should see all documents associated with that project
- All project team members should be able to view and download project documents
- Project Managers should be able to upload documents to their projects

**Search**

- Users should be able to search for documents by: title, description, tags, uploader name, associated project
- Search should return results within 2 seconds
- Users should only see documents they have permission to access in search results

### 3. Document Access and Management

**Download and Preview**

- Users must be able to download any document they have access to
- For common file types (PDF, images), users should be able to preview documents in the browser without downloading

**Edit Metadata**

- Users who uploaded a document should be able to edit the document metadata (title, description, category, tags)
- Users should be able to replace a document file with an updated version

**Delete Documents**

- Users should be able to delete documents they uploaded
- Project Managers can delete any document in their projects
- Deleted documents should be permanently removed after user confirmation

**Share Documents**

- Document owners should be able to share documents with specific users or teams
- Users who receive shared documents should be notified via in-app notification
- Shared documents should appear in recipients' "Shared with Me" section

### 4. Integration with Existing Features

**Task Integration**

- When viewing a task, users should be able to see and attach related documents
- Users should be able to upload a document directly from a task detail page
- Documents attached to tasks should automatically be associated with the task's project

**Dashboard Integration**

- Add a "Recent Documents" widget to the dashboard home page showing the last 5 documents uploaded by the user
- Add document count to the dashboard summary cards

**Notifications**

- Users should receive notifications when someone shares a document with them
- Users should receive notifications when a new document is added to one of their projects

### 5. Performance Requirements

- Document upload should complete within 30 seconds for files up to 25 MB (on typical network)
- Document list pages should load within 2 seconds for up to 500 documents
- Document search should return results within 2 seconds
- Document preview should load within 3 seconds

### 6. Reporting and Audit

**Activity Tracking**

- System should log all document-related activities: uploads, downloads, deletions, share actions
- Administrators should be able to generate reports showing:
	- Most uploaded document types
	- Most active uploaders
	- Document access patterns

## User Experience Goals

- **Simplicity**: Uploading a document should require no more than 3 clicks
- **Speed**: Common operations (upload, download, search) should feel instant
- **Clarity**: Users should always know what happens to uploaded files
- **Confidence**: Users should trust that their documents are secure and won't be lost

## Success Metrics

The feature will be considered successful if, within 3 months of launch:

- 70% of active dashboard users have uploaded at least one document
- Average time to locate a document is reduced to under 30 seconds
- 90% of uploaded documents are properly categorized
- Zero security incidents related to document access

## Technical Constraints

- Must work **offline without cloud services** for training purposes
- Must use **local filesystem storage** for uploaded documents
- Must implement **interface abstractions** (`IFileStorageService`) for future cloud migration
- Must work within current application architecture (no major rewrites)
- Must comply with existing mock authentication system
- Development timeline: Feature should be production-ready within 8-10 weeks
- **Database: DocumentId must be integer (not GUID) for consistency with existing User/Project keys**
- **Database: Category must store text values (not integer enum) for simplicity**

## Implementation Approach

The document management feature is built using a **layered architecture** that separates concerns and enables future cloud migration:

**Data Layer:**
- Document entity stores metadata (title, category, filename, file path, upload date, uploader)
- DocumentId uses integer keys (consistent with existing User and Project tables)
- Category stores text values ("Project Documents", "Personal Files", etc.) for simplicity
- FileType field accommodates long MIME types (255 characters for Office documents)
- FilePath accommodates GUID-based filenames for security (prevents path traversal attacks)
- DocumentShare entity tracks sharing relationships between users

**Storage Layer:**
- Files stored outside web-accessible directories (security requirement)
- IFileStorageService interface abstracts storage implementation
- LocalFileStorageService for training (uses local filesystem)
- Future: Swap to AzureBlobStorageService for production (no code changes needed)
- File organization: `{userId}/{projectId or "personal"}/{guid}.{extension}`

**Business Logic Layer:**
- DocumentService orchestrates upload workflow:
	1. Validate file (size limit, extension whitelist)
	2. Authorize user (project membership if uploading to project)
	3. Generate unique GUID-based filename
	4. Save file to disk
	5. Create database record with file path
	6. Send notifications to project members
- Authorization checks prevent unauthorized document access (IDOR protection)
- Service layer enforces all security rules before data access

**Presentation Layer:**
- Blazor Server page for document upload and viewing
- File upload uses MemoryStream pattern (prevents disposal issues in Blazor)
- Responsive table displays user's documents with metadata
- Upload modal validates input before submission

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Upload a Document (Priority: P1)

An Employee uploads a document for a project.

**Why this priority**: Enables immediate value — centralizing documents used daily.

**Independent Test**: From the Project Details page, upload a PDF ≤25MB and verify it appears in Project Documents and metadata is stored.

**Acceptance Scenarios**:
1. Given an authenticated user assigned to a project, When they upload a PDF with title and category, Then upload completes, file stored on local disk, and metadata recorded with integer DocumentId.
2. Given a file >25MB, When user attempts upload, Then system rejects with clear error message.

---

### User Story 2 - View & Search Documents (Priority: P1)

User searches for a document by tag or title and finds only permitted results.

**Why this priority**: Users must find documents to realize value.

**Independent Test**: Use search box to query by title; verify results return within 2 seconds and only accessible documents appear.

**Acceptance Scenarios**:
1. Given multiple documents, When user searches by tag, Then matching documents visible and downloadable.

---

### User Story 3 - Share Document (Priority: P2)

Document owner shares a file with a teammate.

**Why this priority**: Collaboration; lower priority than core upload/search.

**Independent Test**: Owner shares a document with a specific user; recipient receives in-app notification and sees it in "Shared with Me".

**Acceptance Scenarios**:
1. Given owner shares with teammate, When share is created, Then recipient notified and can access file.

---

### Edge Cases

- Upload interrupted by network failure while writing to disk: system must clean up partial files and not create metadata record.
- Disk full: return clear error and do not create DB metadata.
- Attempt to download a file without permission: return 403/Unauthorized behavior.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow authenticated users to upload one or more files from their computer.
- **FR-002**: System MUST validate file type against a whitelist (PDF, DOCX, XLSX, PPTX, TXT, JPG, PNG) and reject unsupported types.
- **FR-003**: System MUST enforce a per-file size limit of 25 MB and reject larger files with a clear error.
- **FR-004**: System MUST capture and persist metadata for each upload: integer `DocumentId`, `Title`, `Description`, `Category` (text), `ProjectId` (nullable), `Tags`, `UploadedBy`, `UploadedAt`, `FileSize`, `FileType` (up to 255 chars), and `FilePath`.
- **FR-005**: System MUST store files outside `wwwroot` on local filesystem for training (e.g., `AppData/uploads/{userId}/{projectId or 'personal'}/{guid}.{ext}`).
- **FR-006**: System MUST generate and persist unique file paths before inserting database records and follow sequence: generate path → save file → insert metadata.
- **FR-007**: System MUST scan uploaded files for viruses/malware before making them available for download.
- **FR-007**: System MUST scan uploaded files for viruses/malware before making them available for download. **Training note:** use a stubbed/scoped scanner implementation in the local training build; production deployments MUST integrate a real virus/malware scanning service or engine.
- **FR-008**: System MUST provide views: `My Documents`, `Project Documents`, `Shared with Me`, and a `Recent Documents` dashboard widget.
- **FR-009**: System MUST provide search by title, description, tags, uploader name, and project, returning results within 2 seconds for expected dataset sizes.
- **FR-010**: System MUST allow document owners and authorized roles to download and preview files in-browser for supported types.
- **FR-011**: System MUST allow document owners to edit metadata and replace the underlying file (keeping DocumentId unchanged).
- **FR-012**: System MUST allow document owners and authorized roles to delete documents; deletion removes both file and metadata after confirmation.
- **FR-013**: System MUST support sharing documents with specific users or teams; shared recipients must receive in-app notifications and see documents in "Shared with Me".
- **FR-014**: System MUST log uploads, downloads, deletions, shares and expose admin reporting for these events.
- **FR-015**: System MUST integrate with existing mock authentication; authorization checks must be applied to all endpoints.
- **FR-016**: System MUST use integer `DocumentId` primary key (not GUID) as required.

- **FR-017**: System MUST store `Category` as text values (not integer enum).

- **FR-018**: System MUST implement `IFileStorageService` with `LocalFileStorageService` for training and follow the defined interface to enable future Azure swap.

- **FR-019**: System MUST ensure fileType field supports up to 255 characters for long MIME strings.

- **FR-020**: System MUST ensure uploaded files are not directly accessible and must be served via authorized endpoints.

- **FR-021**: System MUST provide progress feedback during upload and success/error messaging on completion.

- **FR-022**: System MUST handle offline operation for training: allow uploads when offline by queuing or by local-only storage pattern. [NEEDS CLARIFICATION: offline sync/conflict resolution strategy]
 - **FR-022**: System MUST handle offline operation for training: uploads are NOT allowed while offline. If the user is offline the UI MUST show a clear message and require reconnect before uploading. (No local queuing or background sync in training implementation.)

 - **FR-023**: System MUST define sharing permission granularity and enforce it consistently. Implement two permission levels for shares: `view` and `edit`. `view` allows download/preview only; `edit` allows replacing the file and editing metadata. (Expiring links and finer-grained permissions are out of scope for this release.)

### Key Entities

- **Document**: { DocumentId:int, Title:string, Description:string, Category:string, ProjectId:int?, Tags:list<string>, UploadedBy:userId, UploadedAt:datetime, FileSize:int, FileType:string(255), FilePath:string }
- **DocumentShare**: { ShareId:int, DocumentId:int, SharedWithUserId:int, SharedByUserId:int, Permission:string (e.g., "view"|"edit"), SharedAt:datetime }
- **Project**: existing Project entity (relationship via ProjectId)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% of uploads ≤25MB complete within 30 seconds in typical training environment.
- **SC-002**: Document list pages load within 2 seconds for datasets up to 500 documents.
- **SC-003**: Search returns relevant results within 2 seconds for expected dataset sizes.
- **SC-004**: 90% of users successfully complete a primary upload flow in an unmoderated usability test.
- **SC-005**: No security incidents related to document access in the first 3 months after launch (measured by audit logs).

## Assumptions

- Training environment will run with local disk storage available and sufficient permissions to write `AppData/uploads`.
- Virus/malware scanning is available in training (or a stubbed scanner exists if not available).
- Sharing will be implemented using existing user and team constructs from the application.
- Offline usage is limited to queued uploads or local-only storage; sync to shared project repositories requires connection.
- Default category list is as specified by stakeholder; admins can modify categories if needed.

## Out of Scope

- Real-time collaborative editing, version history, external integrations (SharePoint/OneDrive), mobile app support, storage quotas, soft delete recovery.

## Appendix: Implementation Notes

- Follow the upload sequence: generate unique path → save file to disk → save metadata to DB.
- Store files outside `wwwroot` and serve via authorized endpoints.
- Use integer keys for `DocumentId` and text for `Category` as required in constraints.

---

## Spec Status

- Draft created and validated against the spec quality checklist (see checklist file).

---

## Clarifications

### Session 2026-03-25

- Q1: `FR-022` → A: Disallow uploads while offline — users shown clear message and must reconnect before uploading. Rationale: keeps training implementation simple, avoids sync/conflict complexity, and reduces test burden.

- Q2: `FR-023` → A: Two-level sharing permissions (`view`, `edit`) — simple, common, and easy to teach/implement. Rationale: covers common collaboration needs without extra UI complexity.

- Q3: `FR-007` → A: Use a stubbed scanner in training; integrate a real scanner in production. Rationale: reduces training/test friction while ensuring production security.


