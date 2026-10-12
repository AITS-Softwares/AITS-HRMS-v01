IF OBJECT_ID('dbo.IntegrationRecordMap', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.IntegrationRecordMap
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ModuleName NVARCHAR(100) NOT NULL,
        LocalTableName NVARCHAR(128) NULL,
        LocalRecordId NVARCHAR(100) NULL,
        ErpNextDocType NVARCHAR(128) NOT NULL,
        ErpNextDocumentName NVARCHAR(255) NOT NULL,
        LastSyncDirection NVARCHAR(30) NULL,
        LastSyncStatus NVARCHAR(30) NULL,
        LastSyncedAtUtc DATETIME2 NULL,
        LastKnownErpNextModifiedAtUtc DATETIME2 NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_IntegrationRecordMap_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL
    );

    CREATE UNIQUE INDEX UX_IntegrationRecordMap_ErpNext
        ON dbo.IntegrationRecordMap(ErpNextDocType, ErpNextDocumentName);
END;

IF OBJECT_ID('dbo.SyncOutbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SyncOutbox
    (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CorrelationId NVARCHAR(64) NOT NULL,
        ModuleName NVARCHAR(100) NOT NULL,
        Operation NVARCHAR(20) NOT NULL,
        ErpNextDocType NVARCHAR(128) NOT NULL,
        ErpNextDocumentName NVARCHAR(255) NULL,
        PayloadJson NVARCHAR(MAX) NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_SyncOutbox_Status DEFAULT 'Pending',
        RetryCount INT NOT NULL CONSTRAINT DF_SyncOutbox_RetryCount DEFAULT 0,
        LastError NVARCHAR(MAX) NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_SyncOutbox_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        ProcessedAtUtc DATETIME2 NULL
    );

    CREATE INDEX IX_SyncOutbox_Status_CreatedAtUtc
        ON dbo.SyncOutbox(Status, CreatedAtUtc);
END;

IF OBJECT_ID('dbo.SyncInbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SyncInbox
    (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CorrelationId NVARCHAR(64) NOT NULL,
        SourceSystem NVARCHAR(50) NOT NULL CONSTRAINT DF_SyncInbox_SourceSystem DEFAULT 'ERPNext',
        ModuleName NVARCHAR(100) NOT NULL,
        ErpNextDocType NVARCHAR(128) NOT NULL,
        ErpNextDocumentName NVARCHAR(255) NOT NULL,
        PayloadJson NVARCHAR(MAX) NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_SyncInbox_Status DEFAULT 'Pending',
        RetryCount INT NOT NULL CONSTRAINT DF_SyncInbox_RetryCount DEFAULT 0,
        LastError NVARCHAR(MAX) NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_SyncInbox_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        ProcessedAtUtc DATETIME2 NULL
    );

    CREATE INDEX IX_SyncInbox_Status_CreatedAtUtc
        ON dbo.SyncInbox(Status, CreatedAtUtc);
END;

IF OBJECT_ID('dbo.SyncAuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SyncAuditLog
    (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CorrelationId NVARCHAR(64) NOT NULL,
        ModuleName NVARCHAR(100) NULL,
        Operation NVARCHAR(20) NOT NULL,
        ErpNextDocType NVARCHAR(128) NULL,
        ErpNextDocumentName NVARCHAR(255) NULL,
        Status NVARCHAR(30) NOT NULL,
        Message NVARCHAR(MAX) NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_SyncAuditLog_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_SyncAuditLog_CorrelationId
        ON dbo.SyncAuditLog(CorrelationId);
END;

IF OBJECT_ID('dbo.IntegrationError', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.IntegrationError
    (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CorrelationId NVARCHAR(64) NOT NULL,
        ModuleName NVARCHAR(100) NULL,
        Operation NVARCHAR(20) NULL,
        ErpNextDocType NVARCHAR(128) NULL,
        ErpNextDocumentName NVARCHAR(255) NULL,
        ErrorMessage NVARCHAR(MAX) NOT NULL,
        ErrorJson NVARCHAR(MAX) NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_IntegrationError_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_IntegrationError_CorrelationId
        ON dbo.IntegrationError(CorrelationId);
END;
