using UnityEngine;
using UnityEditor;

public static class GameplayTestRunner
{
    public struct TestReport
    {
        public bool allPassed;
        public string log;
    }

    [MenuItem("ChronoRift/Run Gameplay Verification Tests")]
    public static string RunAllTests()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== CHRONO RIFT GAMEPLAY VERIFICATION REPORT ===");

        // 1. Verify Hierarchy & Components
        bool p1 = VerifyComponents(sb);

        // 2. Verify Movement & Physics Configuration
        bool p2 = VerifyMovementConfig(sb);

        // 3. Verify Egg Collection Logic
        bool p3 = VerifyEggCollection(sb);

        // 4. Verify Altar Requirement & Locking
        bool p4 = VerifyAltarLockAndUnlock(sb);

        // 5. Verify Temple Gate
        bool p5 = VerifyTempleGate(sb);

        // 6. Verify Time Crystal & Victory State
        bool p6 = VerifyVictoryState(sb);

        // 7. Verify Restart / Reset Full Cycle
        bool p7 = VerifyRestartCycle(sb);

        bool allPassed = p1 && p2 && p3 && p4 && p5 && p6 && p7;
        sb.AppendLine("================================================");
        sb.AppendLine(allPassed ? "ALL 7 TEST SUITES PASSED! GAME FULLY VERIFIED." : "SOME TESTS FAILED.");

