SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
DROP TABLE IF EXISTS dbo.tblDemoData;
CREATE TABLE dbo.tblDemoData
(
    DEMODATA_ID         INT IDENTITY(1,1)   NOT NULL,
    PLACE_HOLDER        NVARCHAR(50)        NULL,
    LCID                INT                 NOT NULL    DEFAULT 1033,
    DEFAULT_LCID        INT                 NOT NULL    DEFAULT 1033,
    STATUS              TINYINT             NOT NULL    DEFAULT 1,
    DELETED             BIT                 NOT NULL    DEFAULT 0,
    IS_PUBLIC           BIT                 NOT NULL    DEFAULT 0,   -- system_public/scope_public yalniz IS_PUBLIC=1 okur
    OWNER_SCOPE_ID     INT                 NULL,
    CREATOR_USER_ID     INT                 NOT NULL,
    CREATOR_SCOPE_ID    INT                 NULL,
    CREATION_TIME       DATETIME2           NOT NULL    DEFAULT SYSUTCDATETIME(),
    EDITOR_USER_ID      INT                 NOT NULL,
    EDITOR_SCOPE_ID     INT                 NULL,
    MODIFIED_TIME       DATETIME2           NOT NULL    DEFAULT SYSUTCDATETIME(),
    DELETER_USER_ID     INT                 NULL,
    DELETER_SCOPE_ID    INT                 NULL,
    DELETION_TIME       DATETIME2           NULL,
    CONSTRAINT PK_tblDemoData PRIMARY KEY CLUSTERED (DEMODATA_ID)
);
GO
CREATE INDEX IX_tblDemoData_Status    ON dbo.tblDemoData (STATUS)    WHERE DELETED = 0;
CREATE INDEX IX_tblDemoData_OwnerScope  ON dbo.tblDemoData (OWNER_SCOPE_ID) WHERE DELETED = 0 AND OWNER_SCOPE_ID IS NOT NULL;
GO