SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
-- conPrincipalType = principal yetenek katalogu (TEK dogruluk kaynagi). C# bunu runtime yukler (§1.2 invariant 4).
-- Kolonlar: CEILING_LEVEL, REACH_LEVEL(all/granted/none), BYPASS_GUARD, CAN_WRITE, PUBLIC_ONLY
MERGE core.conPrincipalType AS target
USING (VALUES
    (-1, 0, N'system_root',           N'System Root',          N'Tum guard''lari bypass eder. Break-glass.',                          N'unlimited',     N'all',     1, 1, 0),
    (-2, 0, N'system_service',        N'System Service',       N'Tum scope''larda calisir; dar yetki seti.',                         N'scope_bounded', N'all',     0, 1, 0),
    (-2, 1, N'system_service_trash',  N'System Trash Cleaner', N'Tum scope''larda; yalniz trash_cleaner job.',                       N'scope_bounded', N'all',     0, 1, 0),
    (-3, 0, N'system_public',         N'System Public',        N'Evrende yalniz IS_PUBLIC=1 okur. Read-only.',                       N'read_only',     N'all',     0, 0, 1),
    (-4, 0, N'scope_root',            N'Scope Root',           N'Yalniz atandigi scope icinde tam yetki.',                           N'scope_bounded', N'granted', 0, 1, 0),
    (-5, 0, N'scope_public',          N'Scope Public',         N'Yalniz atandigi scope''un public vitrini.',                         N'read_only',     N'granted', 0, 0, 1),
    ( 0, 0, N'denied',                N'Denied',               N'Mutlak default. Atanmamis/null/bilinmeyen -> reddedilir.',          N'none',          N'none',    0, 0, 0),
    ( 1, 0, N'guest',                 N'Guest',                N'Anonim ziyaretci. Aktif context public vitrini.',                   N'read_only',     N'granted', 0, 0, 1),
    ( 2, 0, N'public_member',         N'Public Member',        N'Kimlikli, tenant-bagimsiz tuketici (mikro-aksiyon).',               N'scope_bounded', N'granted', 0, 1, 0),
    ( 3, 0, N'tenant_member',         N'Tenant Member',        N'Klasik tenant. Org/Property eksenli; rol (Faz 2).',                 N'scope_bounded', N'granted', 0, 1, 0)
) AS source (TIER_ID, APP_ID, CODE, DISPLAY_NAME, DESCRIPTION, CEILING_LEVEL, REACH_LEVEL, BYPASS_GUARD, CAN_WRITE, PUBLIC_ONLY)
ON target.TIER_ID = source.TIER_ID AND target.APP_ID = source.APP_ID
WHEN MATCHED THEN
    UPDATE SET CODE = source.CODE, DISPLAY_NAME = source.DISPLAY_NAME, DESCRIPTION = source.DESCRIPTION,
               CEILING_LEVEL = source.CEILING_LEVEL, REACH_LEVEL = source.REACH_LEVEL,
               BYPASS_GUARD = source.BYPASS_GUARD, CAN_WRITE = source.CAN_WRITE, PUBLIC_ONLY = source.PUBLIC_ONLY
WHEN NOT MATCHED BY TARGET THEN
    INSERT (TIER_ID, APP_ID, CODE, DISPLAY_NAME, DESCRIPTION, CEILING_LEVEL, REACH_LEVEL, BYPASS_GUARD, CAN_WRITE, PUBLIC_ONLY, STATUS, DELETED)
    VALUES (source.TIER_ID, source.APP_ID, source.CODE, source.DISPLAY_NAME, source.DESCRIPTION, source.CEILING_LEVEL, source.REACH_LEVEL,
            source.BYPASS_GUARD, source.CAN_WRITE, source.PUBLIC_ONLY, 1, 0);
GO
