SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
-- ============================================================
-- Stage 6: core.UserAccessibleScopes — <0 SYSTEM OVERRIDE first-class (Anayasa §5.3).
-- Reach KATALOG-GUUDUMLU (conPrincipalType.REACH_LEVEL), login principal'e gore:
--   'all'     (system_root/service/public) -> TUM aktif scope (source='override'); matrix kisa devre.
--   'granted' (scope_*, domain)            -> matrix DOGRUDAN grant ∪ yapisal alt (org->property).
--   'none'    (denied)                     -> bos.
-- Boylece system reach GOZLEMLENEBILIR (GUI grid) ve TEK karar noktasi (TVF). §2/§4.
-- ============================================================
CREATE OR ALTER FUNCTION core.UserAccessibleScopes(@USER_ID INT)
RETURNS TABLE
AS
RETURN
(
    WITH UserReach AS
    (
        SELECT TOP (1) u.PRINCIPAL_TYPE_ID, pt.REACH_LEVEL
        FROM core.tblUser u
        JOIN core.conPrincipalType pt ON pt.PRINCIPAL_TYPE_ID = u.PRINCIPAL_TYPE_ID
        WHERE u.USER_ID = @USER_ID AND u.DELETED = 0
    ),
    AllReach AS
    (
        -- reach='all' -> tum aktif scope'lar, login principal ile (kisa devre; matrix yok sayilir)
        SELECT s.SCOPE_ID, ur.PRINCIPAL_TYPE_ID
        FROM core.tblScope s
        CROSS JOIN UserReach ur
        WHERE ur.REACH_LEVEL = N'all' AND s.STATUS = 1 AND s.DELETED = 0
    ),
    Direct AS
    (
        -- reach='granted' -> matrix dogrudan grant
        SELECT m.SCOPE_ID, m.PRINCIPAL_TYPE_ID
        FROM core.tblUserScopePrincipalMatrix m
        CROSS JOIN UserReach ur
        WHERE ur.REACH_LEVEL = N'granted' AND m.USER_ID = @USER_ID AND m.DELETED = 0 AND m.STATUS = 1
    ),
    Descend AS
    (
        -- org-scope (Direct icindeki) -> o org'a ait property scope'lari; principal miras.
        SELECT prop.SCOPE_ID, d.PRINCIPAL_TYPE_ID
        FROM Direct d
        JOIN core.tblOrganization org ON org.SCOPE_ID = d.SCOPE_ID AND org.DELETED = 0
        JOIN core.tblProperty prop ON prop.OWNER_ORGANIZATION_ID = org.ORGANIZATION_ID AND prop.DELETED = 0
    ),
    Combined AS
    (
        SELECT SCOPE_ID, PRINCIPAL_TYPE_ID, 0 AS PRIORITY, CAST(N'override'   AS NVARCHAR(16)) AS SOURCE FROM AllReach
        UNION ALL
        SELECT SCOPE_ID, PRINCIPAL_TYPE_ID, 1 AS PRIORITY, CAST(N'direct'     AS NVARCHAR(16)) AS SOURCE FROM Direct
        UNION ALL
        SELECT SCOPE_ID, PRINCIPAL_TYPE_ID, 2 AS PRIORITY, CAST(N'descendant' AS NVARCHAR(16)) AS SOURCE FROM Descend
    ),
    Ranked AS
    (
        SELECT SCOPE_ID, PRINCIPAL_TYPE_ID, SOURCE,
               ROW_NUMBER() OVER (PARTITION BY SCOPE_ID ORDER BY PRIORITY) AS RN
        FROM Combined
    )
    SELECT
        r.SCOPE_ID,
        r.PRINCIPAL_TYPE_ID,
        pt.CEILING_LEVEL,
        r.SOURCE
    FROM Ranked r
    JOIN core.tblScope s
        ON s.SCOPE_ID = r.SCOPE_ID AND s.STATUS = 1 AND s.DELETED = 0
    LEFT JOIN core.conPrincipalType pt
        ON pt.PRINCIPAL_TYPE_ID = r.PRINCIPAL_TYPE_ID
    WHERE r.RN = 1
);
GO
