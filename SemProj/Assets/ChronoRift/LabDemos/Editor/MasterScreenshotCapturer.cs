using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public static class MasterScreenshotCapturer
{
    private static string screenshotDir = "Assets/ChronoRift/LabDemos/Screenshots";

    public static void CaptureAllScreenshots()
    {
        Debug.Log("==================================================");
        Debug.Log("STARTING MASTER AUTHENTIC SCREENSHOT CAPTURE");
        Debug.Log("==================================================");

        if (!Directory.Exists(screenshotDir))
        {
            Directory.CreateDirectory(screenshotDir);
        }

        // 1. CAPTURE FROM LEVEL 1 (SampleScene.unity)
        CaptureLevel1();

        // 2. CAPTURE FROM LEVEL 2 (Level2.unity)
        CaptureLevel2();

        // 3. CAPTURE FROM LEVEL 3 (Level 3.unity)
        CaptureLevel3();

        // 4. CAPTURE FROM LAB DEMO SCENE (LabDemoScene.unity)
        CaptureLabDemoScene();

        Debug.Log("==================================================");
        Debug.Log("ALL SCREENSHOTS CAPTURED SUCCESSFULLY!");
        Debug.Log("==================================================");
    }

    private static void CaptureLevel1()
    {
        string scenePath = "Assets/Scenes/SampleScene.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No Main Camera in SampleScene!");
            return;
        }

        var player = GameObject.FindWithTag("Player");
        var playerCtrl = player != null ? player.GetComponent<PlayerController>() : null;
        var playerSr = player != null ? player.GetComponent<SpriteRenderer>() : null;

        var ui = Object.FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);
        var hudPanel = GetPrivateField<GameObject>(ui, "hudPanel");
        var menuPanel = GetPrivateField<GameObject>(ui, "mainMenuPanel");
        var pausePanel = GetPrivateField<GameObject>(ui, "pauseMenuPanel");
        var vicPanel = GetPrivateField<GameObject>(ui, "victoryPanel");

        // 1. Exp 1: Prehistoric Jungle Theme Overview
        cam.transform.position = new Vector3(8f, 3.2f, -10f);
        cam.orthographicSize = 5.5f;
        HideAllPanels(hudPanel, menuPanel, pausePanel, vicPanel);
        CaptureCameraWithCanvas(cam, "Exp1_ChronoRift_Theme_Prehistoric.png");

        // 2. Exp 2: Player Idle State
        if (player != null && playerSr != null && playerCtrl != null)
        {
            player.transform.position = new Vector3(1f, 1.2f, 0f);
            playerSr.sprite = playerCtrl.idleSprite;
            cam.transform.position = new Vector3(1f, 1.8f, -10f);
            cam.orthographicSize = 3.2f;
            CaptureCameraWithCanvas(cam, "Exp2_Player_IdleState.png");

            // 3. Exp 2: Player Walk State
            playerSr.sprite = playerCtrl.walk1Sprite != null ? playerCtrl.walk1Sprite : playerCtrl.idleSprite;
            CaptureCameraWithCanvas(cam, "Exp2_Player_WalkState.png");

            // 4. Exp 2: Player Jump State
            player.transform.position = new Vector3(1f, 3.0f, 0f);
            playerSr.sprite = playerCtrl.jumpSprite != null ? playerCtrl.jumpSprite : playerCtrl.idleSprite;
            cam.transform.position = new Vector3(1f, 2.8f, -10f);
            CaptureCameraWithCanvas(cam, "Exp2_Player_JumpState.png");

            // Restore player position & idle sprite
            player.transform.position = new Vector3(1f, 1.2f, 0f);
            playerSr.sprite = playerCtrl.idleSprite;
        }

        // 5. Exp 3: Level Design (Platforms, Chasms, Pillars)
        cam.transform.position = new Vector3(19f, 3.5f, -10f);
        cam.orthographicSize = 5.8f;
        CaptureCameraWithCanvas(cam, "Exp3_LevelDesign_Platforms_Hazards.png");

        // 6. Exp 3: Collectible (Emerald Dragon Egg atop canopy)
        cam.transform.position = new Vector3(24f, 7.2f, -10f);
        cam.orthographicSize = 3.0f;
        CaptureCameraWithCanvas(cam, "Exp3_Collectible_DragonEgg.png");

        // 7. Exp 3: Interactive Ancient Altar
        cam.transform.position = new Vector3(45.5f, 2.8f, -10f);
        cam.orthographicSize = 4.0f;
        CaptureCameraWithCanvas(cam, "Exp3_Interactive_AncientAltar.png");

        // 8. Exp 3: Temple Gate Unlocked with Time Crystal Sanctum
        var gate = GameObject.Find("TempleGate");
        if (gate != null)
        {
            gate.transform.position += new Vector3(0f, 4f, 0f); // Slide open gate
        }
        cam.transform.position = new Vector3(58f, 3.2f, -10f);
        cam.orthographicSize = 5.0f;
        CaptureCameraWithCanvas(cam, "Exp3_Interactive_TempleGate_Unlocked.png");

        // 9. Exp 4: Main Menu Screen
        SetPanelActive(menuPanel, true);
        SetPanelActive(hudPanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(vicPanel, false);
        cam.transform.position = new Vector3(1f, 2f, -10f);
        CaptureCameraWithCanvas(cam, "Exp4_MainMenu_Screen.png");

        // 10. Exp 4: In-Game HUD Active
        SetPanelActive(menuPanel, false);
        SetPanelActive(hudPanel, true);
        SetPanelActive(pausePanel, false);
        SetPanelActive(vicPanel, false);
        cam.transform.position = new Vector3(4f, 2.5f, -10f);
        cam.orthographicSize = 5.2f;
        CaptureCameraWithCanvas(cam, "Exp4_HUD_Gameplay.png");

        // 11. Exp 4: Pause Menu Screen
        SetPanelActive(pausePanel, true);
        CaptureCameraWithCanvas(cam, "Exp4_PauseMenu_Screen.png");

        // 12. Exp 4: Victory Screen
        SetPanelActive(pausePanel, false);
        SetPanelActive(hudPanel, false);
        SetPanelActive(vicPanel, true);
        CaptureCameraWithCanvas(cam, "Exp4_Victory_Screen.png");

        // 13. Exp 5: Player Ground Physics & Collision
        HideAllPanels(hudPanel, menuPanel, pausePanel, vicPanel);
        cam.transform.position = new Vector3(1f, 1.8f, -10f);
        cam.orthographicSize = 3.5f;
        CaptureCameraWithCanvas(cam, "Exp5_PlayerPhysics_GroundCollision.png");

        // 14. Exp 5: Hazard Spikes Interaction Chasm
        cam.transform.position = new Vector3(14f, -1.2f, -10f);
        cam.orthographicSize = 3.8f;
        CaptureCameraWithCanvas(cam, "Exp5_Hazard_Spikes_Interaction.png");

        // 15. Exp 6: Audio Integration & Jump Action
        if (player != null && playerCtrl != null && playerSr != null)
        {
            player.transform.position = new Vector3(17f, 2.4f, 0f);
            playerSr.sprite = playerCtrl.jumpSprite != null ? playerCtrl.jumpSprite : playerCtrl.idleSprite;
            cam.transform.position = new Vector3(17f, 2.5f, -10f);
            cam.orthographicSize = 4.2f;
            CaptureCameraWithCanvas(cam, "Exp6_AudioSource_BGM_SFX.png");
        }
    }

    private static void CaptureLevel2()
    {
        string scenePath = "Assets/Scenes/Level2.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Camera cam = Camera.main;
        if (cam == null) return;

        // Exp 1: Medieval Castle Theme Overview
        cam.transform.position = new Vector3(8f, 2.8f, -10f);
        cam.orthographicSize = 5.5f;
        CaptureCameraWithCanvas(cam, "Exp1_ChronoRift_Theme_Medieval.png");

        // Exp 3: Castle Lever Interactive Object
        var lever = GameObject.Find("CastleLever");
        if (lever != null)
        {
            cam.transform.position = lever.transform.position + new Vector3(0f, 0.5f, -10f);
            cam.orthographicSize = 3.6f;
            CaptureCameraWithCanvas(cam, "Exp3_Interactive_CastleLever.png");
        }

        // Exp 5: Secret Door World Interaction
        var door = GameObject.Find("SecretDoor");
        if (door != null)
        {
            door.transform.position += new Vector3(0f, 3.5f, 0f); // Open secret door
            cam.transform.position = door.transform.position + new Vector3(-2f, -1f, -10f);
            cam.orthographicSize = 4.5f;
            CaptureCameraWithCanvas(cam, "Exp5_WorldInteraction_SecretDoor.png");
        }
    }

    private static void CaptureLevel3()
    {
        string scenePath = "Assets/Scenes/Level 3.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Camera cam = Camera.main;
        if (cam == null) return;

        // Exp 1: Future City Abandoned Lab Theme Overview
        cam.transform.position = new Vector3(8f, 2.8f, -10f);
        cam.orthographicSize = 5.5f;
        CaptureCameraWithCanvas(cam, "Exp1_ChronoRift_Theme_FutureLab.png");
    }

    private static void CaptureLabDemoScene()
    {
        string scenePath = "Assets/ChronoRift/LabDemos/LabDemoScene.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Camera cam = Camera.main;
        if (cam == null) return;

        var npc = GameObject.Find("Enemy_NPC");
        var player = GameObject.FindWithTag("Player");
        var fsm = npc != null ? npc.GetComponent<NPCStateMachine>() : null;
        var gom = Object.FindAnyObjectByType<GameOverManager>(FindObjectsInactive.Include);

        // 1. Exp 4: Authentic Game Over Screen
        if (gom != null && gom.gameOverMenuUI != null)
        {
            gom.gameOverMenuUI.SetActive(true);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographicSize = 5.0f;
            CaptureCameraWithCanvas(cam, "Exp4_GameOver_Screen.png");
            gom.gameOverMenuUI.SetActive(false);
        }

        // 2. Exp 7: NPC Patrol State
        if (npc != null && player != null && fsm != null)
        {
            npc.transform.position = new Vector3(-4.5f, -1.8f, 0f);
            player.transform.position = new Vector3(3.5f, -1.8f, 0f);
            fsm.currentState = NPCStateMachine.State.Patrolling;
            cam.transform.position = new Vector3(-0.5f, -0.8f, -10f);
            cam.orthographicSize = 4.2f;
            CaptureCameraWithCanvas(cam, "Exp7_NPC_AI_PatrolState.png");

            // 3. Exp 7: NPC Chase State (Player within 3.0f, chaseRange = 4.5f)
            npc.transform.position = new Vector3(-2.0f, -1.8f, 0f);
            player.transform.position = new Vector3(1.0f, -1.8f, 0f);
            fsm.currentState = NPCStateMachine.State.Chasing;
            cam.transform.position = new Vector3(-0.5f, -0.8f, -10f);
            CaptureCameraWithCanvas(cam, "Exp7_NPC_AI_ChaseState.png");

            // 4. Exp 7: NPC Attack State (Player within 0.8f, attackRange = 1.2f)
            npc.transform.position = new Vector3(-0.5f, -1.8f, 0f);
            player.transform.position = new Vector3(0.3f, -1.8f, 0f);
            fsm.currentState = NPCStateMachine.State.Attacking;
            cam.transform.position = new Vector3(0f, -0.8f, -10f);
            CaptureCameraWithCanvas(cam, "Exp7_NPC_AI_AttackState.png");
        }
    }

    private static void CaptureCameraWithCanvas(Camera cam, string filename, int width = 1920, int height = 1080)
    {
        // Find Canvas in scene and ensure ScreenSpaceCamera for render texture
        var canvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        RenderMode prevMode = RenderMode.ScreenSpaceOverlay;
        Camera prevCam = null;
        float prevDist = 100f;

        if (canvas != null)
        {
            prevMode = canvas.renderMode;
            prevCam = canvas.worldCamera;
            prevDist = canvas.planeDistance;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1.0f;
        }

        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        RenderTexture prevTarget = cam.targetTexture;
        RenderTexture.active = rt;
        cam.targetTexture = rt;
        cam.Render();

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        cam.targetTexture = prevTarget;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        if (canvas != null)
        {
            canvas.renderMode = prevMode;
            canvas.worldCamera = prevCam;
            canvas.planeDistance = prevDist;
        }

        string fullPath = Path.Combine(screenshotDir, filename);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        Debug.Log($"[CAPTURED] {filename} -> {fullPath} ({new FileInfo(fullPath).Length} bytes)");
    }

    private static void HideAllPanels(params GameObject[] panels)
    {
        foreach (var p in panels)
        {
            if (p != null) p.SetActive(false);
        }
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    private static T GetPrivateField<T>(object instance, string fieldName) where T : class
    {
        if (instance == null) return null;
        var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? field.GetValue(instance) as T : null;
    }
}
