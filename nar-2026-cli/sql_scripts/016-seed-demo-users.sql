SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

-- ═══════════════════════════════════════════════════════════════
-- Demo Users + Scope Hierarchy Seed
-- ═══════════════════════════════════════════════════════════════

-- ─── 1. Demo Users ───

-- individual_demo_user: public_member (TIER=2, APP=0, PRINCIPAL=131072)
IF NOT EXISTS (SELECT 1 FROM core.tblUser WHERE USERNAME = N'individual_demo_user')
    INSERT INTO core.tblUser (USERNAME, EMAIL, DISPLAY_NAME, GSM_NO, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (N'individual_demo_user', N'individual_demo@netloji.local', N'Individual Demo User', 9053000000001, 131072, -1, -1);

-- tenant_demo_user_woo: tenant_member (TIER=3, APP=0, PRINCIPAL=196608) — with org+property
IF NOT EXISTS (SELECT 1 FROM core.tblUser WHERE USERNAME = N'tenant_demo_user_woo')
    INSERT INTO core.tblUser (USERNAME, EMAIL, DISPLAY_NAME, GSM_NO, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (N'tenant_demo_user_woo', N'tenant_woo@netloji.local', N'Tenant Demo User WOO', 9053000000002, 196608, -1, -1);

-- tenant_demo_user_wto: tenant_member (TIER=3, APP=0, PRINCIPAL=196608) — with org+property
IF NOT EXISTS (SELECT 1 FROM core.tblUser WHERE USERNAME = N'tenant_demo_user_wto')
    INSERT INTO core.tblUser (USERNAME, EMAIL, DISPLAY_NAME, GSM_NO, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (N'tenant_demo_user_wto', N'tenant_wto@netloji.local', N'Tenant Demo User WTO', 9053000000003, 196608, -1, -1);
GO

-- ─── 2. Scopes ───

-- individual_demo_user → individual scope (SCOPE_TYPE=0)
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 2)
BEGIN
    SET IDENTITY_INSERT core.tblScope ON;
    INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, STATUS, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (2, 0, 1, -1, -1);
    SET IDENTITY_INSERT core.tblScope OFF;
END

-- tenant_demo_user_woo → property scope (SCOPE_TYPE=1)
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 3)
BEGIN
    SET IDENTITY_INSERT core.tblScope ON;
    INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, STATUS, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (3, 1, 1, -1, -1);
    SET IDENTITY_INSERT core.tblScope OFF;
END

-- tenant_demo_user_wto → organization scope (SCOPE_TYPE=2)
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 4)
BEGIN
    SET IDENTITY_INSERT core.tblScope ON;
    INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, STATUS, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (4, 2, 1, -1, -1);
    SET IDENTITY_INSERT core.tblScope OFF;
END

