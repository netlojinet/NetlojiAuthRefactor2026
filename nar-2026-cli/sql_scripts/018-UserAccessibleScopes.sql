SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
-- ============================================================
-- Stage 2: core.UserAccessibleScopes
-- §5.1: erisim = DOGRUDAN grant ∪ YAPISAL ALT (org-scope -> property), ∩ aktif scope.
-- Per-scope principal: direct'te matrix principal'i; descendant'ta org grant'inden MIRAS.
-- Dedup: ayni scope hem direct hem descendant ise DIRECT kazanir.
-- (Derinlik = 2, tek inis: org -> property. OWNER_ORGANIZATION_ID = yapisal parent kenari.)
-- ============================================================
CREATE OR ALTER FUNCTION core.UserAccessibleScopes(@USER_ID INT)
RETURNS TABLE
AS
RETURN
(
    WITH Direct AS
    (
        SELECT m.SCOPE_ID, m.PRINCIPAL_TYPE_ID
        FROM core.tblUserScopePrincipalMatrix m
        WHERE m.USER_ID = @USER_ID AND m.DELETED = 0 AND m.STATUS = 1
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
        SELECT SCOPE_ID, PRINCIPAL_TYPE_ID, 0 AS PRIORITY, CAST(N'direct'     AS NVARCHAR(16)) AS SOURCE FROM Direct
        UNION ALL
        SELECT SCOPE_ID, PRINCIPAL_TYPE_ID, 1 AS PRIORITY, CAST(N'descendant' AS NVARCHAR(16)) AS SOURCE FROM Descend
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
