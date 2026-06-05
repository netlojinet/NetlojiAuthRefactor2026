SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
DROP TABLE IF EXISTS core.tblUser;
CREATE TABLE core.tblUser
(
    USER_ID             INT IDENTITY(1,1)   NOT NULL,
    USERNAME            NVARCHAR(128)       NOT NULL,
    EMAIL               NVARCHAR(256)       NULL,
    DISPLAY_NAME        NVARCHAR(256)       NULL,
    GSM_NO              BIGINT              NOT NULL,
    PRINCIPAL_TYPE_ID   INT                 NOT NULL    DEFAULT 0,  -- 0 = denied (fail-closed). NOT TIER_ID; gecerli PRINCIPAL_TYPE_ID degilse katalogla eslesmez.
    USER_KEY            BIGINT              NOT NULL    DEFAULT 0,
    STATUS              TINYINT             NOT NULL    DEFAULT 1,
    DELETED             BIT                 NOT NULL    DEFAULT 0,
    CREATOR_USER_ID         INT         NOT NULL,
    CREATOR_SCOPE_ID        INT         NULL,
    CREATION_TIME           DATETIME2   NOT NULL    DEFAULT SYSUTCDATETIME(),
    EDITOR_USER_ID          INT         NOT NULL,
    EDITOR_SCOPE_ID         INT         NULL,
    MODIFIED_TIME           DATETIME2   NOT NULL    DEFAULT SYSUTCDATETIME(),
    DELETER_USER_ID         INT         NULL,
    DELETER_SCOPE_ID        INT         NULL,
    DELETION_TIME           DATETIME2   NULL,
    CONSTRAINT PK_tblUser PRIMARY KEY CLUSTERED (USER_ID)
);
GO
CREATE UNIQUE INDEX IX_tblUser_Username  ON core.tblUser (USERNAME) WHERE DELETED = 0;
CREATE UNIQUE INDEX IX_tblUser_Email     ON core.tblUser (EMAIL)    WHERE DELETED = 0 AND EMAIL IS NOT NULL;
CREATE UNIQUE INDEX IX_tblUser_UserKey   ON core.tblUser (USER_KEY) WHERE DELETED = 0;
CREATE INDEX IX_tblUser_PrincipalType    ON core.tblUser (PRINCIPAL_TYPE_ID) WHERE DELETED = 0;
CREATE INDEX IX_tblUser_GsmNo            ON core.tblUser (GSM_NO)   WHERE DELETED = 0;
GO
CREATE OR ALTER TRIGGER core.trg_tblUser_UserKey
ON core.tblUser
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t
    SET USER_KEY = dbo.MvpDummyPrincipalUserHash(
            CAST(t.PRINCIPAL_TYPE_ID AS BIGINT),
            t.GSM_NO)
    FROM core.tblUser t
    INNER JOIN inserted i ON t.USER_ID = i.USER_ID;
END;
GO