SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
DROP TABLE IF EXISTS core.tblScope;
CREATE TABLE core.tblScope
(
    SCOPE_ID        INT IDENTITY(1,1)   NOT NULL,
    SCOPE_TYPE      TINYINT             NOT NULL,
    STATUS          TINYINT             NOT NULL    DEFAULT 1,
    DELETED         BIT                 NOT NULL    DEFAULT 0,
    OWNER_USER_ID   INT                 NOT NULL    DEFAULT -1,   -- F2: scope'un KANONIK sahibi (individual->user, org/property->kurucu). Erisim degil, sahiplik ekseni.
    CREATOR_USER_ID         INT         NOT NULL,
    CREATOR_SCOPE_ID        INT         NULL,
    CREATION_TIME           DATETIME2   NOT NULL    DEFAULT SYSUTCDATETIME(),
    EDITOR_USER_ID          INT         NOT NULL,
    EDITOR_SCOPE_ID         INT         NULL,
    MODIFIED_TIME           DATETIME2   NOT NULL    DEFAULT SYSUTCDATETIME(),
    DELETER_USER_ID         INT         NULL,
    DELETER_SCOPE_ID        INT         NULL,
    DELETION_TIME           DATETIME2   NULL,
    CONSTRAINT PK_tblScope PRIMARY KEY CLUSTERED (SCOPE_ID)
);
GO
CREATE INDEX IX_tblScope_ScopeType ON core.tblScope (SCOPE_TYPE) WHERE DELETED = 0;
CREATE INDEX IX_tblScope_Status    ON core.tblScope (STATUS)    WHERE DELETED = 0;
CREATE INDEX IX_tblScope_OwnerUser ON core.tblScope (OWNER_USER_ID) WHERE DELETED = 0;
GO