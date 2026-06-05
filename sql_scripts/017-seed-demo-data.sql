SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

-- ═══════════════════════════════════════════════════════════════
-- Demo Data Seed — 4 kayıt, farklı user + scope kombinasyonları
-- ═══════════════════════════════════════════════════════════════

-- 1. individual_demo_user → individual scope (SCOPE_ID=2)
--    Audit: CREATOR=3, CREATOR_SCOPE=2 (bireysel erişim)
IF NOT EXISTS (SELECT 1 FROM dbo.tblDemoData WHERE PLACE_HOLDER = N'INDIVIDUAL_DEMO_001')
    INSERT INTO dbo.tblDemoData (PLACE_HOLDER, STATUS, LCID, DEFAULT_LCID, OWNER_SCOPE_ID,
        CREATOR_USER_ID, CREATOR_SCOPE_ID, EDITOR_USER_ID, EDITOR_SCOPE_ID)
    VALUES (N'INDIVIDUAL_DEMO_001', 1, 1033, 1033, 2,
        3, 2, 3, 2);

-- 2. tenant_demo_user_woo → property scope (SCOPE_ID=3)
--    Audit: CREATOR=4, CREATOR_SCOPE=3 (kurumsal tekil — property düzeyinde)
IF NOT EXISTS (SELECT 1 FROM dbo.tblDemoData WHERE PLACE_HOLDER = N'TENANT_WOO_DEMO_001')
    INSERT INTO dbo.tblDemoData (PLACE_HOLDER, STATUS, LCID, DEFAULT_LCID, OWNER_SCOPE_ID,
        CREATOR_USER_ID, CREATOR_SCOPE_ID, EDITOR_USER_ID, EDITOR_SCOPE_ID)
    VALUES (N'TENANT_WOO_DEMO_001', 1, 1033, 1033, 3,
        4, 3, 4, 3);

-- 3. tenant_demo_user_wto → organization scope (SCOPE_ID=4)
--    Audit: CREATOR=5, CREATOR_SCOPE=4 (kurumsal çok şubeli — org düzeyinde)
IF NOT EXISTS (SELECT 1 FROM dbo.tblDemoData WHERE PLACE_HOLDER = N'TENANT_WTO_DEMO_001')
    INSERT INTO dbo.tblDemoData (PLACE_HOLDER, STATUS, LCID, DEFAULT_LCID, OWNER_SCOPE_ID,
        CREATOR_USER_ID, CREATOR_SCOPE_ID, EDITOR_USER_ID, EDITOR_SCOPE_ID)
    VALUES (N'TENANT_WTO_DEMO_001', 1, 1033, 1033, 4,
        5, 4, 5, 4);

-- 4. tenant_demo_user_wto → property scope (SCOPE_ID=5) — org'a bağlı property
--    Audit: CREATOR=5, CREATOR_SCOPE=5 (kurumsal çok şubeli — şube/mülk düzeyinde)
IF NOT EXISTS (SELECT 1 FROM dbo.tblDemoData WHERE PLACE_HOLDER = N'TENANT_WTO_DEMO_002')
    INSERT INTO dbo.tblDemoData (PLACE_HOLDER, STATUS, LCID, DEFAULT_LCID, OWNER_SCOPE_ID,
        CREATOR_USER_ID, CREATOR_SCOPE_ID, EDITOR_USER_ID, EDITOR_SCOPE_ID)
    VALUES (N'TENANT_WTO_DEMO_002', 1, 1055, 1033, 5,
        5, 5, 5, 5);
GO

-- ═══════════════════════════════════════════════════════════════
-- Verify
-- ═══════════════════════════════════════════════════════════════
SET QUOTED_IDENTIFIER ON;
SELECT d.DEMODATA_ID, d.PLACE_HOLDER, d.OWNER_SCOPE_ID, s.SCOPE_TYPE, st.CODE AS SCOPE_TYPE_CODE,
       d.CREATOR_USER_ID, u.USERNAME AS CREATOR_NAME,
       d.CREATOR_SCOPE_ID, d.EDITOR_USER_ID, d.EDITOR_SCOPE_ID
FROM dbo.tblDemoData d
JOIN core.tblScope s ON d.OWNER_SCOPE_ID = s.SCOPE_ID
JOIN core.conScopeType st ON s.SCOPE_TYPE = st.SCOPE_TYPE
JOIN core.tblUser u ON d.CREATOR_USER_ID = u.USER_ID
WHERE d.DELETED = 0
ORDER BY d.DEMODATA_ID;
GO
