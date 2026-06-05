SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
DROP TABLE IF EXISTS core.tblUser;
DROP TABLE IF EXISTS core.tblPrincipalScope;
DROP TABLE IF EXISTS core.conPrincipalType;
CREATE TABLE core.conPrincipalType
(
    TIER_ID             SMALLINT            NOT NULL,
    APP_ID              SMALLINT            NOT NULL    DEFAULT 0,
    PRINCIPAL_TYPE_ID   AS dbo.MvpDummyPrincipalHashFull(TIER_ID, APP_ID),
    CODE                NVARCHAR(64)        NOT NULL,
    DISPLAY_NAME        NVARCHAR(128)       NOT NULL,
    DESCRIPTION         NVARCHAR(512)       NULL,
    CEILING_LEVEL       NVARCHAR(32)        NOT NULL,
    STATUS              TINYINT             NOT NULL    DEFAULT 1,
    DELETED             BIT                 NOT NULL    DEFAULT 0,
    CONSTRAINT PK_conPrincipalType PRIMARY KEY CLUSTERED (TIER_ID, APP_ID)
);
GO
CREATE UNIQUE INDEX IX_conPrincipalType_Code ON core.conPrincipalType (CODE) WHERE DELETED = 0;
CREATE INDEX IX_conPrincipalType_TierId      ON core.conPrincipalType (TIER_ID);
GO