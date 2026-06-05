SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
MERGE core.conPrincipalType AS target
USING (VALUES
    (-1, 0, N'system_root',           N'System Root',            N'Tum guard''lari bypass eder. Break-glass; rutin icin kullanilmaz.',              N'unlimited'),
    (-2, 0, N'system_service',        N'System Service',         N'Tum scope''larda calisir; yalniz kendi job''una ozel dar yetki seti.',            N'scope_bounded'),
    (-2, 1, N'system_service_trash',  N'System Trash Cleaner',   N'Tum scope''larda calisir; yalniz trash_cleaner job''una ozel dar yetki seti.',     N'scope_bounded'),
    (-3, 0, N'system_public',         N'System Public',          N'Evrende yalniz IS_PUBLIC=1 kayitlarini okur. Read-only tavan.',                   N'read_only'),
    (-4, 0, N'scope_root',            N'Scope Root',             N'Yalniz atandigi scope icinde yetkilidir; scope disina cikamaz.',                  N'scope_bounded'),
    (-5, 0, N'scope_public',          N'Scope Public',           N'Yalniz atandigi scope''un public vitrini okur.',                                  N'read_only'),
    ( 0, 0, N'denied',               N'Denied',                 N'Mutlak default. Atanmamis/null/bilinmeyen her durum buraya dusur ve reddedilir.',  N'none'),
    ( 1, 0, N'guest',                N'Guest',                  N'Anonim ziyaretci. Kimlik yok. Aktif context''in public vitrini.',                 N'read_only'),
    ( 2, 0, N'public_member',        N'Public Member',          N'Kimlikli, tenant-bagimsiz tuketici (favori/yorum vb. mikro-aksiyon).',            N'scope_bounded'),
    ( 3, 0, N'tenant_member',        N'Tenant Member',          N'Klasik tenant. Organization/Property eksenli; rol/izin (Faz 2) buradan isler.',   N'scope_bounded')
) AS source (TIER_ID, APP_ID, CODE, DISPLAY_NAME, DESCRIPTION, CEILING_LEVEL)
ON target.TIER_ID = source.TIER_ID AND target.APP_ID = source.APP_ID
WHEN MATCHED THEN
    UPDATE SET CODE = source.CODE, DISPLAY_NAME = source.DISPLAY_NAME,
               DESCRIPTION = source.DESCRIPTION, CEILING_LEVEL = source.CEILING_LEVEL
WHEN NOT MATCHED BY TARGET THEN
    INSERT (TIER_ID, APP_ID, CODE, DISPLAY_NAME, DESCRIPTION, CEILING_LEVEL, STATUS, DELETED)
    VALUES (source.TIER_ID, source.APP_ID, source.CODE, source.DISPLAY_NAME, source.DESCRIPTION, source.CEILING_LEVEL, 1, 0);
GO