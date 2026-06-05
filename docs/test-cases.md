# Test Kılavuzu — Netloji Auth Refactor 2026 MVP

> Bu doküman, auth refactor MVP'sindeki tüm test senaryolarını listeler.
> Her test, beklenen sonucu ve doğrulama yöntemini içerir.

---

## Genel Bilgi

- **Veritabanı:** `NetlojiAuthRefactor2026` (SQL Server, Windows Auth)
- **Console App:** `dotnet run` ile çalıştırılır
- **Otomatik test:** `dotnet run` açıldığında menüden `3 > Stage 3 Self-Test` ve `4 > Stage 4 Cross-Scope Test` seçilebilir
- **Manuel test:** Menüden `1 > System Demo` veya `2 > User Demo` seçilerek她 bir kullanıcı için test edilir

---

## Temel Veri Yapısı

### Kullanıcılar

| USER_ID | USERNAME | PRINCIPAL_TYPE_ID | PRINCIPAL_CODE | TIER_ID | GSM_NO |
|---------|----------|-------------------|----------------|---------|--------|
| -5 | scope_public_user | -327680 | scope_public | -5 | 8264019375612 |
| -4 | scope_root_user | -262144 | scope_root | -4 | 5930182746103 |
| -3 | system_public_user | -196608 | system_public | -3 | 2648193750284 |
| -2 | system_service_default_user | -131072 | system_service | -2 | 4827361092847 |
| -1 | system_root_user | -65536 | system_root | -1 | 7382910465218 |
| 1 | founder_1 | -65536 | system_root | -1 | 905304158501 |
| 2 | founder_2 | -65536 | system_root | -1 | 905304158502 |
| 3 | individual_demo_user | 131072 | public_member | 2 | 9053000000001 |
| 4 | tenant_demo_user_woo | 196608 | tenant_member | 3 | 9053000000002 |
| 5 | tenant_demo_user_wto | 196608 | tenant_member | 3 | 9053000000003 |
| 6 | agency_demo_user | 196608 | tenant_member | 3 | 9053000000006 |

### Matrix Grant'leri (tblUserScopePrincipalMatrix)

