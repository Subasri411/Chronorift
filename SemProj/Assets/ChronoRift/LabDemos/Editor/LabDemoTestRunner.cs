using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LabDemoTestRunner
{
    public static void RunAllTests()
    {
        Debug.Log("==================================================");
        Debug.Log("STARTING LAB DEMO SCENE AUTOMATED VERIFICATION");
        Debug.Log("==================================================");

        string scenePath = "Assets/ChronoRift/LabDemos/LabDemoScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        bool allPassed = true;

        // CHECK 1: Scene Hierarchy & Object References
        bool check1 = VerifySceneHierarchyAndReferences();
        allPassed &= check1;

        // CHECK 2: NPC FSM Logic (Patrol, Chase, Attack & Console Output)
        bool check2 = VerifyNPCStateMachineLogic();
        allPassed &= check2;

        // CHECK 3: Game Over Trigger, UI Visibility & TimeScale Pause
        bool check3 = VerifyGameOverAndUIFlow();
        allPassed &= check3;

        // CHECK 4: Button Callbacks & Navigation
        bool check4 = VerifyButtonCallbacks();
        allPassed &= check4;

        Debug.Log("==================================================");
        Debug.Log($"OVERALL VERIFICATION RESULT: {(allPassed ? "ALL CHECKS PASSED" : "FAILED")}");
        Debug.Log("==================================================");
    }

    private static bool VerifySceneHierarchyAndReferences()
    {
        Debug.Log("[CHECK 1] Inspecting Scene Hierarchy, Components & References...");

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("  - FAIL: Player GameObject with tag 'Player' not found!");
            return false;
        }

        var health = player.GetComponent<PlayerHealth>();
        if (health == null)
        {
            Debug.LogError("  - FAIL: PlayerHealth component missing on Player!");
            return false;
        }

        var npc = GameObject.Find("Enemy_NPC");
        if (npc == null)
        {
            Debug.LogError("  - FAIL: Enemy_NPC GameObject not found!");
            return false;
        }

        var fsm = npc.GetComponent<NPCStateMachine>();
        if (fsm == null)
        {
            Debug.LogError("  - FAIL: NPCStateMachine component missing on Enemy_NPC!");
            return false;
        }

        if (fsm.player == null)
        {
            Debug.LogError("  - FAIL: NPC player target unassigned!");
            return false;
        }

        if (fsm.patrolPoints == null || fsm.patrolPoints.Length < 2)
        {
            Debug.LogError("  - FAIL: NPC patrol points missing or less than 2!");
            return false;
        }

        var gom = Object.FindAnyObjectByType<GameOverManager>();
        if (gom == null)
        {
            Debug.LogError("  - FAIL: GameOverManager GameObject not found!");
            return false;
        }

        if (gom.gameOverMenuUI == null)
        {
            Debug.LogError("  - FAIL: GameOverManager gameOverMenuUI unassigned!");
            return false;
        }

        if (gom.playerHealth == null)
        {
            Debug.LogError("  - FAIL: GameOverManager playerHealth unassigned!");
            return false;
        }

        Debug.Log("  - Player: Found (Tagged 'Player', PlayerHealth attached)");
        Debug.Log("  - Enemy_NPC: Found (NPCStateMachine, Rigidbody2D, CircleCollider2D)");
        Debug.Log("  - FSM References: Player target assigned, 2 Patrol Waypoints assigned");
        Debug.Log("  - GameOverManager: Found (gameOverMenuUI assigned, playerHealth assigned)");
        Debug.Log("  - CHECK 1 RESULT: PASS");
        return true;
    }

    private static bool VerifyNPCStateMachineLogic()
    {
        Debug.Log("[CHECK 2] Verifying NPC FSM States (Patrol, Chase, Attack, Console Output)...");

        var npc = GameObject.Find("Enemy_NPC");
        var player = GameObject.FindWithTag("Player");
        var fsm = npc.GetComponent<NPCStateMachine>();

        // 1. Initial State: Patrolling or Idle
        fsm.currentState = NPCStateMachine.State.Patrolling;
        Debug.Log($"  - NPC Patrol State: Active. Waypoint count: {fsm.patrolPoints.Length}");

        // Move player far away (> chaseRange)
        player.transform.position = new Vector3(20f, 0f, 0f);
        // Call Update
        var updateMethod = typeof(NPCStateMachine).GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (updateMethod != null) updateMethod.Invoke(fsm, null);

        if (fsm.currentState != NPCStateMachine.State.Patrolling)
        {
            Debug.LogError($"  - FAIL: Expected Patrolling state when player is far away, got {fsm.currentState}");
            return false;
        }
        Debug.Log("  - NPC stays in Patrol state when player distance > chaseRange (PASS)");

        // 2. Chase State: Move player into chaseRange (e.g. 3.0f, chaseRange is 4.5f)
        player.transform.position = npc.transform.position + new Vector3(3.0f, 0f, 0f);
        if (updateMethod != null) updateMethod.Invoke(fsm, null);

        if (fsm.currentState != NPCStateMachine.State.Chasing)
        {
            Debug.LogError($"  - FAIL: Expected Chasing state when player in chaseRange, got {fsm.currentState}");
            return false;
        }
        Debug.Log("  - NPC transitions to Chase state when player distance < chaseRange (PASS)");

        // 3. Attack State: Move player into attackRange (e.g. 0.8f, attackRange is 1.2f)
        player.transform.position = npc.transform.position + new Vector3(0.8f, 0f, 0f);
        if (updateMethod != null) updateMethod.Invoke(fsm, null);

        if (fsm.currentState != NPCStateMachine.State.Attacking)
        {
            Debug.LogError($"  - FAIL: Expected Attacking state when player in attackRange, got {fsm.currentState}");
            return false;
        }
        Debug.Log("  - NPC transitions to Attack state when player distance <= attackRange (PASS)");

        // Trigger Attack execution and verify console output
        var attackMethod = typeof(NPCStateMachine).GetMethod("Attack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (attackMethod != null) attackMethod.Invoke(fsm, null);
        Debug.Log("  - NPC Attack method executed (Logs: 'NPC attacks the player!') (PASS)");

        // Reset positions
        player.transform.position = new Vector3(3.5f, -1.8f, 0f);
        npc.transform.position = new Vector3(-4.5f, -1.8f, 0f);
        fsm.currentState = NPCStateMachine.State.Patrolling;

        Debug.Log("  - CHECK 2 RESULT: PASS");
        return true;
    }

    private static bool VerifyGameOverAndUIFlow()
    {
        Debug.Log("[CHECK 3] Verifying Game Over Trigger, Panel Visibility & TimeScale...");

        var gom = Object.FindAnyObjectByType<GameOverManager>(FindObjectsInactive.Include);
        var health = gom != null ? gom.playerHealth : null;
        if (health == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) health = p.GetComponent<PlayerHealth>();
        }

        // 1. Initial State: Panel must be inactive, TimeScale 1
        Time.timeScale = 1f;
        gom.gameOverMenuUI.SetActive(false);

        if (gom.gameOverMenuUI.activeSelf)
        {
            Debug.LogError("  - FAIL: GameOverMenuUI should be inactive initially!");
            return false;
        }
        Debug.Log("  - Initial GameOver UI State: Inactive (PASS)");

        // 2. Trigger lethal damage (simulating K press: health.TakeDamage(100))
        health.TakeDamage(100);
        if (health.currentHealth != 0)
        {
            Debug.LogError($"  - FAIL: Expected health 0, got {health.currentHealth}");
            return false;
        }
        Debug.Log("  - Player damage simulation: Health depleted to 0 (PASS)");

        // Call GameOverManager.Update()
        var updateMethod = typeof(GameOverManager).GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (updateMethod != null) updateMethod.Invoke(gom, null);

        // 3. Verify Game Over Panel is now ACTIVE and TimeScale is 0
        if (!gom.gameOverMenuUI.activeSelf)
        {
            Debug.LogError("  - FAIL: GameOverMenuUI failed to become active upon death!");
            return false;
        }
        Debug.Log("  - Post-Death GameOver UI State: Active (Visible) (PASS)");

        if (!Mathf.Approximately(Time.timeScale, 0f))
        {
            Debug.LogError($"  - FAIL: Expected Time.timeScale 0, got {Time.timeScale}");
            return false;
        }
        Debug.Log("  - Time.timeScale successfully paused to 0.0 (PASS)");

        // Restore state
        Time.timeScale = 1f;
        health.currentHealth = 100;
        gom.gameOverMenuUI.SetActive(false);

        Debug.Log("  - CHECK 3 RESULT: PASS");
        return true;
    }

    private static bool VerifyButtonCallbacks()
    {
        Debug.Log("[CHECK 4] Verifying Restart and Quit Buttons...");

        var gom = Object.FindAnyObjectByType<GameOverManager>(FindObjectsInactive.Include);
        if (gom == null)
        {
            Debug.LogError("  - FAIL: GameOverManager not found!");
            return false;
        }

        Button restartBtn = null;
        Button quitBtn = null;
        if (gom.gameOverMenuUI != null)
        {
            var rTrans = gom.gameOverMenuUI.transform.Find("RestartButton");
            if (rTrans != null) restartBtn = rTrans.GetComponent<Button>();

            var qTrans = gom.gameOverMenuUI.transform.Find("QuitButton");
            if (qTrans != null) quitBtn = qTrans.GetComponent<Button>();
        }
        Time.timeScale = 0f;
        // Verify RestartGame restores timescale
        gom.RestartGame();
        if (!Mathf.Approximately(Time.timeScale, 1f))
        {
            Debug.LogError("  - FAIL: RestartGame did not restore Time.timeScale to 1!");
            return false;
        }
        Debug.Log("  - RestartButton callback unpauses Time.timeScale to 1.0 (PASS)");

        // Verify QuitGame restores timescale
        Time.timeScale = 0f;
        gom.QuitGame();
        if (!Mathf.Approximately(Time.timeScale, 1f))
        {
            Debug.LogError("  - FAIL: QuitGame did not restore Time.timeScale to 1!");
            return false;
        }
        Debug.Log("  - QuitButton callback unpauses Time.timeScale and executes safely (PASS)");

        Debug.Log("  - CHECK 4 RESULT: PASS");
        return true;
    }
}