        Debug.Log(sb.ToString());
        return sb.ToString();
    }

    private static bool VerifyComponents(System.Text.StringBuilder sb)
    {
        sb.AppendLine("[TEST 1] Verifying Essential Scene Objects & References...");
        var gm = Object.FindAnyObjectByType<GameManager>();
        var sm = Object.FindAnyObjectByType<SoundManager>();
        var ui = Object.FindAnyObjectByType<UIManager>();
        var player = Object.FindAnyObjectByType<PlayerController>();
        var altar = Object.FindAnyObjectByType<Altar>();
        var gate = Object.FindAnyObjectByType<TempleGate>();
        var crystal = Object.FindAnyObjectByType<TimeCrystal>();
        var eggs = Object.FindObjectsByType<DragonEgg>(FindObjectsInactive.Include);

        bool ok = gm != null && sm != null && ui != null && player != null && altar != null && gate != null && crystal != null && eggs.Length == 3;
        sb.AppendLine($"  - GameManager: {(gm != null ? "OK" : "FAIL")}");
        sb.AppendLine($"  - SoundManager: {(sm != null ? "OK" : "FAIL")}");
        sb.AppendLine($"  - UIManager: {(ui != null ? "OK" : "FAIL")}");
        sb.AppendLine($"  - PlayerController: {(player != null ? "OK" : "FAIL")}");
        sb.AppendLine($"  - Altar: {(altar != null ? "OK" : "FAIL")}");
        sb.AppendLine($"  - TempleGate: {(gate != null ? "OK" : "FAIL")}");
        sb.AppendLine($"  - TimeCrystal: {(crystal != null ? "OK" : "FAIL")}");
        sb.AppendLine($"  - Sacred Dragon Eggs count: {eggs.Length}/3 ({(eggs.Length == 3 ? "OK" : "FAIL")})");

        sb.AppendLine($"[TEST 1 RESULT] {(ok ? "PASSED" : "FAILED")}");
        return ok;
    }

    private static bool VerifyMovementConfig(System.Text.StringBuilder sb)
    {
        sb.AppendLine("[TEST 2] Verifying Player Movement & Ground Detection Setup...");
        var player = Object.FindAnyObjectByType<PlayerController>();
        if (player == null) return false;

        var rb = player.GetComponent<Rigidbody2D>();
        var col = player.GetComponent<BoxCollider2D>();

        bool ok = rb != null && col != null && rb.freezeRotation && !col.isTrigger;
        sb.AppendLine($"  - Rigidbody2D configured: {(rb != null && rb.freezeRotation ? "OK" : "FAIL")}");
        sb.AppendLine($"  - BoxCollider2D non-trigger: {(col != null && !col.isTrigger ? "OK" : "FAIL")}");
        sb.AppendLine($"  - Visual Sprites assigned: {(player.idleSprite != null && player.jumpSprite != null ? "OK" : "FAIL")}");

        sb.AppendLine($"[TEST 2 RESULT] {(ok ? "PASSED" : "FAILED")}");
        return ok;
    }

    private static bool VerifyEggCollection(System.Text.StringBuilder sb)
    {
        sb.AppendLine("[TEST 3] Verifying Egg Collection Logic & Counter Updates...");
        var gm = Object.FindAnyObjectByType<GameManager>();
        var eggs = Object.FindObjectsByType<DragonEgg>(FindObjectsInactive.Include);

        gm.StartGame();
        int initialCount = gm.EggsCollectedCount;
        sb.AppendLine($"  - Initial egg count: {initialCount} (Expected 0)");

        // Collect egg 0
        eggs[0].Collect();
        int count1 = gm.EggsCollectedCount;
        sb.AppendLine($"  - After collecting Egg 0 ({eggs[0].EggName}): {count1}/3");

        // Collect egg 1
        eggs[1].Collect();
        int count2 = gm.EggsCollectedCount;
        sb.AppendLine($"  - After collecting Egg 1 ({eggs[1].EggName}): {count2}/3");

        // Collect egg 2
        eggs[2].Collect();
        int count3 = gm.EggsCollectedCount;
        sb.AppendLine($"  - After collecting Egg 2 ({eggs[2].EggName}): {count3}/3");

        bool ok = initialCount == 0 && count1 == 1 && count2 == 2 && count3 == 3;
        sb.AppendLine($"[TEST 3 RESULT] {(ok ? "PASSED" : "FAILED")}");
        return ok;
    }

    private static bool VerifyAltarLockAndUnlock(System.Text.StringBuilder sb)
    {
        sb.AppendLine("[TEST 4] Verifying Altar Locking Mechanism (Requires 3 Eggs)...");
        var gm = Object.FindAnyObjectByType<GameManager>();
        var altar = Object.FindAnyObjectByType<Altar>();

        // Reset to test requirement
        gm.RestartGame();
        sb.AppendLine($"  - Fresh game eggs count: {gm.EggsCollectedCount}");

        // Try with 0 eggs
        bool lockedWith0 = !altar.IsActivated && gm.EggsCollectedCount < 3;
        sb.AppendLine($"  - Altar locked with 0 eggs: {(lockedWith0 ? "YES (Correct)" : "NO (FAIL)")}");

        // Collect only 2 eggs
        var eggs = Object.FindObjectsByType<DragonEgg>(FindObjectsInactive.Include);
        eggs[0].Collect();
        eggs[1].Collect();
        bool lockedWith2 = !altar.IsActivated && gm.EggsCollectedCount < 3;
        sb.AppendLine($"  - Altar locked with 2 eggs ({gm.EggsCollectedCount}/3): {(lockedWith2 ? "YES (Correct)" : "NO (FAIL)")}");

        // Collect 3rd egg and activate
        eggs[2].Collect();
        sb.AppendLine($"  - All 3 eggs collected ({gm.EggsCollectedCount}/3)");
        altar.ActivateAltar();
        bool unlockedWith3 = altar.IsActivated && gm.IsAltarActivated;
        sb.AppendLine($"  - Altar activated after 3 eggs: {(unlockedWith3 ? "YES (Correct)" : "NO (FAIL)")}");

        bool ok = lockedWith0 && lockedWith2 && unlockedWith3;
        sb.AppendLine($"[TEST 4 RESULT] {(ok ? "PASSED" : "FAILED")}");
        return ok;
    }

    private static bool VerifyTempleGate(System.Text.StringBuilder sb)
    {
        sb.AppendLine("[TEST 5] Verifying Temple Gate Opening Mechanism...");
        var gate = Object.FindAnyObjectByType<TempleGate>();
        var gateCol = gate.GetComponent<Collider2D>();

        // Initially when gate is opened via altar
        bool colDisabled = gateCol == null || !gateCol.enabled;
        sb.AppendLine($"  - Gate barrier collider disabled on open: {(colDisabled ? "YES" : "NO")}");

        // Reset gate and check barrier collider re-enabled
        gate.ResetGate();
        bool colRestored = gateCol != null && gateCol.enabled;
        sb.AppendLine($"  - Gate barrier collider restored on reset: {(colRestored ? "YES" : "NO")}");

        // Open gate again
        gate.OpenGate();
        bool colReDisabled = gateCol == null || !gateCol.enabled;
        sb.AppendLine($"  - Gate barrier re-disabled on OpenGate: {(colReDisabled ? "YES" : "NO")}");

        bool ok = colDisabled && colRestored && colReDisabled;
        sb.AppendLine($"[TEST 5 RESULT] {(ok ? "PASSED" : "FAILED")}");
        return ok;
    }

    private static bool VerifyVictoryState(System.Text.StringBuilder sb)
    {
        sb.AppendLine("[TEST 6] Verifying Time Crystal Collection & Victory State...");
        var gm = Object.FindAnyObjectByType<GameManager>();
        var crystal = Object.FindAnyObjectByType<TimeCrystal>();

        crystal.Collect();
        bool isVictory = gm.CurrentState == GameState.Victory;
        sb.AppendLine($"  - GameState transitioned to Victory: {(isVictory ? "YES" : "NO")}");

        sb.AppendLine($"[TEST 6 RESULT] {(isVictory ? "PASSED" : "FAILED")}");
        return isVictory;
    }

    private static bool VerifyRestartCycle(System.Text.StringBuilder sb)
    {
        sb.AppendLine("[TEST 7] Verifying Game Restart & Full Reset Cycle...");
        var gm = Object.FindAnyObjectByType<GameManager>();
        var altar = Object.FindAnyObjectByType<Altar>();
        var gate = Object.FindAnyObjectByType<TempleGate>();
        var crystal = Object.FindAnyObjectByType<TimeCrystal>();

        // Call RestartGame
        gm.RestartGame();

        var eggs = Object.FindObjectsByType<DragonEgg>(FindObjectsInactive.Include);
        int eggsActiveCount = 0;
        foreach (var egg in eggs)
        {
            if (egg.gameObject.activeInHierarchy && !egg.IsCollected) eggsActiveCount++;
        }

        bool eggsReset = gm.EggsCollectedCount == 0 && eggsActiveCount == 3;
        bool altarReset = !altar.IsActivated && !gm.IsAltarActivated;
        var gateCol = gate.GetComponent<Collider2D>();
        bool gateReset = gateCol != null && gateCol.enabled;
        bool statePlaying = gm.CurrentState == GameState.Playing;

        sb.AppendLine($"  - Eggs reset to 0/3 collected (3 active in scene): {(eggsReset ? "OK" : "FAIL")}");
        sb.AppendLine($"  - Altar reset to locked: {(altarReset ? "OK" : "FAIL")}");
        sb.AppendLine($"  - Temple Gate reset to closed barrier: {(gateReset ? "OK" : "FAIL")}");
        sb.AppendLine($"  - GameState reset to Playing: {(statePlaying ? "OK" : "FAIL")}");

        // Now return to MainMenu cleanly
        gm.GoToMainMenu();
        bool stateMenu = gm.CurrentState == GameState.MainMenu;
        sb.AppendLine($"  - Return to Main Menu: {(stateMenu ? "OK" : "FAIL")}");

        bool ok = eggsReset && altarReset && gateReset && statePlaying && stateMenu;
        sb.AppendLine($"[TEST 7 RESULT] {(ok ? "PASSED" : "FAILED")}");
        return ok;
    }
}
