SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
-- ============================================================
-- Stage 5 (F2): scope SAHİPLİĞİ — her scope tek owner user'a kilitlenir.
-- Erişim (matrix) DEĞİL, sahiplik ekseni. owned-tree artık veri-güdümlü.
--   -1 root        -> -1 (system_root)
--    2 individual  ->  3 (individual_demo_user)   ← individual<->scope çözüldü
--    3 property woo ->  4 (tenant_demo_user_woo)
--    4 organization ->  5 (tenant_demo_user_wto, kurucu)
--    5 property     ->  5 (tenant_demo_user_wto)
-- ============================================================
UPDATE core.tblScope SET OWNER_USER_ID = -1 WHERE SCOPE_ID = -1;
UPDATE core.tblScope SET OWNER_USER_ID =  3 WHERE SCOPE_ID =  2;
UPDATE core.tblScope SET OWNER_USER_ID =  4 WHERE SCOPE_ID =  3;
UPDATE core.tblScope SET OWNER_USER_ID =  5 WHERE SCOPE_ID =  4;
UPDATE core.tblScope SET OWNER_USER_ID =  5 WHERE SCOPE_ID =  5;
GO

SELECT s.SCOPE_ID, st.CODE AS scope_type, s.OWNER_USER_ID, u.USERNAME AS owner_user
FROM core.tblScope s
LEFT JOIN core.conScopeType st ON st.SCOPE_TYPE = s.SCOPE_TYPE
LEFT JOIN core.tblUser u ON u.USER_ID = s.OWNER_USER_ID
WHERE s.DELETED = 0
ORDER BY s.SCOPE_ID;
GO
