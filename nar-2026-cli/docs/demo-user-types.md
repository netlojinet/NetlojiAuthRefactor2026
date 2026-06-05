# Demo User Türleri — MVP Auth Refactor

## Genel Bakış

Bu doküman, Netloji Auth Refactor 2026 MVP'sindeki demo kullanıcı türlerini ve
kullanım amaçlarını tanımlar. Her tür, farklı bir erişim modelini temsil eder.

---

## 1. individual_demo_user — Bireysel Kullanıcı

| Özellik | Değer |
|---------|-------|
| USER_ID | 3 (otomatik) |
| PRINCIPAL_TYPE_ID | 131072 (public_member) |
| TIER_ID | 2 |
| SCOPE_ID | 2 |
| SCOPE_TYPE | 0 (individual) |

**Erişim Modeli:** Doğrudan user ↔ scope ilişkisi. Organizationsuz, propsuz,
tekil bireysel kullanıcı. En basit erişim kalıbı.

**Kullanım Senaryosu:** Bireysel abonelik, kişisel hesap, bağımsız kullanıcı.

```
User (ID=3) ←→ Scope (ID=2, type=individual)
```

---

## 2. tenant_demo_user_woo — Kurumsal Tekil (With Only Organization)

| Özellik | Değer |
|---------|-------|
| USER_ID | 4 (otomatik) |
| PRINCIPAL_TYPE_ID | 196608 (tenant_member) |
| TIER_ID | 3 |
| SCOPE_ID | 3 |
| SCOPE_TYPE | 1 (property) |

**Erişim Modeli:** Doğrudan property ↔ scope ilişkisi. Organizationa bağlı
değil, tekil mülk/tesis düzeyinde erişim. "WOO" = With Only Organization
(değil) → doğrudan property.

**Kullanım Senaryosu:** Tek mülk sahibi, bağımsız tesis, tek şubeli işletme.

```
User (ID=4) ←→ Property (ID=2, SCOPE_ID=3)
                  └── (org bağımlılığı yok)
```

---

## 3. tenant_demo_user_wto — Kurumsal Çok Şubeli/Zincir

| Özellik | Değer |
|---------|-------|
| USER_ID | 5 (otomatik) |
| PRINCIPAL_TYPE_ID | 196608 (tenant_member) |
| TIER_ID | 3 |
| SCOPE_ID | 4 (organization) + 5 (property) |
| SCOPE_TYPE | 2 (organization) + 1 (property) |

**Erişim Modeli:** Organization ↔ scope + property ↔ scope hiyerarşisi.
Organizationa bağlı property'ler zinciri. "WTO" = With To (organizasyona bağlı).

**Kullanım Senaryosu:** Zincir oteller, çok şubeli mağazalar, franchise yapısı.

```
User (ID=5) ←→ Organization (ID=1, SCOPE_ID=4)
                  └── Property (ID=1, SCOPE_ID=5)
                        └── OWNER_ORGANIZATION_ID = 1
```

---

## Karşılaştırma Tablosu

| User | Tür | Scope Type | Hiyerarşi | Senaryo |
|------|-----|------------|-----------|---------|
| individual_demo_user | Bireysel | individual (0) | user ↔ scope | Kişisel hesap |
| tenant_demo_user_woo | Kurumsal Tekil | property (1) | prop ↔ scope | Tek mülk |
| tenant_demo_user_wto | Kurumsal Çok Şubeli | org (2) + prop (1) | org ↔ scope + prop ↔ scope | Zincir/şube |

---

## Audit Kayıt Kuralları

Her demo data kaydında audit alanları aşağıdaki gibi doldurulur:

| Alan | Değer | Açıklama |
|------|-------|----------|
| CREATOR_USER_ID | Kaydı oluşturan user | Demo user'ın kendi ID'si |
| CREATOR_SCOPE_ID | Oluşturma scope'u | Kullanıcının aktif scope'u |
| EDITOR_USER_ID | Son düzenleyen | İlk oluşturmada CREATOR ile aynı |
| EDITOR_SCOPE_ID | Düzenleme scope'u | İlk oluşturmada CREATOR_SCOPE ile aynı |
| OWNER_SCOPE_ID | Kaydın gerçek sahibi | Verinin ait olduğu scope |

---

## Scope Hiyerarşisi

```
SCOPE_ID=1 (root_scope, type=255) — system reserved
├── SCOPE_ID=2 (individual, type=0) — individual_demo_user
├── SCOPE_ID=3 (property, type=1) — tenant_demo_user_woo
├── SCOPE_ID=4 (organization, type=2) — tenant_demo_user_wto (org)
│   └── SCOPE_ID=5 (property, type=1) — tenant_demo_user_wto (prop)
```
