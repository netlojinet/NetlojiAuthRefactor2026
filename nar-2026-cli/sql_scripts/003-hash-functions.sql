SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
DROP TABLE IF EXISTS core.tblUser;
DROP TABLE IF EXISTS core.tblPrincipalScope;
DROP TABLE IF EXISTS core.conPrincipalType;
DROP FUNCTION IF EXISTS dbo.MvpDummyHash;
DROP FUNCTION IF EXISTS dbo.MvpDummyUserHashFull;
DROP FUNCTION IF EXISTS dbo.MvpDummyPrincipalHashFull;
DROP FUNCTION IF EXISTS dbo.MvpDummyPrincipalUserHash;
GO
CREATE FUNCTION dbo.MvpDummyHash
(
    @id1 INT,
    @id2 INT
)
RETURNS BIGINT
AS
BEGIN
    RETURN (CAST(@id1 AS BIGINT) * CAST(0x100000000 AS BIGINT))
           + CAST(@id2 AS BIGINT);
END;
GO
CREATE FUNCTION dbo.MvpDummyPrincipalHashFull
(
    @tierId SMALLINT,
    @appId  SMALLINT
)
RETURNS INT
AS
BEGIN
    RETURN (CAST(@tierId AS INT) * 65536)
           + CAST(@appId AS INT);
END;
GO
CREATE FUNCTION dbo.MvpDummyUserHashFull
(
    @tierId SMALLINT,
    @appId  SMALLINT,
    @userId INT
)
RETURNS BIGINT
AS
BEGIN
    RETURN (CAST(@tierId AS BIGINT) * CAST(0x1000000000000 AS BIGINT))
           + (CAST(@appId AS BIGINT) * CAST(0x100000000 AS BIGINT))
           + CAST(@userId AS BIGINT);
END;
GO
CREATE FUNCTION dbo.MvpDummyPrincipalUserHash
(
    @principalTypeId BIGINT,
    @gsmNo           BIGINT
)
RETURNS BIGINT
AS
BEGIN
    RETURN @principalTypeId ^ @gsmNo;
END;
GO