-- tenant_demo_user_wto → property scope under org (SCOPE_TYPE=1)
IF NOT EXISTS (SELECT 1 FROM core.tblScope WHERE SCOPE_ID = 5)
BEGIN
    SET IDENTITY_INSERT core.tblScope ON;
    INSERT INTO core.tblScope (SCOPE_ID, SCOPE_TYPE, STATUS, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (5, 1, 1, -1, -1);
    SET IDENTITY_INSERT core.tblScope OFF;
END
GO

-- ─── 3. Organization + Property (tenant_demo_user_wto) ───

-- Organization demo (SCOPE_ID=4)
IF NOT EXISTS (SELECT 1 FROM core.tblOrganization WHERE ORG_NAME = N'Demo Organization')
    INSERT INTO core.tblOrganization (SCOPE_ID, ORG_NAME, STATUS, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (4, N'Demo Organization', 1, -1, -1);

-- Property demo under organization (SCOPE_ID=5, OWNER_ORGANIZATION_ID = org above)
IF NOT EXISTS (SELECT 1 FROM core.tblProperty WHERE PROP_NAME = N'Demo Property')
    INSERT INTO core.tblProperty (SCOPE_ID, ORGANIZATION_ID, OWNER_ORGANIZATION_ID, PROP_NAME, STATUS, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (5, 1, 1, N'Demo Property', 1, -1, -1);

-- Property demo for tenant_demo_user_woo (SCOPE_ID=3, no org)
IF NOT EXISTS (SELECT 1 FROM core.tblProperty WHERE PROP_NAME = N'Demo Property WOO')
    INSERT INTO core.tblProperty (SCOPE_ID, PROP_NAME, STATUS, CREATOR_USER_ID, EDITOR_USER_ID)
    VALUES (3, N'Demo Property WOO', 1, -1, -1);
GO

-- ─── 4. tblUserScopePrincipalMatrix ───

-- individual_demo_user → individual scope (SCOPE_ID=2, PRINCIPAL=131072)
IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = (SELECT USER_ID FROM core.tblUser WHERE USERNAME = N'individual_demo_user') AND SCOPE_ID = 2)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    SELECT u.USER_ID, 2, u.PRINCIPAL_TYPE_ID, -1, -1
    FROM core.tblUser u WHERE u.USERNAME = N'individual_demo_user';

-- tenant_demo_user_woo → property scope (SCOPE_ID=3, PRINCIPAL=196608)
IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = (SELECT USER_ID FROM core.tblUser WHERE USERNAME = N'tenant_demo_user_woo') AND SCOPE_ID = 3)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    SELECT u.USER_ID, 3, u.PRINCIPAL_TYPE_ID, -1, -1
    FROM core.tblUser u WHERE u.USERNAME = N'tenant_demo_user_woo';

-- tenant_demo_user_wto → org scope (SCOPE_ID=4, PRINCIPAL=196608)
IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = (SELECT USER_ID FROM core.tblUser WHERE USERNAME = N'tenant_demo_user_wto') AND SCOPE_ID = 4)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    SELECT u.USER_ID, 4, u.PRINCIPAL_TYPE_ID, -1, -1
    FROM core.tblUser u WHERE u.USERNAME = N'tenant_demo_user_wto';

-- tenant_demo_user_wto → property scope (SCOPE_ID=5, PRINCIPAL=196608)
IF NOT EXISTS (SELECT 1 FROM core.tblUserScopePrincipalMatrix WHERE USER_ID = (SELECT USER_ID FROM core.tblUser WHERE USERNAME = N'tenant_demo_user_wto') AND SCOPE_ID = 5)
    INSERT INTO core.tblUserScopePrincipalMatrix (USER_ID, SCOPE_ID, PRINCIPAL_TYPE_ID, CREATOR_USER_ID, EDITOR_USER_ID)
    SELECT u.USER_ID, 5, u.PRINCIPAL_TYPE_ID, -1, -1
    FROM core.tblUser u WHERE u.USERNAME = N'tenant_demo_user_wto';
GO

-- ═══════════════════════════════════════════════════════════════
-- Verify
-- ═══════════════════════════════════════════════════════════════
SET QUOTED_IDENTIFIER ON;
SELECT u.USER_ID, u.USERNAME, u.PRINCIPAL_TYPE_ID, pt.CODE AS PRINCIPAL,
       m.SCOPE_ID, s.SCOPE_TYPE, st.CODE AS SCOPE_TYPE_CODE
FROM core.tblUser u
JOIN core.tblUserScopePrincipalMatrix m ON u.USER_ID = m.USER_ID
JOIN core.tblScope s ON m.SCOPE_ID = s.SCOPE_ID
JOIN core.conScopeType st ON s.SCOPE_TYPE = st.SCOPE_TYPE
LEFT JOIN core.conPrincipalType pt ON u.PRINCIPAL_TYPE_ID = pt.PRINCIPAL_TYPE_ID
WHERE u.USERNAME LIKE N'%demo%'
ORDER BY u.USER_ID;
GO
