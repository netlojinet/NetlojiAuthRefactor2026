SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
DROP TABLE IF EXISTS core.tblUserScopePrincipalMatrix;
CREATE TABLE core.tblUserScopePrincipalMatrix
(
    USER_ID             INT                 NOT NULL,
    SCOPE_ID            INT                 NOT NULL,
    PRINCIPAL_TYPE_ID   INT                 NOT NULL,
    STATUS              TINYINT             NOT NULL    DEFAULT 1,
    DELETED             BIT                 NOT NULL    DEFAULT 0,
    CREATOR_USER_ID     INT                 NOT NULL,
    CREATOR_SCOPE_ID    INT                 NULL,
    CREATION_TIME       DATETIME2           NOT NULL    DEFAULT SYSUTCDATETIME(),
    EDITOR_USER_ID      INT                 NOT NULL,
    EDITOR_SCOPE_ID     INT                 NULL,
    MODIFIED_TIME       DATETIME2           NOT NULL    DEFAULT SYSUTCDATETIME(),
    DELETER_USER_ID     INT                 NULL,
    DELETER_SCOPE_ID    INT                 NULL,
    DELETION_TIME       DATETIME2           NULL,
    CONSTRAINT PK_tblUserScopePrincipalMatrix PRIMARY KEY CLUSTERED (USER_ID, SCOPE_ID)
);
GO
CREATE INDEX IX_USPM_ScopeId ON core.tblUserScopePrincipalMatrix (SCOPE_ID) WHERE DELETED = 0;
GO