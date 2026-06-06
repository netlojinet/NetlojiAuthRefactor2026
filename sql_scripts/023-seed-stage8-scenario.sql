SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
-- ============================================================
-- Stage 8: senaryo (99-doc domain sekilleri) + audit seed duzeltme (F1).
--   3 GALERI: individual / property-only / org-zincir(3 prop)
--   1 OTOKENT: scope_root SISTEM hesabi (negatif USER_ID), yalniz org scope (property yok).
-- ============================================================

-- ─── Users ───
SET IDENTITY_INSERT core.tblUser ON;
IF NOT EXISTS (SELECT 1 FROM core.tblUser WHERE USER_ID = 7)
    INSERT INTO core.tblUser (USER_ID, USERNAME, EMAIL, DISPLAY_NAME, GSM_NO, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (7, N'gallery_individual_user', N'gal_ind@netloji.local',  N'Galeri Bireysel', 9053000000007, 196608, -1, -1);
IF NOT EXISTS (SELECT 1 FROM core.tblUser WHERE USER_ID = 8)
    INSERT INTO core.tblUser (USER_ID, USERNAME, EMAIL, DISPLAY_NAME, GSM_NO, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (8, N'gallery_property_user', N'gal_prop@netloji.local', N'Galeri Tek-Mulk', 9053000000008, 196608, -1, -1);
IF NOT EXISTS (SELECT 1 FROM core.tblUser WHERE USER_ID = 9)
    INSERT INTO core.tblUser (USER_ID, USERNAME, EMAIL, DISPLAY_NAME, GSM_NO, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (9, N'gallery_chain_user', N'gal_chain@netloji.local', N'Galeri Zincir', 9053000000009, 196608, -1, -1);
-- otokent_root: scope_root (-262144) SISTEM hesabi, negatif USER_ID, sentinel GSM (login disi)
IF NOT EXISTS (SELECT 1 FROM core.tblUser WHERE USER_ID = -6)
    INSERT INTO core.tblUser (USER_ID, USERNAME, EMAIL, DISPLAY_NAME, GSM_NO, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (-6, N'otokent_root', N'otokent_root@netloji.local', N'Otokent Root (scope_root)', 7000000000006, -262144, -1, -1);
SET IDENTITY_INSERT core.tblUser OFF;
GO

-- ─── Scopes (OWNER_USER_ID = sahip; creator/editor de owner = audit net) ───
SET IDENTITY_INSERT core.tblScope ON;
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 6)  INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, OWNER_USER_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (6,  0, 7, 7, 7);   -- galeri individual
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 7)  INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, OWNER_USER_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (7,  1, 8, 8, 8);   -- galeri property-only
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 8)  INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, OWNER_USER_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (8,  2, 9, 9, 9);   -- galeri zincir ORG
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 9)  INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, OWNER_USER_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (9,  1, 9, 9, 9);   -- zincir sube 1
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 10) INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, OWNER_USER_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (10, 1, 9, 9, 9);   -- zincir sube 2
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 11) INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, OWNER_USER_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (11, 1, 9, 9, 9);   -- zincir sube 3
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 12) INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, OWNER_USER_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (12, 2, -6, -6, -6); -- otokent ORG
SET IDENTITY_INSERT core.tblScope OFF;
GO

-- ─── Orgs ───
IF NOT EXISTS (SELECT 1 FROM core.tblOrganization WHERE SCOPE_ID = 8)
    INSERT INTO core.tblOrganization (SCOPE_ID, ORG_NAME, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (8, N'Galeri Zincir A.S.', 9, 9);
IF NOT EXISTS (SELECT 1 FROM core.tblOrganization WHERE SCOPE_ID = 12)
    INSERT INTO core.tblOrganization (SCOPE_ID, ORG_NAME, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (12, N'Otokent AVM', -6, -6);
GO

-- ─── Properties ───
IF NOT EXISTS (SELECT 1 FROM core.tblProperty WHERE SCOPE_ID = 7)
    INSERT INTO core.tblProperty (SCOPE_ID, PROP_NAME, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (7, N'Tek Galeri', 8, 8);   -- standalone
DECLARE @chainOrg INT = (SELECT ORGANIZATION_ID FROM core.tblOrganization WHERE SCOPE_ID = 8);
IF NOT EXISTS (SELECT 1 FROM core.tblProperty WHERE SCOPE_ID = 9)
    INSERT INTO core.tblProperty (SCOPE_ID, ORGANIZATION_ID, OWNER_ORGANIZATION_ID, PROP_NAME, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (9,  @chainOrg, @chainOrg, N'Zincir Sube 1', 9, 9);
IF NOT EXISTS (SELECT 1 FROM core.tblProperty WHERE SCOPE_ID = 10)
    INSERT INTO core.tblProperty (SCOPE_ID, ORGANIZATION_ID, OWNER_ORGANIZATION_ID, PROP_NAME, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (10, @chainOrg, @chainOrg, N'Zincir Sube 2', 9, 9);
IF NOT EXISTS (SELECT 1 FROM core.tblProperty WHERE SCOPE_ID = 11)
    INSERT INTO core.tblProperty (SCOPE_ID, ORGANIZATION_ID, OWNER_ORGANIZATION_ID, PROP_NAME, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (11, @chainOrg, @chainOrg, N'Zincir Sube 3', 9, 9);
GO

-- ─── Matrix grant'leri ───
IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = 7  AND SCOPE_ID = 6)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (7, 6, 196608, -1, -1);
IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = 8  AND SCOPE_ID = 7)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (8, 7, 196608, -1, -1);
IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = 9  AND SCOPE_ID = 8)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (9, 8, 196608, -1, -1);  -- org -> 3 prop descent
IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = -6 AND SCOPE_ID = 12)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID) VALUES (-6, 12, -262144, -1, -1); -- otokent_root scope_root
GO

-- ─── Audit seed duzeltme (F1): scope/org/property CREATOR/EDITOR = scope sahibi ───
UPDATE core.tblScope SET CREATOR_USER_ID = OWNER_USER_ID, EDITOR_USER_ID = OWNER_USER_ID WHERE DELETED = 0;
UPDATE o SET o.CREATOR_USER_ID = s.OWNER_USER_ID, o.EDITOR_USER_ID = s.OWNER_USER_ID
    FROM core.tblOrganization o JOIN core.tblScope s ON s.SCOPE_ID = o.SCOPE_ID WHERE o.DELETED = 0;
UPDATE p SET p.CREATOR_USER_ID = s.OWNER_USER_ID, p.EDITOR_USER_ID = s.OWNER_USER_ID
    FROM core.tblProperty p JOIN core.tblScope s ON s.SCOPE_ID = p.SCOPE_ID WHERE p.DELETED = 0;
GO

-- ─── Verify ───
SELECT u.USER_ID, u.USERNAME, pt.CODE AS principal, m.SCOPE_ID, st.CODE AS scope_type, s.OWNER_USER_ID
FROM core.tblUser u
JOIN core.tblUserScopePrincipalMatrix m ON m.USER_ID = u.USER_ID AND m.DELETED = 0
JOIN core.tblScope s ON s.SCOPE_ID = m.SCOPE_ID
JOIN core.conScopeType st ON st.SCOPE_TYPE = s.SCOPE_TYPE
LEFT JOIN core.conPrincipalType pt ON pt.PRINCIPAL_TYPE_ID = u.PRINCIPAL_TYPE_ID
WHERE u.USER_ID IN (7, 8, 9, -6)
ORDER BY u.USER_ID;
GO
