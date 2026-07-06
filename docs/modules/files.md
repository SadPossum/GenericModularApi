# Files Module

`Files` is an optional tenant-scoped API front door over shared file storage.

The module profile requires:

- `tenancy.context`
- `file-management.storage`

It provides:

- `files.objects`

Use it when a host wants a centralized upload/download/delete surface for shared files such as avatars, attachments, imports, or exports. Feature modules can also bypass the front door and use `Shared.FileManagement` directly when they own their own file lifecycle.

## Endpoints

```text
POST   /api/files
GET    /api/files/{fileId}
DELETE /api/files/{fileId}
```

All endpoints require authorization. When tenancy is enabled, the standard tenant header is required and the storage key is partitioned by a tenant hash.

## Composition Example

```csharp
builder.AddLocalFileStorage();
builder.AddModule<FilesModule>();
builder.ValidateModuleComposition();
```

For MinIO, use `builder.AddMinioFileStorage()` instead, or register both adapters and let `FileManagement:Provider` select one.
