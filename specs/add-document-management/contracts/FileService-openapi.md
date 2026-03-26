# FileService API (OpenAPI-style brief)

Base: `/api/files`

1) Upload
- Method: POST
- Path: `/api/files/upload`
- Auth: required (authenticated)
- Request: multipart/form-data; fields: `file` (binary), `title` (string), `description` (string, optional), `category` (string), `projectId` (int?, optional), `tags` (array/string)
- Response: 201 Created
  - Body: `{ documentId:int, title:string, fileType:string, fileSize:int, uploadedAt:datetime }`
 - Response: 201 Created
  - Body: `{ documentId:int, title:string, fileType:string, fileSize:int, uploadedAt:datetime, scanStatus:string (Pending|Queued|Scanning|Clean|Infected|Error) }`
- Errors: 400 Bad Request (validation), 413 Payload Too Large, 415 Unsupported Media Type, 500 Server Error

Notes: server generates unique `FilePath` before DB insert; upload sequence enforced: generate path → save file → insert metadata.

2) Download
- Method: GET
- Path: `/api/files/{documentId}/download`
- Auth: required; authorization check: owner | project member | share with 'view'/'edit' | admin
- Response: 200 OK (streamed file) with `Content-Type` from stored `FileType` and `Content-Disposition` attachment; 403/404 as applicable

3) Delete
- Method: DELETE
- Path: `/api/files/{documentId}`
- Auth: required; authorization: owner or project manager or admin
- Response: 200 OK (or 204 No Content) on success; 403/404 as applicable

4) GetMetadata
- Method: GET
- Path: `/api/files/{documentId}`
- Auth: required; authorization: owner | project member | share | admin
- Response: 200 OK
  - Body: `{ documentId:int, title:string, description:string, category:string, projectId:int?, tags:[], uploadedBy:int, uploadedAt:datetime, fileSize:int, fileType:string, shared:boolean, scanStatus:string, scanReport?:string }`

5) Share
- Method: POST
- Path: `/api/files/{documentId}/share`
- Auth: required; authorization: owner or project manager
- Request: `{ sharedWithUserId:int, permission:string ("view"|"edit") }`
- Response: 200 OK
- Side-effects: create DocumentShare record, send in-app notification to recipient

Notes: On successful upload the endpoint must enqueue a scan message (production) and return the initial `scanStatus` (Queued or Pending). The `scanStatus` field is updated asynchronously by the background scanner.

Security Notes

- All endpoints must validate inputs, enforce role/ownership checks, and scan files (stubbed in training). Files are not served directly from disk; controller enforces auth.
