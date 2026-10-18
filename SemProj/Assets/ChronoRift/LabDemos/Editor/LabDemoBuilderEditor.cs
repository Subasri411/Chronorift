using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class LabDemoBuilderEditor
{
    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ChronoRift/Sprites/" + name);
    }

    [MenuItem("ChronoRift/Build Lab Demo Scene")]
    public static void BuildLabDemoScene()
    {
        string scenePath = "Assets/ChronoRift/LabDemos/LabDemoScene.unity";
        Scene demoScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Sprite sprPlatform = LoadSprite("spr_platform.png");
        Sprite sprPlayer = LoadSprite("spr_player_idle.png");
        Sprite sprPanel = LoadSprite("spr_ui_panel.png");
        Sprite sprButton = LoadSprite("spr_ui_button.png");

        // 1. Camera
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.12f, 0.18f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        // 2. Global Light 2D (if Light2D exists in project)
        var light2DType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (light2DType != null)
        {
            GameObject lightGo = new GameObject("Global Light 2D");
            lightGo.AddComponent(light2DType);
        }

        // 3. Ground Platform
        GameObject ground = new GameObject("Ground");
        ground.transform.position = new Vector3(0f, -2.5f, 0f);
        ground.transform.localScale = new Vector3(14f, 1f, 1f);
        var groundSr = ground.AddComponent<SpriteRenderer>();
        groundSr.sprite = sprPlatform;
        groundSr.sortingOrder = 0;
        var groundCol = ground.AddComponent<BoxCollider2D>();
        groundCol.size = new Vector2(2f, 0.6f);

        // 4. Waypoints for NPC Patrol
        GameObject wpA = new GameObject("Waypoint_A");
        wpA.transform.position = new Vector3(-4.5f, -1.8f, 0f);

        GameObject wpB = new GameObject("Waypoint_B");
        wpB.transform.position = new Vector3(-0.5f, -1.8f, 0f);

        // 5. Player
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(3.5f, -1.8f, 0f);

        var playerSr = player.AddComponent<SpriteRenderer>();
        playerSr.sprite = sprPlayer;
        playerSr.sortingOrder = 10;

        var playerCol = player.AddComponent<BoxCollider2D>();
        playerCol.size = new Vector2(0.8f, 1f);

        var playerHealth = player.AddComponent<PlayerHealth>();
        playerHealth.maxHealth = 100;
        playerHealth.currentHealth = 100;

        // 6. NPC with NPCStateMachine
        GameObject npc = new GameObject("Enemy_NPC");
        npc.transform.position = new Vector3(-4.5f, -1.8f, 0f);

        var npcSr = npc.AddComponent<SpriteRenderer>();
        npcSr.sprite = sprPlayer;
        npcSr.color = new Color(1f, 0.35f, 0.35f); // Red tint to distinguish as NPC enemy
        npcSr.sortingOrder = 10;

        var npcCol = npc.AddComponent<CircleCollider2D>();
        npcCol.radius = 0.5f;

        var npcRb = npc.AddComponent<Rigidbody2D>();
        npcRb.gravityScale = 0f;
        npcRb.freezeRotation = true;

        var fsm = npc.AddComponent<NPCStateMachine>();
        fsm.player = player.transform;
        fsm.patrolPoints = new Transform[] { wpA.transform, wpB.transform };
        fsm.speed = 2.2f;
        fsm.chaseRange = 4.5f;
        fsm.attackRange = 1.2f;

        // 7. UI Canvas
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        var uiModType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (uiModType != null)
        {
            esGo.AddComponent(uiModType);
        }
        else
        {
            esGo.AddComponent<StandaloneInputModule>();
        }

        // HUD Banner
        GameObject hud = CreatePanel(canvasGo, "HUDPanel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -35), new Vector2(850, 60), new Color(0.1f, 0.14f, 0.2f, 0.92f), sprPanel);
        CreateText(hud, "Title", "EXPERIMENT 4 & 7: UI & NPC AI DEMONSTRATION", 22, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleCenter, defaultFont);

        GameObject infoBox = CreatePanel(canvasGo, "InfoBox", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -95), new Vector2(850, 45), new Color(0.08f, 0.12f, 0.18f, 0.9f), sprPanel);
        CreateText(infoBox, "InfoText", "NPC patrols waypoints -> chases player -> attacks.  |  Press [K] to trigger Game Over.", 18, Color.white, TextAnchor.MiddleCenter, defaultFont);

        // Game Over Panel
        GameObject gameOverPanel = CreatePanel(canvasGo, "GameOverPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 440), new Color(0.06f, 0.08f, 0.12f, 0.98f), sprPanel);

        GameObject goTitle = CreateText(gameOverPanel, "GOTitle", "GAME OVER", 54, new Color(1f, 0.25f, 0.25f), TextAnchor.UpperCenter, defaultFont);
        goTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30);

        GameObject goSub = CreateText(gameOverPanel, "GOSub", "Chrono Energy Depleted — Timeline Fracture Imminent", 20, new Color(0.9f, 0.9f, 0.95f), TextAnchor.UpperCenter, defaultFont);
        goSub.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -95);

        // GameOverManager GameObject
        GameObject gomGo = new GameObject("_GameOverManager");
        var gom = gomGo.AddComponent<GameOverManager>();
        gom.gameOverMenuUI = gameOverPanel;
        gom.playerHealth = playerHealth;

        // Restart and Quit Buttons
        GameObject restartBtn = CreateButton(gameOverPanel, "RestartButton", "RESTART LEVEL", new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(280, 52), sprButton, defaultFont);
        restartBtn.GetComponent<Button>().onClick.AddListener(() => gom.RestartGame());

        GameObject quitBtn = CreateButton(gameOverPanel, "QuitButton", "QUIT GAME", new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(280, 52), sprButton, defaultFont);
        quitBtn.GetComponent<Button>().onClick.AddListener(() => gom.QuitGame());

        gameOverPanel.SetActive(false);

        // Save Scene
        EditorSceneManager.SaveScene(demoScene, scenePath);
        Debug.Log("ChronoRift: LabDemoScene built and saved successfully at " + scenePath);
    }

    private static GameObject CreatePanel(GameObject parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color c, Sprite spr)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent.transform;
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        if (spr != null)
        {
            img.sprite = spr;
            img.type = Image.Type.Sliced;
        }
        img.color = c;
        return go;
    }

    private static GameObject CreateText(GameObject parent, string name, string content, int fontSize, Color c, TextAnchor align, Font font)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent.transform;
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        var txt = go.AddComponent<Text>();
        txt.font = font;
        txt.fontSize = fontSize;
        txt.text = content;
        txt.color = c;
        txt.alignment = align;
        return go;
    }

    private static GameObject CreateButton(GameObject parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, Sprite spr, Font font)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent.transform;
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        if (spr != null)
        {
            img.sprite = spr;
            img.type = Image.Type.Sliced;
        }
        img.color = new Color(0.25f, 0.55f, 0.9f);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;

        GameObject textGo = new GameObject("Text");
        textGo.transform.parent = go.transform;
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;

        var txt = textGo.AddComponent<Text>();
        txt.font = font;
        txt.fontSize = 20;
        txt.text = label;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;

        return go;
    }
}
