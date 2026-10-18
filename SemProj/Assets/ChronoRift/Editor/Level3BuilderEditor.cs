using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class Level3BuilderEditor
{
    private static Font defaultFont;
    private static Sprite sprPanel;
    private static Sprite sprButton;

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ChronoRift/Sprites/" + name);
    }

    [MenuItem("ChronoRift/Build Level 3 (Future City Lab)")]
    public static void BuildLevel3()
    {
        string scenePath = "Assets/Scenes/Level 3.unity";
        Scene level3Scene;

        if (System.IO.File.Exists(scenePath))
        {
            level3Scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        else
        {
            level3Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(level3Scene, scenePath);
        }

        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        sprPanel = LoadSprite("spr_ui_panel.png");
        sprButton = LoadSprite("spr_ui_button.png");

        // Clean any existing objects
        var roots = level3Scene.GetRootGameObjects();
        foreach (var r in roots)
        {
            Object.DestroyImmediate(r);
        }

        // 1. Setup Camera & Light
        SetupCameraAndLighting();

        // 2. Setup Managers
        SetupManagers(out GameManager gm, out SoundManager sm, out UIManager ui);

        // 3. Setup Background
        SetupBackground();

        // 4. Setup Lab Geometry, Obstacles, Chips, Security Door, Time Core
        SetupLabGeometry(out PowerChip chip0, out PowerChip chip1, out PowerChip chip2, out SecurityDoor secDoor, out TimeCore timeCore);

        // 5. Setup Player
        SetupPlayer(out PlayerController player);

        // 6. Setup UI
        SetupUI(gm, ui);

        // Save Scene
        EditorSceneManager.MarkSceneDirty(level3Scene);
        EditorSceneManager.SaveScene(level3Scene);

        // Add to Build Settings
        AddSceneToBuildSettings(scenePath);

        Debug.Log("Chrono Rift: Level 3 built and saved successfully!");
    }

    private static void SetupCameraAndLighting()
    {
        // Main Camera
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        camGo.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = 5.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.04f, 0.05f, 0.1f);
        camGo.transform.position = new Vector3(1f, 2f, -10f);

        CameraFollow follow = camGo.AddComponent<CameraFollow>();
        camGo.AddComponent<AudioListener>();

        // Global Light 2D
        GameObject lightGo = new GameObject("Global Light 2D");
        var light = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
        light.color = new Color(0.85f, 0.92f, 1.0f);
        light.intensity = 1.0f;
    }

    private static void SetupManagers(out GameManager gm, out SoundManager sm, out UIManager ui)
    {
        GameObject mgrGo = new GameObject("_GameManagers");
        gm = mgrGo.AddComponent<GameManager>();
        sm = mgrGo.AddComponent<SoundManager>();
        ui = mgrGo.AddComponent<UIManager>();
    }

    private static void SetupBackground()
    {
        GameObject bgRoot = new GameObject("ParallaxBackground");
        Sprite bgLab = LoadSprite("spr_bg_lab.png");

        GameObject labBgLayer = new GameObject("LabBackdrop");
        labBgLayer.transform.parent = bgRoot.transform;

        var pLab = labBgLayer.AddComponent<ParallaxLayer>();
        SetPrivateField(pLab, "parallaxFactorX", 0.18f);
        SetPrivateField(pLab, "parallaxFactorY", 0.06f);

        for (int i = -1; i <= 5; i++)
        {
            GameObject tile = new GameObject("LabBackdrop_" + i);
            tile.transform.parent = labBgLayer.transform;
            tile.transform.position = new Vector3(i * 14f, 6f, 0f);
            tile.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = bgLab;
            sr.sortingOrder = -50;
        }
    }

    private static void SetupLabGeometry(out PowerChip chip0, out PowerChip chip1, out PowerChip chip2, out SecurityDoor secDoor, out TimeCore timeCore)
    {
        GameObject geom = new GameObject("_LevelGeometry");
        Sprite sprFloor = LoadSprite("spr_lab_floor.png");
        Sprite sprPlat = LoadSprite("spr_lab_platform.png");
        Sprite sprSpike = LoadSprite("spr_hazard_spikes.png");
        Sprite sprConsole = LoadSprite("spr_lab_console.png");
        Sprite sprChip = LoadSprite("spr_power_chip.png");
        Sprite sprDoor = LoadSprite("spr_security_door.png");
        Sprite sprCore = LoadSprite("spr_time_core.png");

        // Section 1: Entry / Server Bay (X: -3 to 12, topY: 0)
        CreateLabFloor(geom, sprFloor, -3f, 12f, 0f, 3f);

        // Hazard Pit 1: X: 12 to 17.5. Spikes at Y: -3.2
        CreateSpikeHazard(geom, sprSpike, 12f, 17.5f, -3.2f);

        // Section 2: Reactor Core Floor (X: 17.5 to 28, topY: 0.5)
        CreateLabFloor(geom, sprFloor, 17.5f, 28f, 0.5f, 3f);

        // Hazard Pit 2: X: 28 to 33.5. Spikes at Y: -3.2
        CreateSpikeHazard(geom, sprSpike, 28f, 33.5f, -3.2f);

        // Section 3: Pre-Chamber Corridor (X: 33.5 to 43, topY: 0.8)
        CreateLabFloor(geom, sprFloor, 33.5f, 43f, 0.8f, 3.2f);

        // Section 4: Time Core Chamber (X: 43 to 58, topY: 0.8)
        CreateLabFloor(geom, sprFloor, 43f, 58f, 0.8f, 3.2f);

        // Left Boundary Wall
        CreateWall(geom, sprFloor, -3.2f, 4.0f, 1.2f, 10f);

        // Right Boundary Wall
        CreateWall(geom, sprFloor, 58.2f, 4.0f, 1.2f, 10f);

        // Fall Death Barrier (Y: -8)
        GameObject fallDeath = new GameObject("FallDeathBarrier");
        fallDeath.transform.parent = geom.transform;
        fallDeath.transform.position = new Vector3(25f, -8f, 0f);
        fallDeath.tag = "Hazard";
        var deathCol = fallDeath.AddComponent<BoxCollider2D>();
        deathCol.size = new Vector2(100f, 2f);
        deathCol.isTrigger = true;

        // Platforms
        // Crossing Hazard Pit 1
        CreatePlatform(geom, sprPlat, 14.75f, 1.5f);

        // Catwalk 1 above Section 1
        CreatePlatform(geom, sprPlat, 5.0f, 2.8f);
        CreatePlatform(geom, sprPlat, 8.5f, 4.6f); // Holds Chip 0

        // Crossing to Section 2
        CreatePlatform(geom, sprPlat, 20.0f, 3.0f);
        CreatePlatform(geom, sprPlat, 24.0f, 5.2f); // Holds Chip 1

        // Crossing Hazard Pit 2
        CreatePlatform(geom, sprPlat, 30.75f, 1.8f);

        // High Observation Catwalk
        CreatePlatform(geom, sprPlat, 27.5f, 7.2f);
        CreatePlatform(geom, sprPlat, 31.5f, 8.8f);
        CreatePlatform(geom, sprPlat, 36.0f, 10.4f); // Holds Chip 2
        CreatePlatform(geom, sprPlat, 39.5f, 5.5f);  // Descent platform

        // Decorative Consoles
        CreateDecoration(geom, sprConsole, new Vector3(3.5f, 0.8f, 0f), Vector3.one, 2);
        CreateDecoration(geom, sprConsole, new Vector3(6.5f, 0.8f, 0f), Vector3.one, 2);
        CreateDecoration(geom, sprConsole, new Vector3(19.5f, 1.3f, 0f), Vector3.one, 2);
        CreateDecoration(geom, sprConsole, new Vector3(26.0f, 1.3f, 0f), Vector3.one, 2);
        CreateDecoration(geom, sprConsole, new Vector3(35.5f, 1.6f, 0f), Vector3.one, 2);
        CreateDecoration(geom, sprConsole, new Vector3(46.0f, 1.6f, 0f), Vector3.one, 2);
        CreateDecoration(geom, sprConsole, new Vector3(54.0f, 1.6f, 0f), Vector3.one, 2);

        // ============================================
        // 3 COLLECTIBLE POWER CHIPS
        // ============================================
        chip0 = CreatePowerChip(geom, sprChip, 0, new Vector3(8.5f, 5.5f, 0f));
        chip1 = CreatePowerChip(geom, sprChip, 1, new Vector3(24.0f, 6.1f, 0f));
        chip2 = CreatePowerChip(geom, sprChip, 2, new Vector3(36.0f, 11.3f, 0f));

        // ============================================
        // SECURITY BLAST DOOR (X: 42.5, Y: 2.2)
        // ============================================
        GameObject doorGo = new GameObject("SecurityDoor");
        doorGo.transform.parent = geom.transform;
        doorGo.transform.position = new Vector3(42.5f, 2.2f, 0f);

        var dSr = doorGo.AddComponent<SpriteRenderer>();
        dSr.sprite = sprDoor;
        dSr.sortingOrder = 10;

        var dCol = doorGo.AddComponent<BoxCollider2D>();
        dCol.size = new Vector2(1.2f, 2.6f);

        secDoor = doorGo.AddComponent<SecurityDoor>();
        SetPrivateField(secDoor, "doorCollider", dCol);
        SetPrivateField(secDoor, "doorRenderer", dSr);

        // Security Arch Frame
        GameObject archGo = new GameObject("SecurityArch");
        archGo.transform.parent = geom.transform;
        archGo.transform.position = new Vector3(42.5f, 4.3f, 0f);
        var archCol = archGo.AddComponent<BoxCollider2D>();
        archCol.size = new Vector2(1.4f, 1.8f);
        var aSr = archGo.AddComponent<SpriteRenderer>();
        aSr.sprite = sprFloor;
        aSr.sortingOrder = 9;

        // ============================================
        // TIME CORE IN FINAL CHAMBER (X: 50.0, Y: 2.3)
        // ============================================
        // Pedestal
        GameObject pedestal = new GameObject("CorePedestal");
        pedestal.transform.parent = geom.transform;
        pedestal.transform.position = new Vector3(50.0f, 1.3f, 0f);
        pedestal.transform.localScale = new Vector3(1.4f, 1.0f, 1f);
        var pSr = pedestal.AddComponent<SpriteRenderer>();
        pSr.sprite = sprConsole;
        pSr.sortingOrder = 4;

        GameObject coreGo = new GameObject("TimeCore");
        coreGo.transform.parent = geom.transform;
        coreGo.transform.position = new Vector3(50.0f, 2.4f, 0f);

        var cSr = coreGo.AddComponent<SpriteRenderer>();
        cSr.sprite = sprCore;
        cSr.sortingOrder = 15;

        var cCol = coreGo.AddComponent<CircleCollider2D>();
        cCol.isTrigger = true;
        cCol.radius = 2.0f;

        timeCore = coreGo.AddComponent<TimeCore>();
    }

    private static PowerChip CreatePowerChip(GameObject parent, Sprite spr, int chipId, Vector3 pos)
    {
        GameObject chipGo = new GameObject($"PowerChip_{chipId}");
        chipGo.transform.parent = parent.transform;
        chipGo.transform.position = pos;

        var sr = chipGo.AddComponent<SpriteRenderer>();
        sr.sprite = spr;
        sr.sortingOrder = 12;

        var col = chipGo.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        var chip = chipGo.AddComponent<PowerChip>();
        SetPrivateField(chip, "chipId", chipId);
        return chip;
    }

    private static void CreateLabFloor(GameObject parent, Sprite sprFloor, float startX, float endX, float topY, float depth)
    {
        float width = endX - startX;
        float centerX = startX + width / 2f;
        float centerY = topY - depth / 2f;

        GameObject slab = new GameObject($"LabFloor_{startX:F0}_{endX:F0}");
        slab.transform.parent = parent.transform;
        slab.transform.position = new Vector3(centerX, centerY, 0f);

        var col = slab.AddComponent<BoxCollider2D>();
        col.size = new Vector2(width, depth);

        int count = Mathf.CeilToInt(width);
        for (int i = 0; i < count; i++)
        {
            float tx = startX + 0.5f + i;
            if (tx > endX) break;

            GameObject topTile = new GameObject("FloorTileTop_" + i);
            topTile.transform.parent = slab.transform;
            topTile.transform.position = new Vector3(tx, topY - 0.5f, 0f);
            var sr = topTile.AddComponent<SpriteRenderer>();
            sr.sprite = sprFloor;
            sr.sortingOrder = 0;

            GameObject subTile = new GameObject("FloorTileSub_" + i);
            subTile.transform.parent = slab.transform;
            subTile.transform.position = new Vector3(tx, topY - 1.5f, 0f);
            var srSub = subTile.AddComponent<SpriteRenderer>();
            srSub.sprite = sprFloor;
            srSub.sortingOrder = -5;
        }
    }

    private static void CreateWall(GameObject parent, Sprite spr, float x, float y, float width, float height)
    {
        GameObject wall = new GameObject($"Wall_{x:F1}");
        wall.transform.parent = parent.transform;
        wall.transform.position = new Vector3(x, y, 0f);
        var col = wall.AddComponent<BoxCollider2D>();
        col.size = new Vector2(width, height);
        var sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = spr;
        sr.transform.localScale = new Vector3(width, height, 1f);
        sr.sortingOrder = -2;
    }

    private static void CreatePlatform(GameObject parent, Sprite sprPlat, float x, float y)
    {
        GameObject plat = new GameObject($"Platform_{x:F1}_{y:F1}");
        plat.transform.parent = parent.transform;
        plat.transform.position = new Vector3(x, y, 0f);

        var sr = plat.AddComponent<SpriteRenderer>();
        sr.sprite = sprPlat;
        sr.sortingOrder = 0;

        var col = plat.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.0f, 0.6f);
        col.offset = new Vector2(0f, 0.05f);
    }

    private static void CreateSpikeHazard(GameObject parent, Sprite sprSpike, float startX, float endX, float y)
    {
        GameObject haz = new GameObject($"HazardSpikes_{startX:F0}");
        haz.transform.parent = parent.transform;
        haz.transform.position = new Vector3((startX + endX) / 2f, y, 0f);
        haz.tag = "Hazard";

        var col = haz.AddComponent<BoxCollider2D>();
        col.size = new Vector2(endX - startX, 0.8f);
        col.isTrigger = true;

        int count = Mathf.CeilToInt(endX - startX);
        for (int i = 0; i < count; i++)
        {
            GameObject sp = new GameObject("Spike_" + i);
            sp.transform.parent = haz.transform;
            sp.transform.position = new Vector3(startX + 0.5f + i, y + 0.2f, 0f);
            var sr = sp.AddComponent<SpriteRenderer>();
            sr.sprite = sprSpike;
            sr.sortingOrder = 5;
        }
    }

    private static void CreateDecoration(GameObject parent, Sprite spr, Vector3 pos, Vector3 scale, int order)
    {
        GameObject decor = new GameObject("Decor_" + spr.name);
        decor.transform.parent = parent.transform;
        decor.transform.position = pos;
        decor.transform.localScale = scale;
        var sr = decor.AddComponent<SpriteRenderer>();
        sr.sprite = spr;
        sr.sortingOrder = order;
    }

    private static void SetupPlayer(out PlayerController player)
    {
        GameObject pGo = new GameObject("Player");
        pGo.tag = "Player";
        pGo.transform.position = new Vector3(1f, 1.2f, 0f);

        var sr = pGo.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("spr_player_idle.png");
        sr.sortingOrder = 10;

        var rb = pGo.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = pGo.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.9f);

        player = pGo.AddComponent<PlayerController>();
        player.idleSprite = LoadSprite("spr_player_idle.png");
        player.walk1Sprite = LoadSprite("spr_player_walk1.png");
        player.walk2Sprite = LoadSprite("spr_player_walk2.png");
        player.jumpSprite = LoadSprite("spr_player_jump.png");

        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.SetTarget(pGo.transform);
        }
    }

    private static void SetupUI(GameManager gm, UIManager ui)
    {
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        var uiModType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (uiModType != null)
        {
            eventSystem.AddComponent(uiModType);
        }
        else
        {
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // 1. HUD Panel
        GameObject hud = CreateUIPanel(canvasGo, "HUDPanel", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, Color.clear);

        // Top Bar
        GameObject topBar = CreateUIPanel(hud, "TopBar", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -45), new Vector2(0, 70), new Color(0.06f, 0.08f, 0.12f, 0.92f));

        // Era Badge
        GameObject eraBadge = CreateUIPanel(topBar, "EraBadge", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(160, 0), new Vector2(280, 42), new Color(0.12f, 0.18f, 0.28f, 0.95f));
        CreateUIText(eraBadge, "EraText", "ERA: 2185 AD — TIME LAB", 17, new Color(0.3f, 0.9f, 1f), TextAnchor.MiddleCenter);

        // Lives Badge
        GameObject livesBadge = CreateUIPanel(topBar, "LivesBadge", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(360, 0), new Vector2(160, 42), new Color(0.2f, 0.08f, 0.12f, 0.95f));
        Text livesText = CreateUIText(livesBadge, "LivesText", "LIVES: 5 / 5", 18, new Color(1f, 0.35f, 0.4f), TextAnchor.MiddleCenter);

        // Chip Counter Badge
        GameObject chipBadge = CreateUIPanel(topBar, "ChipBadge", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, 48), new Color(0.08f, 0.14f, 0.2f, 0.95f));
        Text chipCountText = CreateUIText(chipBadge, "ChipCountText", "Power Chips: 0/3", 20, new Color(0.3f, 0.95f, 1f), TextAnchor.MiddleCenter);

        // Chip Icons
        Sprite sprChip = LoadSprite("spr_power_chip.png");
        Image[] chipIcons = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject iconGo = new GameObject($"ChipIcon_{i}");
            iconGo.transform.parent = topBar.transform;
            var rt = iconGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(160 + i * 36, 0);
            rt.sizeDelta = new Vector2(28, 28);
            var img = iconGo.AddComponent<Image>();
            img.sprite = sprChip;
            img.color = new Color(0.25f, 0.25f, 0.25f, 0.6f);
            chipIcons[i] = img;
        }

        // Objective Bar
        GameObject objBox = CreateUIPanel(hud, "ObjectiveBox", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -95), new Vector2(800, 38), new Color(0.08f, 0.12f, 0.18f, 0.85f));
        Text objText = CreateUIText(objBox, "ObjectiveText", "OBJECTIVE: Search the laboratory to collect 3 Power Chips.", 17, new Color(0.95f, 0.95f, 0.8f), TextAnchor.MiddleCenter);

        // Prompt Box
        GameObject promptBox = CreateUIPanel(hud, "PromptBox", new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(540, 75), new Color(0.06f, 0.1f, 0.16f, 0.95f));
        Text promptText = CreateUIText(promptBox, "PromptText", "Press [E] to Interact", 21, new Color(1f, 0.9f, 0.3f), TextAnchor.MiddleCenter);
        promptBox.SetActive(false);

        // Notification Text
        GameObject notifGo = new GameObject("NotificationText");
        notifGo.transform.parent = hud.transform;
        var notifRt = notifGo.AddComponent<RectTransform>();
        notifRt.anchorMin = new Vector2(0.5f, 0.82f);
        notifRt.anchorMax = new Vector2(0.5f, 0.82f);
        notifRt.sizeDelta = new Vector2(800, 50);
        notifRt.anchoredPosition = Vector2.zero;
        Text notifText = notifGo.AddComponent<Text>();
        notifText.font = defaultFont;
        notifText.fontSize = 24;
        notifText.color = new Color(0.3f, 1f, 0.6f);
        notifText.alignment = TextAnchor.MiddleCenter;
        notifGo.SetActive(false);

        // 2. Main Menu Panel (Fullscreen)
        GameObject menu = CreateUIPanel(canvasGo, "MainMenuPanel", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(0.04f, 0.06f, 0.1f, 0.97f));

        GameObject menuContent = CreateUIPanel(menu, "MenuContent", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 680), Color.clear);
        Text titleText = CreateUIText(menuContent, "Title", "CHRONO RIFT", 64, new Color(0.2f, 0.95f, 1f), TextAnchor.UpperCenter);
        titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);

        Text subTitle = CreateUIText(menuContent, "Subtitle", "RESTORE THE TIMELINE — LEVEL 3: FUTURE CITY LAB", 22, new Color(1f, 0.85f, 0.2f), TextAnchor.UpperCenter);
        subTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -95);

        GameObject storyBox = CreateUIPanel(menuContent, "StoryBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(680, 180), new Color(0.08f, 0.12f, 0.2f, 0.92f));
        CreateUIText(storyBox, "StoryText", "2185 AD: Temporal epicenter reached!\n\nThe master Time Core in the Abandoned Time Machine Laboratory is unstable and locked behind a high-security blast barrier.\n\nCollect 3 Power Chips to deactivate the security grid, stabilize the Time Core, and permanently restore the timeline across all eras!", 19, new Color(0.9f, 0.95f, 1f), TextAnchor.MiddleCenter);

        CreateUIButton(menuContent, "StartBtn", "START LEVEL 3", new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(320, 54), () => gm.StartGame());
        CreateUIButton(menuContent, "HowToPlayBtn", "HOW TO PLAY", new Vector2(0.5f, 0.5f), new Vector2(0, -180), new Vector2(320, 54), () => ui.ShowHowToPlay(true));
        CreateUIButton(menuContent, "QuitBtn", "QUIT GAME", new Vector2(0.5f, 0.5f), new Vector2(0, -250), new Vector2(320, 54), () => gm.QuitGame());

        // 3. How to Play Panel (Modal)
        GameObject howToPlay = CreateUIPanel(canvasGo, "HowToPlayPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(750, 600), new Color(0.06f, 0.09f, 0.14f, 0.98f));
        CreateUIText(howToPlay, "HtpTitle", "MISSION MANUAL & CONTROLS", 32, new Color(1f, 0.85f, 0.2f), TextAnchor.UpperCenter);

        GameObject htpBody = CreateUIPanel(howToPlay, "HtpBody", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 15), new Vector2(680, 420), Color.clear);
        string htpContent = "CONTROLS:\n" +
            "  • [A] / [D] or [Left / Right Arrows] : Move Left & Right\n" +
            "  • [Space] / [W] / [Up Arrow] : Jump\n" +
            "  • [E] : Interact with Time Core\n" +
            "  • [Escape] : Pause Game & View Menu\n\n" +
            "MISSION OBJECTIVES:\n" +
            "  1. Traverse futuristic laboratory catwalks and avoid plasma hazard pits.\n" +
            "  2. You have 5 LIVES per level. Falling into hazard pits costs 1 life.\n" +
            "  3. Collect all 3 Power Chips scattered across the facility.\n" +
            "  4. Collecting 3 chips automatically unlocks the security blast barrier.\n" +
            "  5. Enter the final chamber and press [E] to stabilize the Time Core!";
        CreateUIText(htpBody, "HtpText", htpContent, 20, Color.white, TextAnchor.MiddleLeft);

        CreateUIButton(howToPlay, "CloseHtpBtn", "CLOSE MANUAL", new Vector2(0.5f, 0), new Vector2(0, 35), new Vector2(240, 48), () => ui.ShowHowToPlay(false));
        howToPlay.SetActive(false);

        // 4. Pause Menu Panel
        GameObject pause = CreateUIPanel(canvasGo, "PauseMenuPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 480), new Color(0.06f, 0.08f, 0.12f, 0.98f));
        CreateUIText(pause, "PauseTitle", "CHRONO FIELD PAUSED", 32, new Color(1f, 0.85f, 0.2f), TextAnchor.UpperCenter);

        CreateUIButton(pause, "ResumeBtn", "RESUME GAME", new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(280, 50), () => gm.ResumeGame());
        CreateUIButton(pause, "PauseHtpBtn", "HOW TO PLAY", new Vector2(0.5f, 0.5f), new Vector2(0, -5), new Vector2(280, 50), () => ui.ShowHowToPlay(true));
        CreateUIButton(pause, "RestartBtn", "RESTART LEVEL", new Vector2(0.5f, 0.5f), new Vector2(0, -70), new Vector2(280, 50), () => gm.RestartGame());
        CreateUIButton(pause, "MainMenuBtn", "MAIN MENU", new Vector2(0.5f, 0.5f), new Vector2(0, -135), new Vector2(280, 50), () => gm.GoToMainMenu());
        pause.SetActive(false);

        // 5. Victory Panel
        GameObject victory = CreateUIPanel(canvasGo, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 560), new Color(0.06f, 0.1f, 0.15f, 0.98f));
        CreateUIText(victory, "VicTitle", "TIMELINE RESTORED!", 44, new Color(0.2f, 0.95f, 0.4f), TextAnchor.UpperCenter);
        CreateUIText(victory, "VicSub", "You have saved all eras! — Chrono Rift Complete", 20, new Color(0.3f, 0.9f, 1f), TextAnchor.UpperCenter).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -70);

        GameObject statsBox = CreateUIPanel(victory, "StatsBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(580, 170), new Color(0.1f, 0.14f, 0.2f, 0.9f));
        Text vicStatsText = CreateUIText(statsBox, "StatsText", "TIMELINE RESTORED!\nYou have saved all eras!\n\nTIME: 00:00\nLIVES REMAINING: 5 / 5\nPOWER CHIPS: 3 / 3\nTIME CORE: RESTORED\nALL ERAS STABILIZED: 100%", 22, Color.white, TextAnchor.MiddleCenter);

        CreateUIButton(victory, "PlayAgainBtn", "PLAY AGAIN", new Vector2(0.5f, 0.5f), new Vector2(0, -135), new Vector2(280, 52), () => gm.RestartGame());
        CreateUIButton(victory, "VicMenuBtn", "MAIN MENU", new Vector2(0.5f, 0.5f), new Vector2(0, -200), new Vector2(280, 52), () => gm.GoToMainMenu());
        victory.SetActive(false);

        // Wire references to UIManager
        SetPrivateField(ui, "hudPanel", hud);
        SetPrivateField(ui, "mainMenuPanel", menu);
        SetPrivateField(ui, "pauseMenuPanel", pause);
        SetPrivateField(ui, "howToPlayPanel", howToPlay);
        SetPrivateField(ui, "victoryPanel", victory);

        SetPrivateField(ui, "livesText", livesText);
        SetPrivateField(ui, "eggCounterText", chipCountText);
        SetPrivateField(ui, "eggIcons", chipIcons);
        SetPrivateField(ui, "objectiveText", objText);
        SetPrivateField(ui, "promptBox", promptBox);
        SetPrivateField(ui, "promptText", promptText);
        SetPrivateField(ui, "notificationText", notifText);
        SetPrivateField(ui, "victoryStatsText", vicStatsText);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return; // Already present
        }

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, newScenes, scenes.Length);
        newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }

    private static GameObject CreateUIPanel(GameObject parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color c)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent.transform;
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.sprite = sprPanel;
        img.type = Image.Type.Sliced;
        img.color = c;
        return go;
    }

    private static Text CreateUIText(GameObject parent, string name, string content, int fontSize, Color c, TextAnchor align)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent.transform;
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        var txt = go.AddComponent<Text>();
        txt.font = defaultFont;
        txt.fontSize = fontSize;
        txt.text = content;
        txt.color = c;
        txt.alignment = align;
        return txt;
    }

    private static GameObject CreateUIButton(GameObject parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent.transform;
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.sprite = sprButton;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.85f, 0.9f, 1f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.6f);
        colors.pressedColor = new Color(0.7f, 0.85f, 1f);
        btn.colors = colors;

        if (onClick != null)
        {
            btn.onClick.AddListener(onClick);
        }

        GameObject tGo = new GameObject("Label");
        tGo.transform.parent = go.transform;
        var trt = tGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        trt.anchoredPosition = Vector2.zero;

        var txt = tGo.AddComponent<Text>();
        txt.font = defaultFont;
        txt.fontSize = 20;
        txt.text = label;
        txt.color = new Color(0.9f, 0.95f, 1f);
        txt.alignment = TextAnchor.MiddleCenter;

        return go;
    }

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var f = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (f != null)
        {
            f.SetValue(obj, value);
        }
    }
}
