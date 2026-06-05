SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
DROP TABLE IF EXISTS core.conScopeType;
CREATE TABLE core.conScopeType
(
    SCOPE_TYPE      TINYINT             NOT NULL,
    CODE            NVARCHAR(32)        NOT NULL,
    DISPLAY_NAME    NVARCHAR(64)        NOT NULL,
    DESCRIPTION     NVARCHAR(256)       NULL,
    STATUS          TINYINT             NOT NULL    DEFAULT 1,
    DELETED         BIT                 NOT NULL    DEFAULT 0,
    CONSTRAINT PK_conScopeType PRIMARY KEY CLUSTERED (SCOPE_TYPE)
);
GO
MERGE core.conScopeType AS target
USING (VALUES
    (255, N'root_scope',     N'Root Scope',      N'Sistem kok scope - tum agacin en ustunu temsil eder', N'unlimited'),
    (0,   N'individual',     N'Individual',      N'Bireysel kullanici scope''u',                         N'scope_bounded'),
    (1,   N'property',       N'Property',        N'Mulk/tesis scope''u',                                 N'scope_bounded'),
    (2,   N'organization',   N'Organization',    N'Organizasyon scope''u (kok)',                         N'scope_bounded')
) AS source (SCOPE_TYPE, CODE, DISPLAY_NAME, DESCRIPTION, CEILING_LEVEL)
ON target.SCOPE_TYPE = source.SCOPE_TYPE
WHEN MATCHED THEN
    UPDATE SET CODE = source.CODE, DISPLAY_NAME = source.DISPLAY_NAME, DESCRIPTION = source.DESCRIPTION
WHEN NOT MATCHED BY TARGET THEN
    INSERT (SCOPE_TYPE, CODE, DISPLAY_NAME, DESCRIPTION, STATUS, DELETED)
    VALUES (source.SCOPE_TYPE, source.CODE, source.DISPLAY_NAME, source.DESCRIPTION, 1, 0);
GO

SET QUOTED_IDENTIFIER ON;
SELECT SCOPE_TYPE, CODE, DISPLAY_NAME FROM core.conScopeType ORDER BY SCOPE_TYPE;
GO