| USER_ID | SCOPE_ID | PRINCIPAL_TYPE_ID | PRINCIPAL_CODE | SOURCE |
|---------|----------|-------------------|----------------|--------|
| -5..-1, 1, 2 | -1 | (kendi principal'ı) | (kendi kodu) | direct |
| 3 | 2 | 131072 | public_member | direct |
| 4 | 3 | 196608 | tenant_member | direct |
| 5 | 4 | 196608 | tenant_member | direct |
| 5 | 5 | 196608 | tenant_member | direct |
| 6 | 2 | -327680 | scope_public | direct |
| 6 | 4 | -262144 | scope_root | direct |

### Scope Hiyerarşisi

```
SCOPE_ID=-1 (root_scope, type=255) — system reserved
├── SCOPE_ID=2 (individual, type=0) — individual_demo_user, agency @ scope_public
├── SCOPE_ID=3 (property, type=1) — tenant_demo_user_woo
├── SCOPE_ID=4 (organization, type=2) — tenant_demo_user_wto, agency @ scope_root
│   └── SCOPE_ID=5 (property, type=1) — tenant_demo_user_wto (org'a bağlı)
```

### Demo Data

| ID | PLACE_HOLDER | IS_PUBLIC | OWNER_SCOPE_ID | CREATOR_USER_ID |
|----|-------------|-----------|----------------|-----------------|
| 1 | INDIVIDUAL_DEMO_001 | 1 | 2 | 3 |
| 2 | TENANT_WOO_DEMO_001 | 0 | 3 | 4 |
| 3 | TENANT_WTO_DEMO_001 | 1 | 4 | 5 |
| 4 | TENANT_WTO_DEMO_002 | 0 | 5 | 5 |

---

## A. Sistem Kullanıcıları Testleri (USER_ID < 0)

### A1. system_root_user (ID=-1) — Guard Bypass, Sınırsız

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=unlimited |
| Reach | AllScopes (tüm scope'lar) |
| Guard Bypass | EVET |
| CanWrite | EVET |
| PublicOnly | HAYIR |
| Working Set | TÜM scope'lar (-1, 2, 3, 4, 5) |
| Görünen veri | 4/4 (tümü) |
| Insert | Başarılı |
| Update | Başarılı |
| Delete | Başarılı |

**Doğrulama:** Menüden `1 > System Demo` → `-1 system_root_user` seç → scope `-1` seç → `4 > Kayıtları Listele` → **4 kayıt görünmeli**.

---

### A2. system_service_default_user (ID=-2) — AllScopes, Guard'a Tabi

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=scope_bounded |
| Reach | AllScopes |
| Guard Bypass | HAYIR |
| CanWrite | EVET |
| PublicOnly | HAYIR |
| Working Set | TÜM scope'lar |
| Görünen veri | 4/4 (tümü) |
| Insert | Başarılı |
| Update | Başarılı |
| Delete | Başarılı |

**Doğrulama:** Menüden `1 > System Demo` → `-2 system_service_default_user` seç → scope `-1` seç → Listele → **4 kayıt**.

---

### A3. system_public_user (ID=-3) — AllScopes + Read-Only + IS_PUBLIC

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=read_only |
| Reach | AllScopes |
| Guard Bypass | HAYIR |
| CanWrite | HAYIR |
| PublicOnly | EVET |
| Working Set | TÜM scope'lar |
| Görünen veri | 2/4 (yalnız IS_PUBLIC=1) |
| Insert | **ENGELLENDİ** (read-only) |
| Update | **ENGELLENDİ** (read-only) |
| Delete | **ENGELLENDİ** (read-only) |

**Doğrulama:** Menüden `1 > System Demo` → `-3 system_public_user` seç → scope `-1` seç → Listele → **2 kayıt** (ID=1, ID=3) → Insert dene → `[ENGELLENDİ]` mesajı.

---

### A4. scope_root_user (ID=-4) — GrantedScopes, Write

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=scope_bounded |
| Reach | GrantedScopes (yalnız grant'li scope) |
| Guard Bypass | HAYIR |
| CanWrite | EVET |
| PublicOnly | HAYIR |
| Grant | SCOPE_ID=-1 (root_scope) |
| Working Set | {-1} |
| Görünen veri | 0/4 (root_scope'a ait demo data yok) |
| Insert | Başarılı (SCOPE_ID=-1 olarak eklenir) |

**Doğrulama:** Menüden `1 > System Demo` → `-4 scope_root_user` seç → scope `-1` seç → Listele → **0 kayıt** → Insert dene → başarılı, ama listelemede görünmüyor (scope=-1'te veri yok).

---

### A5. scope_public_user (ID=-5) — GrantedScopes, Read-Only, IS_PUBLIC

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=read_only |
| Reach | GrantedScopes |
| Guard Bypass | HAYIR |
| CanWrite | HAYIR |
| PublicOnly | EVET |
| Grant | SCOPE_ID=-1 (root_scope) |
| Görünen veri | 0/4 (root_scope + IS_PUBLIC filtresi) |
| Insert | **ENGELLENDİ** (read-only) |

**Doğrulama:** Menüden `1 > System Demo` → `-5 scope_public_user` seç → scope `-1` seç → Listele → **0 kayıt** → Insert dene → `[ENGELLENDİ]`.

---

## B. Domain Kullanıcıları Testleri (USER_ID > 0)

### B1. founder_1 (ID=1) — System Root, Guard Bypass

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=unlimited |
| Reach | AllScopes (guard bypass) |
| Grant | SCOPE_ID=-1 (root_scope) |
| Görünen veri | 4/4 (tümü) |
| Insert | Başarılı |
| Update | Başarılı |
| Delete | Başarılı |

**Doğrulama:** Menüden `2 > User Demo` → `1 founder_1` seç → scope `-1` seç → Listele → **4 kayıt**.

---

### B2. founder_2 (ID=2) — System Root, Guard Bypass

Aynı B1 ile aynı sonuçlar.

---

### B3. individual_demo_user (ID=3) — Bireysel, Public Member

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=read_only |
| Reach | GrantedScopes |
| Grant | SCOPE_ID=2, principal=public_member (131072) |
| CanWrite | HAYIR (public_member ceiling=read_only) |
| PublicOnly | HAYIR (domain principal, public_only değil) |
| Görünen veri | 1/4 (yalnız OWNER_SCOPE_ID=2) |
| Insert | **ENGELLENDİ** (read-only) |
| Update | **ENGELLENDİ** (read-only) |
| Delete | **ENGELLENDİ** (read-only) |

**Doğrulama:** Menüden `2 > User Demo` → `3 individual_demo_user` seç → scope `2` seç → Listele → **1 kayıt** (ID=1, INDIVIDUAL_DEMO_001).

---

### B4. tenant_demo_user_woo (ID=4) — Kurumsal Tekil, Property

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=scope_bounded |
| Reach | GrantedScopes |
| Grant | SCOPE_ID=3, principal=tenant_member (196608) |
| CanWrite | EVET (tenant_member ceiling=scope_bounded) |
| Görünen veri | 1/4 (yalnız OWNER_SCOPE_ID=3) |
| Insert | Başarılı (OWNER_SCOPE_ID=3 olarak eklenir) |
| Update | Başarılı |
| Delete | Başarılı |

**Doğrulama:** Menüden `2 > User Demo` → `4 tenant_demo_user_woo` seç → scope `3` seç → Listele → **1 kayıt** (ID=2, TENANT_WOO_DEMO_001).

---

### B5. tenant_demo_user_wto (ID=5) — Kurumsal Çok Şubeli, Org+Property

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=scope_bounded |
| Reach | GrantedScopes |
| Grantlar | SCOPE_ID=4 (org, tenant_member) + SCOPE_ID=5 (prop, tenant_member) |
| CanWrite | EVET |
| Görünen veri | 2/4 (OWNER_SCOPE_ID=4 ve 5) |
| Descendants | SCOPE_ID=4 → SCOPE_ID=5 (org→property) |
| Active=4 | Working Set={4, 5} (org + property) |
| Active=5 | Working Set={5} (yalnız property) |
| Insert (active=4) | Başarılı (OWNER_SCOPE_ID=4) |
| Insert (active=5) | Başarılı (OWNER_SCOPE_ID=5) |

**Doğrulama:**
1. Menüden `2 > User Demo` → `5 tenant_demo_user_wto` seç
2. Scope `4` seç → Listele → **2 kayıt** (ID=3, ID=4)
3. Scope `5` seç → Listele → **1 kayıt** (ID=4, TENANT_WTO_DEMO_002)

---

### B6. agency_demo_user (ID=6) — Cross-Scope (Aynı Kimlik, Farklı Principal)

| Test | Beklenen |
|------|----------|
| Login | Başarılı, ceiling=scope_bounded (login principal=tenant_member) |
| Grantlar | SCOPE_ID=2 (scope_public, read-only) + SCOPE_ID=4 (scope_root, write) |
| Active=4 | Effective principal=scope_root (-262144), CanWrite=EVET |
| Active=2 | Effective principal=scope_public (-327680), CanWrite=HAYIR |
| Görünen veri (active=4) | 2/4 (OWNER_SCOPE_ID=4, 5) |
| Görünen veri (active=2) | 1/4 (OWNER_SCOPE_ID=2, IS_PUBLIC=1) |
| Insert (active=4) | Başarılı |
| Insert (active=2) | **ENGELLENDİ** (scope_public=read_only) |

**Doğrulama:** Menüden `4 > Stage 4 Cross-Scope Test` otomatik çalışır veya manuel:
1. `2 > User Demo` → `6 agency_demo_user` seç
2. Scope `4` seç → Listele → **2 kayıt**, Insert başarılı
3. Oturumu kapat → tekrar gir → Scope `2` seç → Listele → **1 kayıt**, Insert engellendi

**Kritik gözlem:** Aynı kullanıcı, active scope değiştirince effective principal değişir. Kat1 tavanı login'den değil, matrix'ten gelir.

---

## C. Otomatik Testler (Menü Seçenekleri)

### C1. Stage 3 Self-Test (Menü: `3`)

Bu test otomatik olarak çalışır ve şu doğrulamaları yapar:

| Test | Kullanıcı | Beklenen Görünen |
|------|-----------|-----------------|
| system_root | ID=-1 | 4 (tümü) |
| system_service | ID=-2 | 4 (tümü) |
| system_public | ID=-3 | 2 (yalnız IS_PUBLIC) |
| scope_root | ID=-4 | 0 (root_scope'ta veri yok) |

**Beklenen çıktı:**
```
[PASS] system_root     görünen=4   beklenen=4   canWrite=True
[PASS] system_service  görünen=4   beklenen=4   canWrite=True
[PASS] system_public   görünen=2   beklenen=2   canWrite=False
[PASS] scope_root      görünen=0   beklenen=0   canWrite=True
```

---

### C2. Stage 4 Cross-Scope Test (Menü: `4`)

Bu test `agency_demo_user` (ID=6) ile cross-scope senaryosunu doğrular:

| Active Scope | Effective Principal | CanWrite | Görünen Veri |
|-------------|-------------------|----------|-------------|
| 4 (org) | scope_root (-262144) | EVET | 2 |
| 2 (individual) | scope_public (-327680) | HAYIR | 1 |

**Beklenen çıktı:**
```
active=4: eff=-262144   scope_root   readOnly=False  görünenVeri=2
active=2: eff=-327680   scope_public readOnly=True   görünenVeri=1
→ AYNI kullanıcı: active=4 (scope_root, write) ↔ active=2 (scope_public, read-only).
  Kat1 tavanı login'den DEĞİL, aktif scope'un matrix principal'inden geldi. Cross-scope ✓
```

---

## D. SQL Seviyesi Testler

### D1. UserAccessibleScopes TVF Testi

```sql
-- individual_demo_user → 1 scope (direct)
SELECT * FROM core.UserAccessibleScopes(3);
-- Beklenen: SCOPE_ID=2, PRINCIPAL=131072, SOURCE=direct

-- tenant_demo_user_wto → 2 scope (1 direct + 1 descendant)
SELECT * FROM core.UserAccessibleScopes(5);
-- Beklenen: SCOPE_ID=4 (direct) + SCOPE_ID=5 (descendant, principal=196608 mirası)

-- agency_demo_user → 2 scope (farklı principal'lar)
SELECT * FROM core.UserAccessibleScopes(6);
-- Beklenen: SCOPE_ID=2 (scope_public) + SCOPE_ID=4 (scope_root)
```

### D2. ssp_CheckForLogin Testi

```sql
-- founder_1 ile login
EXEC core.ssp_CheckForLogin @GSM_NO = 905304158501;
-- Result 1: USER_ID=1, PRINCIPAL=-65536, ceiling=unlimited
-- Result 2: SCOPE_ID=-1, principal=-65536

-- individual_demo_user ile login
EXEC core.ssp_CheckForLogin @GSM_NO = 9053000000001;
-- Result 1: USER_ID=3, PRINCIPAL=131072, ceiling=read_only
-- Result 2: SCOPE_ID=2, principal=131072

-- agency_demo_user ile login (cross-scope)
EXEC core.ssp_CheckForLogin @GSM_NO = 9053000000006;
-- Result 1: USER_ID=6, PRINCIPAL=196608
-- Result 2: SCOPE_ID=2 (scope_public) + SCOPE_ID=4 (scope_root)
```

### D3. sp_DemoData_List Filtre Testi

```sql
-- Guard bypass (NULL) → tümünü getir
EXEC dbo.sp_DemoData_List @WORKING_SCOPE_IDS = NULL, @PUBLIC_ONLY = 0;
-- Beklenen: 4 kayıt

-- Public only (system_public)
EXEC dbo.sp_DemoData_List @WORKING_SCOPE_IDS = NULL, @PUBLIC_ONLY = 1;
-- Beklenen: 2 kayıt (IS_PUBLIC=1)

-- Scope filtresi (tenant_demo_user_wto, active=4)
EXEC dbo.sp_DemoData_List @WORKING_SCOPE_IDS = '4,5', @PUBLIC_ONLY = 0;
-- Beklenen: 2 kayıt (OWNER_SCOPE_ID=4 ve 5)

-- Scope filtresi (individual_demo_user, active=2)
EXEC dbo.sp_DemoData_List @WORKING_SCOPE_IDS = '2', @PUBLIC_ONLY = 0;
-- Beklenen: 1 kayıt (OWNER_SCOPE_ID=2)
```

---

## E. Sınır Durumları (Edge Cases)

### E1. Boş Working Set

Kullanıcının hiç grant'i yoksa → `working_set = ∅` →hiçbir veri görünmez.

```sql
-- Hiç grant'i olmayan hayali kullanıcı
EXEC dbo.sp_DemoData_List @WORKING_SCOPE_IDS = '-2147483648', @PUBLIC_ONLY = 0;
-- Beklenen: 0 kayıt (sentinel değer, eşleşme yok)
```

### E2. Geçersiz GSM ile Login

```sql
EXEC core.ssp_CheckForLogin @GSM_NO = 0;
-- Beklenen: RETURN_CODE=404, RETURN_MESSAGE='User not found.'
```

### E3. Soft Delete Sonrası

Kayıt silindikten sonra (DELETED=1) List'te görünmez:
```sql
-- INSERT yap, sil, listele
EXEC dbo.sp_DemoData_Insert @PLACE_HOLDER=N'TEST', @STATUS=1, @LCID=1033, @DEFAULT_LCID=1033, @ACTOR_USER_ID=1, @ACTOR_SCOPE_ID=-1;
DECLARE @newId INT = SCOPE_IDENTITY();
EXEC dbo.sp_DemoData_Delete @DEMODATA_ID=@newId, @ACTOR_USER_ID=1, @ACTOR_SCOPE_ID=-1;
EXEC dbo.sp_DemoData_List @WORKING_SCOPE_IDS=NULL, @PUBLIC_ONLY=0;
-- @newId artık görünmüyor
```

### E4. Anti-Escalation Invariant

Her test senaryosunda doğrulanmalı:
```
working_set ⊆ accessible_scopes = True
```
Consol'da bu değer otomatik gösterilir.

### E5. Identity Insert (System Users)

System users IDENTITY ile eklenir, `IDENTITY_INSERT ON` gerekir:
```sql
SET IDENTITY_INSERT core.tblUser ON;
INSERT core.tblUser (USER_ID, ...) VALUES (-99, ...);
SET IDENTITY_INSERT core.tblUser OFF;
```

---

## F. Beklenen Test Senaryoları Tablosu

| # | Senaryo | Kullanıcı | Active Scope | Görünen | Write | Public Only |
|---|---------|-----------|-------------|---------|-------|-------------|
| 1 | System root tam erişim | system_root (-1) | -1 | 4 | EVET | HAYIR |
| 2 | System service tüm scope | system_service (-2) | -1 | 4 | EVET | HAYIR |
| 3 | System public yalnız public | system_public (-3) | -1 | 2 | HAYIR | EVET |
| 4 | Scope root boş scope | scope_root (-4) | -1 | 0 | EVET | HAYIR |
| 5 | Bireysel tek scope | individual_demo (3) | 2 | 1 | HAYIR | HAYIR |
| 6 | Kurumsal tekil property | tenant_woo (4) | 3 | 1 | EVET | HAYIR |
| 7 | Kurumsal çok şubeli org | tenant_wto (5) | 4 | 2 | EVET | HAYIR |
| 8 | Kurumsal çok şubeli prop | tenant_wto (5) | 5 | 1 | EVET | HAYIR |
| 9 | Cross-scope write | agency (6) | 4 | 2 | EVET | HAYIR |
| 10 | Cross-scope read-only | agency (6) | 2 | 1 | HAYIR | HAYIR |
| 11 | Founder guard bypass | founder_1 (1) | -1 | 4 | EVET | HAYIR |

---

## G. Otomatik Test Çıktı Formatı

`dotnet run` → `3 > Stage 3 Self-Test` seçildiğinde:

```
─── STAGE 3: ISystemPrinciple Self-Test ───

  Hardcoded sistem principal tavanları:
    system_root    id=-65536   reach=AllScopes  bypass=True  write=True  publicOnly=False
    system_service id=-131072  reach=AllScopes  bypass=False write=True  publicOnly=False
    system_public  id=-196608  reach=AllScopes  bypass=False write=False publicOnly=True
    scope_root     id=-262144  reach=GrantedScopes bypass=False write=True  publicOnly=False
    scope_public   id=-327680  reach=GrantedScopes bypass=False write=False publicOnly=True

  Referans: toplam demo=4, IS_PUBLIC=1 olan=2

  Reach + ceiling testleri:
    [PASS] system_root     görünen=4   beklenen=4   canWrite=True
    [PASS] system_service  görünen=4   beklenen=4   canWrite=True
    [PASS] system_public   görünen=2   beklenen=2   canWrite=False
    [PASS] scope_root      görünen=0   beklenen=0   canWrite=True
```

---

## H. Hızlı Referans — Principal Karar Matrisi

| Principal | Reach | Guard Bypass | Write | Public Only | Erişim |
|-----------|-------|-------------|-------|-------------|--------|
| system_root | AllScopes | EVET | EVET | HAYIR | Tüm scope, tüm veri, tüm işlem |
| system_service | AllScopes | HAYIR | EVET | HAYIR | Tüm scope, tüm veri, yazma var |
| system_public | AllScopes | HAYIR | HAYIR | EVET | Tüm scope, yalnız IS_PUBLIC, okuma |
| scope_root | GrantedScopes | HAYIR | EVET | HAYIR | Grant'li scope, tüm veri, yazma |
| scope_public | GrantedScopes | HAYIR | HAYIR | EVET | Grant'li scope, yalnız IS_PUBLIC |
| public_member | GrantedScopes | HAYIR | HAYIR | HAYIR | Grant'li scope, tüm veri, okuma |
| tenant_member | GrantedScopes | HAYIR | EVET | HAYIR | Grant'li scope, tüm veri, yazma |
