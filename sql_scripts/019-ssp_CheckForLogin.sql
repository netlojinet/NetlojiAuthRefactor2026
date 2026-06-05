SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
-- ============================================================
-- Stage 1: core.ssp_CheckForLogin  (framework login methodunun yeni-model refactor'u)
-- Eski model: GSM ile user bul + UserAccessibleOrganizations/Properties say (cift eksen).
-- Yeni model: GSM ile user bul + matrix-tabanli UserAccessibleScopes (tek eksen,
--             scope + per-scope principal + ceiling). System (<0) ve user (>0) ayni yol.
-- Not: GSM non-unique olabilir (ayni GSM farkli principal'larda). Bu surumde TOP(1);
--      coklu-principal ayristirmasi USER_KEY ile (cross-scope asamasinda).
-- ============================================================
CREATE OR ALTER PROCEDURE core.ssp_CheckForLogin
    @GSM_NO BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @USER_ID INT;
    DECLARE @RETURN_CODE INT = 200;
    DECLARE @RETURN_MESSAGE NVARCHAR(200) = N'OK';

    SELECT TOP (1) @USER_ID = u.USER_ID
    FROM core.tblUser u
    WHERE u.GSM_NO = @GSM_NO
      AND u.DELETED = 0
      AND u.STATUS = 1
    ORDER BY u.USER_ID;

    IF @USER_ID IS NULL
    BEGIN
        SET @RETURN_CODE = 404;
        SET @RETURN_MESSAGE = N'User not found.';
    END

    -- Result 1: principal header (tier + ceiling)
    SELECT
        u.USER_ID,
        u.USERNAME,
        u.PRINCIPAL_TYPE_ID,
        pt.TIER_ID,
        pt.APP_ID,
        pt.CODE          AS PRINCIPAL_CODE,
        pt.CEILING_LEVEL,
        u.USER_KEY,
        @RETURN_CODE      AS RETURN_CODE,
        @RETURN_MESSAGE   AS RETURN_MESSAGE
    FROM core.tblUser u
    LEFT JOIN core.conPrincipalType pt
        ON pt.PRINCIPAL_TYPE_ID = u.PRINCIPAL_TYPE_ID
    WHERE u.USER_ID = @USER_ID;

    -- Result 2: accessible scopes (matrix-based, per-scope principal)
    SELECT
        SCOPE_ID,
        PRINCIPAL_TYPE_ID,
        CEILING_LEVEL,
        SOURCE
    FROM core.UserAccessibleScopes(@USER_ID)
    ORDER BY SCOPE_ID;

    -- Result 3: yapisal alt inis (parent org-scope -> child property-scope) — §5.2 working set
    SELECT
        org.SCOPE_ID  AS PARENT_SCOPE_ID,
        prop.SCOPE_ID AS CHILD_SCOPE_ID
    FROM core.UserAccessibleScopes(@USER_ID) acc
    JOIN core.tblOrganization org ON org.SCOPE_ID = acc.SCOPE_ID AND org.DELETED = 0
    JOIN core.tblProperty prop ON prop.OWNER_ORGANIZATION_ID = org.ORGANIZATION_ID AND prop.DELETED = 0
    WHERE prop.STATUS = 1
    ORDER BY org.SCOPE_ID, prop.SCOPE_ID;
END;
GO
