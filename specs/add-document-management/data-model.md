# Data Model

Entities

- Document
  - DocumentId: int (PK)
  - Title: string (required, max 250)
  - Description: string (optional, max 2000)
  - Category: string (required, max 100)  # store as text per constraints
  - ProjectId: int? (FK -> Project.ProjectId, nullable)
  - Tags: string (store as delimited text or separate tag table) (optional)
  - UploadedBy: int (FK -> User.UserId)
  - UploadedAt: datetime
  - FileSize: long (bytes, <= 25 * 1024 * 1024 validation)
  - FileType: string (required, max 255)
  - FilePath: string (required, max 1024)  # GUID-based path outside wwwroot

- DocumentShare
  - ShareId: int (PK)
  - DocumentId: int (FK -> Document.DocumentId)
  - SharedWithUserId: int (FK -> User.UserId)
  - SharedByUserId: int (FK -> User.UserId)
  - Permission: string ("view"|"edit")
  - SharedAt: datetime

Relationships

- Document (1) — (0..*) DocumentShare
- Document (many) — (0..1) Project via ProjectId

Validation rules

- Title: required, non-empty, trim, max 250 chars
- Category: required, must be one of configured categories (Project Documents, Team Resources, Personal Files, Reports, Presentations, Other)
- FileSize: <= 25MB (25 * 1024 * 1024 bytes)
- FileType: required, validate against whitelist for upload and stored up to 255 chars
- FilePath: generated server-side (GUID-based) and validated for path traversal safety
- UploadedBy/SharedWithUserId: must reference existing users

Indexes

- Document: index on UploadedBy, UploadedAt, Title (for sorting), Category
- Search index (simple): Title, Description, Tags, UploadedBy

Storage/migration notes

- Use EF Core migrations; add `DbSet<Document>` and `DbSet<DocumentShare>` to `ApplicationDbContext`.
- DocumentId must be integer per spec.
