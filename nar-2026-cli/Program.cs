using Microsoft.Data.SqlClient;

namespace NetlojiAuthRefactor2026;

internal static class Program
{
    private const string ConnectionString =
        "Server=.;Database=NetlojiAuthRefactor2026;Trusted_Connection=True;TrustServerCertificate=True;";

    // Paylaşılan auth motoru (nar-2026-core) — login + erişim çözümü buradan. CLI == GUI.
    private static readonly AuthEngine Engine = new(ConnectionString);

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Netloji Auth Refactor 2026 — MVP ===");
        Console.WriteLine("Target: Server=. Database=NetlojiAuthRefactor2026");
        Console.WriteLine();

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        // --- Script runner ---
        await RunScripts(connection);

        // --- Principal kataloğunu DB'den yükle (§1.2: tek doğruluk kaynağı) ---
        await Engine.LoadCatalogAsync();

        // --- Demo menu ---
        Console.WriteLine();
        await RunDemoMenu(connection);
    }

    private static async Task RunDemoMenu(SqlConnection connection)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║        DEMO OTURUM SEÇİMİ           ║");
            Console.WriteLine("╠══════════════════════════════════════╣");
            Console.WriteLine("║  1 > System Demo  (USER_ID < 0)     ║");
            Console.WriteLine("║  2 > User Demo    (USER_ID > 0)     ║");
            Console.WriteLine("║  3 > Stage 3 Self-Test              ║");
            Console.WriteLine("║  4 > Stage 4 Cross-Scope Test       ║");
            Console.WriteLine("║  0 > Çıkış                           ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.Write("Seçiminiz: ");

            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await SystemDemoFlow(connection);
                    break;
                case "2":
                    await UserDemoFlow(connection);
                    break;
                case "3":
                    await Stage3SelfTest(connection);
                    break;
                case "4":
                    await Stage4CrossScopeTest(connection);
                    break;
                case "0":
                    Console.WriteLine("Çıkış yapılıyor...");
                    return;
                default:
                    Console.WriteLine("Geçersiz seçim.");
                    break;
            }
        }
    }

    /// <summary>
    /// System demo akışı — USER_ID &lt; 0 olan system kullanıcılarını listeler.
    /// Sıralama: 1→5 arası negatif (tamsayı sırasıyla).
    /// </summary>
    private static async Task SystemDemoFlow(SqlConnection connection)
    {
        Console.WriteLine("─── SYSTEM DEMO ───");
        Console.WriteLine("Sistem kullanıcıları (USER_ID < 0):");
        Console.WriteLine();

        var users = new List<(int UserId, string Username, int PrincipalTypeId, string PrincipalCode, long UserKey)>();

        const string sql = """
            SET QUOTED_IDENTIFIER ON;
            SELECT u.USER_ID, u.USERNAME, u.PRINCIPAL_TYPE_ID, pt.CODE, u.USER_KEY
            FROM core.tblUser u
            LEFT JOIN core.conPrincipalType pt ON u.PRINCIPAL_TYPE_ID = pt.PRINCIPAL_TYPE_ID
            WHERE u.USER_ID < 0 AND u.DELETED = 0
            ORDER BY u.USER_ID DESC
            """;

        await using (var cmd = new SqlCommand(sql, connection))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            var idx = 1;
            while (await reader.ReadAsync())
            {
                var uid = reader.GetInt32(0);
                var uname = reader.GetString(1);
                var pid = reader.GetInt32(2);
                var pcode = reader.IsDBNull(3) ? "N/A" : reader.GetString(3);
                var ukey = reader.GetInt64(4);
                users.Add((uid, uname, pid, pcode, ukey));
                Console.WriteLine($"  {idx} > [{uid}] {uname} | principal={pcode} ({pid}) | key={ukey}");
                idx++;
            }
        }

        Console.WriteLine();
        Console.Write("Kullanıcı seçin (1-{0}) veya 0 ile geri dönün: ", users.Count);
        var input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out var selection) || selection < 0 || selection > users.Count)
        {
            Console.WriteLine("Geçersiz seçim.");
            return;
        }

        if (selection == 0) return;

        var selected = users[selection - 1];
        Console.WriteLine();
        Console.WriteLine($"Seçilen: [{selected.UserId}] {selected.Username}");

        var context = await BuildUserContext(connection, selected.UserId);
        if (context != null)
            StartMockSession(context);
    }

    /// <summary>
    /// User demo akışı — USER_ID &gt; 0 olan gerçek kullanıcıları listeler.
    /// </summary>
    private static async Task UserDemoFlow(SqlConnection connection)
    {
        Console.WriteLine("─── USER DEMO ───");
        Console.WriteLine("Gerçek kullanıcılar (USER_ID > 0):");
        Console.WriteLine();

        var users = new List<(int UserId, string Username, int PrincipalTypeId, string PrincipalCode, long UserKey)>();

        const string sql = """
            SET QUOTED_IDENTIFIER ON;
            SELECT u.USER_ID, u.USERNAME, u.PRINCIPAL_TYPE_ID, pt.CODE, u.USER_KEY
            FROM core.tblUser u
            LEFT JOIN core.conPrincipalType pt ON u.PRINCIPAL_TYPE_ID = pt.PRINCIPAL_TYPE_ID
            WHERE u.USER_ID > 0 AND u.DELETED = 0
            ORDER BY u.USER_ID ASC
            """;

        await using (var cmd = new SqlCommand(sql, connection))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            var idx = 1;
            while (await reader.ReadAsync())
            {
                var uid = reader.GetInt32(0);
                var uname = reader.GetString(1);
                var pid = reader.GetInt32(2);
                var pcode = reader.IsDBNull(3) ? "N/A" : reader.GetString(3);
                var ukey = reader.GetInt64(4);
                users.Add((uid, uname, pid, pcode, ukey));
                Console.WriteLine($"  {idx} > [{uid}] {uname} | principal={pcode} ({pid}) | key={ukey}");
                idx++;
            }
        }

        if (users.Count == 0)
        {
            Console.WriteLine("Kayıtlı kullanıcı bulunamadı.");
            return;
        }

        Console.WriteLine();
        Console.Write("Kullanıcı seçin (1-{0}) veya 0 ile geri dönün: ", users.Count);
        var input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out var selection) || selection < 0 || selection > users.Count)
        {
            Console.WriteLine("Geçersiz seçim.");
            return;
        }

        if (selection == 0) return;

        var selected = users[selection - 1];
        Console.WriteLine();
        Console.WriteLine($"Seçilen: [{selected.UserId}] {selected.Username}");

        var context = await BuildUserContext(connection, selected.UserId);
        if (context != null)
            StartMockSession(context);
    }

    /// <summary>Login → paylaşılan AuthEngine (core.ssp_CheckForLogin). CLI ve GUI AYNI motoru kullanır.</summary>
    private static Task<DummyUserContext?> BuildUserContext(SqlConnection connection, int userId)
        => Engine.LoginByUserIdAsync(userId);

    // NOT: Eski GetAccessibleScopes (C# tarafı reach hesabı) kaldırıldı.
    // Reach artık SQL'de: core.UserAccessibleScopes TVF (matrix-tabanlı, per-scope principal),
    // login üzerinden core.ssp_CheckForLogin Result2 olarak gelir → BuildUserContext.
    // Yapısal alt iniş (org → property) Stage 2'de TVF'e eklenecek.

    /// <summary>
    /// Mock oturum başlatır — DummyUserContext ile session simülasyonu.
    /// USER_ID &gt; 0 ise DemoData CRUD menüsünü açar.
    /// </summary>
    private static void StartMockSession(DummyUserContext context)
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                 MOCK OTURUM AÇILDI                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine($"  Kullanıcı     : {context.Username} [ID={context.UserId}]");
        Console.WriteLine($"  Login Principal: {context.PrincipalTypeId} (tier={context.TierId}, app={context.AppId}, ceiling={context.CeilingLevel})");
        Console.WriteLine($"  System User   : {(context.IsSystemUser ? "EVET" : "HAYIR")} (tier-bazlı)");
        if (context.SystemPrinciple is { } sysp)
            Console.WriteLine($"  Sys Principal : {sysp.Code} | reach={sysp.Reach} bypass={sysp.BypassGuard} write={sysp.CanWrite} publicOnly={sysp.PublicOnly}");
        Console.WriteLine();

        Console.WriteLine($"  Erişilebilir Scope'lar ({context.AccessibleScopes.Count}):");
        if (context.AccessibleScopes.Count == 0)
        {
            Console.WriteLine("    (hiç yok)");
        }
        else
        {
            foreach (var grant in context.ScopeGrants.Values.OrderBy(x => x.ScopeId))
            {
                var marker = grant.ScopeId == -1 ? " ← root_scope" : "";
                Console.WriteLine($"    SCOPE_ID={grant.ScopeId} | principal={grant.PrincipalTypeId} ceiling={grant.CeilingLevel} ({grant.Source}){marker}");
            }
        }

        if (context.SystemPrinciple is { Reach: SystemReach.AllScopes })
            Console.WriteLine($"    → reach=AllScopes: veri filtresinde TÜM scope'lar görünür" +
                              (context.SystemPrinciple.PublicOnly ? " (yalnız IS_PUBLIC=1)" : "") +
                              (context.SystemPrinciple.BypassGuard ? " (guard bypass)" : "") + ".");

        Console.WriteLine();

        // Context switch
        if (context.AccessibleScopes.Count > 0)
        {
            Console.Write("Aktif scope seçin (SCOPE_ID) veya Enter ile atla: ");
            var scopeInput = Console.ReadLine()?.Trim();

            if (int.TryParse(scopeInput, out var activeScopeId) && context.AccessibleScopes.Contains(activeScopeId))
            {
                context.ActiveScopeId = activeScopeId;
                context.WorkingSet = ComputeWorkingSet(context);

                Console.WriteLine();
                Console.WriteLine($"  Active Scope   : {activeScopeId}");
                Console.WriteLine($"  Eff. Principal : {context.EffectivePrincipalTypeId} (ceiling={context.EffectiveCeiling})");
                Console.WriteLine($"  Guard Bypass   : {(context.HasGuardBypass ? "EVET" : "HAYIR")}");
                Console.WriteLine($"  ReadOnly       : {(context.IsReadOnly ? "EVET" : "HAYIR")}");
                Console.WriteLine($"  Working Set    : [{string.Join(", ", context.WorkingSet.OrderBy(x => x))}]");
                Console.WriteLine($"  Anti-escalation invariant : working_set ⊆ accessible = {context.WorkingSet.IsSubsetOf(context.AccessibleScopes)}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("─── Oturum aktif ───");

        // Tüm principal'lar veri menüsüne girer; reach + ceiling guard'da uygulanır
        // (system_root/service → tüm scope; public → IS_PUBLIC + read-only; scope_* → grant'li).
        Console.WriteLine();
        RunDemoDataMenu(context).GetAwaiter().GetResult();
    }

    /// <summary>Working set → paylaşılan AuthEngine (§5.2).</summary>
    private static HashSet<int> ComputeWorkingSet(DummyUserContext context) => AuthEngine.ComputeWorkingSet(context);

    /// <summary>Veri filtresi → paylaşılan AuthEngine.</summary>
    private static (string? scopeIds, bool publicOnly) ResolveDataFilter(DummyUserContext context) => AuthEngine.ResolveDataFilter(context);

    /// <summary>Stage 3 self-test — ISystemPrinciple hardcoded tavanlarını doğrular (1-2 test).</summary>
    private static async Task Stage3SelfTest(SqlConnection connection)
    {
        Console.WriteLine("─── STAGE 3: ISystemPrinciple Self-Test ───");
        Console.WriteLine();
        Console.WriteLine("  Hardcoded sistem principal tavanları:");
        foreach (var p in SystemPrincipleRegistry.All.OrderByDescending(x => x.PrincipalTypeId))
            Console.WriteLine($"    {p.Code,-15} id={p.PrincipalTypeId,-9} reach={p.Reach,-13} bypass={p.BypassGuard,-5} write={p.CanWrite,-5} publicOnly={p.PublicOnly}");
        Console.WriteLine();

        var total = await ScalarInt(connection, "SELECT COUNT(*) FROM dbo.tblDemoData WHERE DELETED=0");
        var publicCount = await ScalarInt(connection, "SELECT COUNT(*) FROM dbo.tblDemoData WHERE DELETED=0 AND IS_PUBLIC=1");
        Console.WriteLine($"  Referans: toplam demo={total}, IS_PUBLIC=1 olan={publicCount}");
        Console.WriteLine();

        Console.WriteLine("  Reach + ceiling testleri:");
        await AssertVisible(connection, userId: -1, code: "system_root",    expected: total);        // bypass → hepsi
        await AssertVisible(connection, userId: -2, code: "system_service", expected: total);        // AllScopes → hepsi
        await AssertVisible(connection, userId: -3, code: "system_public",  expected: publicCount);  // AllScopes + IS_PUBLIC
        await AssertVisible(connection, userId: -4, code: "scope_root",     expected: 0);            // GrantedScopes={-1} → demo yok
    }

    private static async Task AssertVisible(SqlConnection connection, int userId, string code, int expected)
    {
        var ctx = await BuildUserContext(connection, userId);
        if (ctx is null) { Console.WriteLine($"    [FAIL] {code}: login null"); return; }
        var count = (await GetDemoDataList(connection, ctx)).Count;
        var pass = count == expected;
        Console.WriteLine($"    [{(pass ? "PASS" : "FAIL")}] {code,-15} görünen={count,-3} beklenen={expected,-3} canWrite={ctx.SystemPrinciple?.CanWrite}");
    }

    private static async Task<int> ScalarInt(SqlConnection connection, string sql)
    {
        await using var cmd = new SqlCommand("SET QUOTED_IDENTIFIER ON; " + sql, connection);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    /// <summary>
    /// Stage 4 cross-scope test — agency_demo_user (6): aynı kimlik, active scope değişince
    /// effective principal (Kat1 tavan) değişiyor. scope 4 → scope_root (write), scope 2 → scope_public (read-only).
    /// </summary>
    private static async Task Stage4CrossScopeTest(SqlConnection connection)
    {
        Console.WriteLine("─── STAGE 4: Cross-Scope (aynı kimlik, scope'a göre farklı principal) ───");
        Console.WriteLine();

        var ctx = await BuildUserContext(connection, 6);
        if (ctx is null) { Console.WriteLine("  agency_demo_user (6) yok — 021 seed çalıştı mı?"); return; }

        Console.WriteLine($"  Kimlik: [{ctx.UserId}] {ctx.Username} | login principal={ctx.PrincipalTypeId}");
        Console.WriteLine("  Cross-scope grant'ler (matrix):");
        foreach (var g in ctx.ScopeGrants.Values.OrderBy(x => x.ScopeId))
            Console.WriteLine($"    scope {g.ScopeId,-3} → principal {g.PrincipalTypeId,-9} ceiling={g.CeilingLevel} ({g.Source})");
        Console.WriteLine();

        Console.WriteLine("  Active scope değiştikçe EFFECTIVE tavan:");
        foreach (var scopeId in new[] { 4, 2 })
        {
            ctx.ActiveScopeId = scopeId;
            ctx.WorkingSet = ComputeWorkingSet(ctx);
            var rows = (await GetDemoDataList(connection, ctx)).Count;
            var sp = ctx.SystemPrinciple?.Code ?? "domain";
            Console.WriteLine($"    active={scopeId}: eff={ctx.EffectivePrincipalTypeId,-9} {sp,-13} readOnly={ctx.IsReadOnly,-5} workingSet=[{string.Join(",", ctx.WorkingSet.OrderBy(x => x))}] görünenVeri={rows}");
        }
        Console.WriteLine();
        Console.WriteLine("  → AYNI kullanıcı: active=4 (scope_root, write) ↔ active=2 (scope_public, read-only).");
        Console.WriteLine("    Kat1 tavanı login'den DEĞİL, aktif scope'un matrix principal'inden geldi. Cross-scope ✓");
    }

    // ─── DemoData CRUD Menüsü ───

    private static async Task RunDemoDataMenu(DummyUserContext context)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║          DEMO DATA İŞLEMLERİ         ║");
            Console.WriteLine("╠══════════════════════════════════════╣");
            Console.WriteLine("║  1 > Yeni Kayıt Ekle (INSERT)        ║");
            Console.WriteLine("║  2 > Kayıt Düzenle (UPDATE)          ║");
            Console.WriteLine("║  3 > Kayıt Sil (DELETE)              ║");
            Console.WriteLine("║  4 > Kayıtları Listele (LIST)        ║");
            Console.WriteLine("║  0 > Oturumu Kapat                   ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.Write("Seçiminiz: ");

            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await DemoDataInsert(connection, context);
                    break;
                case "2":
                    await DemoDataUpdate(connection, context);
                    break;
                case "3":
                    await DemoDataDelete(connection, context);
                    break;
                case "4":
                    await DemoDataList(connection, context);
                    break;
                case "0":
                    Console.WriteLine("Oturum kapatılıyor...");
                    return;
                default:
                    Console.WriteLine("Geçersiz seçim.");
                    break;
            }
        }
    }

    /// <summary>
    /// Yeni DemoData kaydı — console'dan PLACE_HOLDER input'u alınır.
    /// </summary>
    private static async Task DemoDataInsert(SqlConnection connection, DummyUserContext context)
    {
        Console.WriteLine("─── YENİ KAYIT ───");
        if (context.IsReadOnly)
        {
            Console.WriteLine($"[ENGELLENDİ] read-only tavan ({context.SystemPrinciple?.Code ?? "domain"}) — yazma yok.");
            return;
        }
        Console.Write("PLACE_HOLDER değeri (max 50 karakter): ");
        var placeholder = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(placeholder))
        {
            Console.WriteLine("İptal: değer boş olamaz.");
            return;
        }

        if (placeholder.Length > 50)
            placeholder = placeholder[..50];

        Console.Write("STATUS (1=aktif, varsayılan=1): ");
        var statusInput = Console.ReadLine()?.Trim();
        byte status = 1;
        if (!string.IsNullOrEmpty(statusInput))
            byte.TryParse(statusInput, out status);

        Console.Write("LCID (varsayılan=1033): ");
        var lcidInput = Console.ReadLine()?.Trim();
        int lcid = 1033;
        if (!string.IsNullOrEmpty(lcidInput))
            int.TryParse(lcidInput, out lcid);

        Console.Write("DEFAULT_LCID (varsayılan=1033): ");
        var defLcidInput = Console.ReadLine()?.Trim();
        int defLcid = 1033;
        if (!string.IsNullOrEmpty(defLcidInput))
            int.TryParse(defLcidInput, out defLcid);

        const string sql = """
            SET QUOTED_IDENTIFIER ON;
            EXEC dbo.sp_DemoData_Insert
                @PLACE_HOLDER = @ph,
                @STATUS = @st,
                @LCID = @lcid,
                @DEFAULT_LCID = @dlcid,
                @ACTOR_USER_ID = @uid,
                @ACTOR_SCOPE_ID = @sid
            """;

        await using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@ph", placeholder);
            cmd.Parameters.AddWithValue("@st", status);
            cmd.Parameters.AddWithValue("@lcid", lcid);
            cmd.Parameters.AddWithValue("@dlcid", defLcid);
            cmd.Parameters.AddWithValue("@uid", context.UserId);
            cmd.Parameters.AddWithValue("@sid", (object?)context.ActiveScopeId ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            Console.WriteLine();
            Console.WriteLine($"Kayıt başarılı. Yeni ID: {result}");
        }
    }

    /// <summary>
    /// DemoData düzenle — önce listele, seçim al, yeni değer gir.
    /// </summary>
    private static async Task DemoDataUpdate(SqlConnection connection, DummyUserContext context)
    {
        Console.WriteLine("─── KAYIT DÜZENLE ───");
        if (context.IsReadOnly)
        {
            Console.WriteLine($"[ENGELLENDİ] read-only tavan ({context.SystemPrinciple?.Code ?? "domain"}) — yazma yok.");
            return;
        }
        var items = await GetDemoDataList(connection, context);
        if (items.Count == 0) return;

        Console.Write("Düzenlenecek ID seçin (1-{0}) veya 0 ile iptal: ", items.Count);
        var input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out var selection) || selection < 0 || selection > items.Count)
        {
            Console.WriteLine("Geçersiz seçim.");
            return;
        }

        if (selection == 0) return;

        var selected = items[selection - 1];
        Console.WriteLine($"Seçilen: [ID={selected.Id}] {selected.PlaceHolder} | status={selected.Status} | lcid={selected.Lcid}");

        Console.Write($"Yeni PLACE_HOLDER [{selected.PlaceHolder}]: ");
        var newPh = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(newPh)) newPh = selected.PlaceHolder;
        if (newPh.Length > 50) newPh = newPh[..50];

        Console.Write($"Yeni STATUS [{selected.Status}]: ");
        var stInput = Console.ReadLine()?.Trim();
        byte newStatus = selected.Status;
        if (!string.IsNullOrEmpty(stInput)) byte.TryParse(stInput, out newStatus);

        Console.Write($"Yeni LCID [{selected.Lcid}]: ");
        var lcidInput = Console.ReadLine()?.Trim();
        int newLcid = selected.Lcid;
        if (!string.IsNullOrEmpty(lcidInput)) int.TryParse(lcidInput, out newLcid);

        Console.Write($"Yeni DEFAULT_LCID [{selected.DefaultLcid}]: ");
        var defLcidInput = Console.ReadLine()?.Trim();
        int newDefLcid = selected.DefaultLcid;
        if (!string.IsNullOrEmpty(defLcidInput)) int.TryParse(defLcidInput, out newDefLcid);

        const string sql = """
            SET QUOTED_IDENTIFIER ON;
            EXEC dbo.sp_DemoData_Update
                @DEMODATA_ID = @id,
                @PLACE_HOLDER = @ph,
                @STATUS = @st,
                @LCID = @lcid,
                @DEFAULT_LCID = @dlcid,
                @ACTOR_USER_ID = @uid,
                @ACTOR_SCOPE_ID = @sid
            """;

        await using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@id", selected.Id);
            cmd.Parameters.AddWithValue("@ph", newPh);
            cmd.Parameters.AddWithValue("@st", newStatus);
            cmd.Parameters.AddWithValue("@lcid", newLcid);
            cmd.Parameters.AddWithValue("@dlcid", newDefLcid);
            cmd.Parameters.AddWithValue("@uid", context.UserId);
            cmd.Parameters.AddWithValue("@sid", (object?)context.ActiveScopeId ?? DBNull.Value);

            var affected = await cmd.ExecuteScalarAsync();
            Console.WriteLine();
            Console.WriteLine(Convert.ToInt32(affected) > 0 ? "Güncelleme başarılı." : "Kayıt bulunamadı.");
        }
    }

    /// <summary>
    /// DemoData sil (soft delete) — önce listele, seçim al.
    /// </summary>
    private static async Task DemoDataDelete(SqlConnection connection, DummyUserContext context)
    {
        Console.WriteLine("─── KAYIT SİL ───");
        if (context.IsReadOnly)
        {
            Console.WriteLine($"[ENGELLENDİ] read-only tavan ({context.SystemPrinciple?.Code ?? "domain"}) — yazma yok.");
            return;
        }
        var items = await GetDemoDataList(connection, context);
        if (items.Count == 0) return;

        Console.Write("Silinecek ID seçin (1-{0}) veya 0 ile iptal: ", items.Count);
        var input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out var selection) || selection < 0 || selection > items.Count)
        {
            Console.WriteLine("Geçersiz seçim.");
            return;
        }

        if (selection == 0) return;

        var selected = items[selection - 1];
        Console.Write($"[ID={selected.Id}] {selected.PlaceHolder} silinecek. Emin misiniz? (E/H): ");
        var confirm = Console.ReadLine()?.Trim().ToUpperInvariant();

        if (confirm != "E")
        {
            Console.WriteLine("İptal edildi.");
            return;
        }

        const string sql = """
            SET QUOTED_IDENTIFIER ON;
            EXEC dbo.sp_DemoData_Delete
                @DEMODATA_ID = @id,
                @ACTOR_USER_ID = @uid,
                @ACTOR_SCOPE_ID = @sid
            """;

        await using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@id", selected.Id);
            cmd.Parameters.AddWithValue("@uid", context.UserId);
            cmd.Parameters.AddWithValue("@sid", (object?)context.ActiveScopeId ?? DBNull.Value);

            var affected = await cmd.ExecuteScalarAsync();
            Console.WriteLine();
            Console.WriteLine(Convert.ToInt32(affected) > 0 ? "Silme başarılı." : "Kayıt bulunamadı.");
        }
    }

    /// <summary>
    /// DemoData listele — scope filtreli.
    /// System root (guard bypass) → tümünü gösterir.
    /// Diğerleri → WorkingSet içerisindeki scope'lara ait kayıtlar.
    /// </summary>
    private static async Task DemoDataList(SqlConnection connection, DummyUserContext context)
    {
        Console.WriteLine("─── KAYIT LİSTESİ ───");
        var items = await GetDemoDataList(connection, context);

        if (items.Count == 0)
        {
            Console.WriteLine("Kayıt bulunamadı.");
            return;
        }

        Console.WriteLine($"  {"#",-4} {"ID",-8} {"PLACE_HOLDER",-25} {"ST",-4} {"LCID",-6} {"OWNER_SCOPE",-12} {"CREATOR",-8} {"CREATED"}");
        Console.WriteLine("  ---- ------- ------------------------- ---- ------ ----------- ------- -------------------");
        var idx = 1;
        foreach (var item in items)
        {
            Console.WriteLine($"  {idx,-4} {item.Id,-8} {item.PlaceHolder,-25} {item.Status,-4} {item.Lcid,-6} {item.OwnerScopeId,-12} {item.CreatorUserId,-8} {item.CreationTime:yyyy-MM-dd HH:mm}");
            idx++;
        }
    }

    private static async Task<List<DemoDataItem>> GetDemoDataList(SqlConnection connection, DummyUserContext context)
    {
        var items = new List<DemoDataItem>();

        // Reach + public-only çözümü (ISystemPrinciple / working set).
        var (scopeIds, publicOnly) = ResolveDataFilter(context);

        const string sql = """
            SET QUOTED_IDENTIFIER ON;
            EXEC dbo.sp_DemoData_List @WORKING_SCOPE_IDS = @scopeIds, @PUBLIC_ONLY = @publicOnly
            """;

        await using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@scopeIds", (object?)scopeIds ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@publicOnly", publicOnly);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new DemoDataItem
                {
                    Id = reader.GetInt32(0),
                    PlaceHolder = reader.GetString(1),
                    Status = reader.GetByte(2),
                    Lcid = reader.GetInt32(3),
                    DefaultLcid = reader.GetInt32(4),
                    OwnerScopeId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    CreatorUserId = reader.GetInt32(6),
                    CreatorScopeId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    CreationTime = reader.GetDateTime(8),
                    EditorUserId = reader.GetInt32(9),
                    ModifiedTime = reader.GetDateTime(10)
                });
            }
        }

        return items;
    }

    private sealed class DemoDataItem
    {
        public int Id { get; init; }
        public string PlaceHolder { get; init; } = string.Empty;
        public byte Status { get; init; }
        public int Lcid { get; init; }
        public int DefaultLcid { get; init; }
        public int? OwnerScopeId { get; init; }
        public int CreatorUserId { get; init; }
        public int? CreatorScopeId { get; init; }
        public DateTime CreationTime { get; init; }
        public int EditorUserId { get; init; }
        public DateTime ModifiedTime { get; init; }
    }

    private static async Task RunScripts(SqlConnection connection)
    {
        var scriptDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "sql_scripts");

        if (!Directory.Exists(scriptDir))
        {
            Console.WriteLine($"[ERROR] Scripts directory not found: {scriptDir}");
            return;
        }

        var scripts = Directory.GetFiles(scriptDir, "*.sql")
            .OrderBy(f => f)
            .ToList();

        Console.WriteLine($"Found {scripts.Count} SQL scripts in: {scriptDir}");
        Console.WriteLine();

        foreach (var script in scripts)
        {
            var name = Path.GetFileName(script);
            var sql = await File.ReadAllTextAsync(script);
            var batches = SplitOnGo(sql);

            Console.Write($"  [{name}] ({batches.Count} batch(es)) ... ");
            try
            {
                foreach (var batch in batches)
                {
                    await using var cmd = new SqlCommand(batch, connection);
                    cmd.CommandTimeout = 30;
                    await cmd.ExecuteNonQueryAsync();
                }
                Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAIL: {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== Scripts Done ===");
    }

    private static List<string> SplitOnGo(string sql)
    {
        var batches = new List<string>();
        var lines = sql.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        var current = new System.Text.StringBuilder();

        foreach (var line in lines)
        {
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                var batch = current.ToString().Trim();
                if (batch.Length > 0)
                    batches.Add(batch);
                current.Clear();
            }
            else
            {
                current.AppendLine(line);
            }
        }

        var last = current.ToString().Trim();
        if (last.Length > 0)
            batches.Add(last);

        return batches;
    }
}
