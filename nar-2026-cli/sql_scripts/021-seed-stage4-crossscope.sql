SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
-- ============================================================
-- Stage 4: cross-scope grant — aynı kimlik, scope'a göre FARKLI principal.
-- agency_demo_user (6): login principal = tenant_member.
--   scope 4 (org)    -> scope_root   (-262144): tam yetki (write); descent ile scope 5.
--   scope 2 (indiv.) -> scope_public (-327680): yalniz public vitrin (read-only).
-- Domain ROL yok — yalniz scope-baglamsal principal (Kat1 tavan) atamasi.
-- ============================================================
SET IDENTITY_INSERT core.tblUser ON;
IF NOT EXISTS (SELECT 1 FROM core.tblUser WHERE USER_ID = 6)
    INSERT INTO core.tblUser (USER_ID, USERNAME, EMAIL, DISPLAY_NAME, GSM_NO, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (6, N'agency_demo_user', N'agency@netloji.local', N'Agency Demo User', 9053000000006, 196608, -1, -1);
SET IDENTITY_INSERT core.tblUser OFF;
GO

-- cross-scope grant'ler
IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = 6 AND SCOPE_ID = 4)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (6, 4, -262144, -1, -1);   -- scope_root @ org scope 4

IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = 6 AND SCOPE_ID = 2)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (6, 2, -327680, -1, -1);   -- scope_public @ individual scope 2
GO

SELECT m.USER_ID, u.USERNAME, m.SCOPE_ID, m.PRINCIPAL_TYPE_ID, pt.CODE AS PRINCIPAL_CODE
FROM core.tblUserScopePrincipalMatrix m
JOIN core.tblUser u ON u.USER_ID = m.USER_ID
LEFT JOIN core.conPrincipalType pt ON pt.PRINCIPAL_TYPE_ID = m.PRINCIPAL_TYPE_ID
WHERE m.USER_ID = 6 AND m.DELETED = 0
ORDER BY m.SCOPE_ID;
GO
