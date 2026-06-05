SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
-- ============================================================
-- Stage 3: IS_PUBLIC karışımı (system_public / scope_public testleri için)
-- scope 2 (individual) + scope 4 (org) → public ; scope 3 + scope 5 → private.
-- Beklenen: toplam 4 demo, IS_PUBLIC=1 olan 2.
-- ============================================================
UPDATE dbo.tblDemoData SET IS_PUBLIC = 1 WHERE OWNER_SCOPE_ID IN (2, 4) AND DELETED = 0;
GO

SELECT OWNER_SCOPE_ID, IS_PUBLIC, COUNT(*) AS cnt
FROM dbo.tblDemoData
WHERE DELETED = 0
GROUP BY OWNER_SCOPE_ID, IS_PUBLIC
ORDER BY OWNER_SCOPE_ID;
GO
