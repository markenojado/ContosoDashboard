# Quickstart — Document Upload & Management (training)

Prerequisites

- .NET 8 SDK installed
- LocalDB or dev database usable by the app
- Repository checked out and working copy at feature branch

Setup

1. From repository root, restore and build:

   dotnet restore
   dotnet build ContosoDashboard/ContosoDashboard.csproj

2. Update database (EF Core migrations). If the project uses migrations:

   cd ContosoDashboard
   dotnet ef database update

3. Configure file storage path in `appsettings.Development.json`:

   {
     "FileStorage": {
       "BasePath": "%USERPROFILE%\\AppData\\uploads"
     }
   }

4. Ensure the application process has write permissions to the configured `BasePath`.

Run

   cd ContosoDashboard
   dotnet run

Testing notes

- Unit tests: `dotnet test` at solution root (ensure new tests added under `tests/` as needed).
- Integration: Add at least one integration test that performs upload → metadata verify → download → delete.
- Manual QA: Login as `testuser`, navigate to Documents page, try upload a PDF ≤25MB with required metadata. Verify file is saved to `AppData/uploads/{userId}/{projectId or personal}/{guid}.{ext}` and DB record exists.

Debugging

- Logs: enable structured logging; check for upload errors (disk full, permissions).
- Scanner: training uses `StubVirusScanner` — review `Services/Scanner` implementation.
