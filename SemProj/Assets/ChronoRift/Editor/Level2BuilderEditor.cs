using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class Level2BuilderEditor
{
    private static Font defaultFont;
    private static Sprite sprPanel;
    private static Sprite sprButton;

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ChronoRift/Sprites/" + name);
    }

    [MenuItem("ChronoRift/Build Level 2 (Medieval Castle)")]
    public static void BuildLevel2()
    {
        string scenePath = "Assets/Scenes/Level2.unity";
        Scene level2Scene;

        if (System.IO.File.Exists(scenePath))
        {
            level2Scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        else
        {
            level2Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(level2Scene, scenePath);
        }

        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        sprPanel = LoadSprite("spr_ui_panel.png");
        sprButton = LoadSprite("spr_ui_button.png");

        // Clean any existing objects
        var roots = level2Scene.GetRootGameObjects();
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

        // 4. Setup Castle & Library Architecture
        SetupCastleGeometry(out CastleLever lever, out SecretDoor door, out TimeCrystal crystal);

        // 5. Setup Player
        SetupPlayer(out PlayerController player);

        // 6. Setup UI
        SetupUI(gm, ui);

        // Save Scene
        EditorSceneManager.MarkSceneDirty(level2Scene);
        EditorSceneManager.SaveScene(level2Scene);

        // Add to Build Settings
        AddSceneToBuildSettings(scenePath);

        Debug.Log("Chrono Rift: Level 2 built and saved successfully!");
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
        cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
        camGo.transform.position = new Vector3(1f, 2f, -10f);

        CameraFollow follow = camGo.AddComponent<CameraFollow>();
        camGo.AddComponent<AudioListener>();

        // Global Light 2D
        GameObject lightGo = new GameObject("Global Light 2D");
        var light = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
        light.color = new Color(0.95f, 0.92f, 0.85f);
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
        Sprite bgCastle = LoadSprite("spr_bg_castle.png");

        GameObject castleBgLayer = new GameObject("CastleBackdrop");
        castleBgLayer.transform.parent = bgRoot.transform;

        var pCastle = castleBgLayer.AddComponent<ParallaxLayer>();
        SetPrivateField(pCastle, "parallaxFactorX", 0.2f);
        SetPrivateField(pCastle, "parallaxFactorY", 0.08f);

        for (int i = -1; i <= 4; i++)
        {
            GameObject tile = new GameObject("CastleWall_" + i);
            tile.transform.parent = castleBgLayer.transform;
            tile.transform.position = new Vector3(i * 16f, 6f, 0f);
            tile.transform.localScale = new Vector3(4f, 4f, 1f);
            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = bgCastle;
            sr.sortingOrder = -50;
        }
    }

    private static void SetupCastleGeometry(out CastleLever lever, out SecretDoor door, out TimeCrystal crystal)
    {
        GameObject geom = new GameObject("_LevelGeometry");
        Sprite sprBrick = LoadSprite("spr_castle_brick.png");
        Sprite sprPlat = LoadSprite("spr_castle_platform.png");
        Sprite sprSpike = LoadSprite("spr_hazard_spikes.png");
        Sprite sprShelf = LoadSprite("spr_bookshelf.png");
        Sprite sprShelfSec = LoadSprite("spr_bookshelf_secret.png");
        Sprite sprLevOff = LoadSprite("spr_lever_off.png");
        Sprite sprLevOn = LoadSprite("spr_lever_on.png");
        Sprite sprSecDoor = LoadSprite("spr_secret_door.png");
        Sprite sprBanner = LoadSprite("spr_castle_banner.png");
        Sprite sprTorch = LoadSprite("spr_castle_torch.png");
        Sprite sprCrystal = LoadSprite("spr_time_crystal.png");

        // Section 1: Entry & Lower Library Hall (X: -2 to 11, Y: 0)
        CreateCastleFloor(geom, sprBrick, -2f, 11f, 0f, 3f);

        // Chasm 1 (X: 11 to 15.5): Dungeon spikes at Y: -3.2
        CreateSpikeHazard(geom, sprSpike, 11f, 15.5f, -3.2f);

        // Section 2: Library Central Stacks (X: 15.5 to 25, Y: 0.8)
        CreateCastleFloor(geom, sprBrick, 15.5f, 25f, 0.8f, 3f);

        // Chasm 2 (X: 25 to 29.5): Spikes at Y: -3.2
        CreateSpikeHazard(geom, sprSpike, 25f, 29.5f, -3.2f);

        // Section 3: Lower Vault Approach (X: 29.5 to 39, Y: 1.2)
        CreateCastleFloor(geom, sprBrick, 29.5f, 39f, 1.2f, 3.2f);

        // Section 4: Secret Vault Interior (X: 39 to 52, Y: 1.2)
        CreateCastleFloor(geom, sprBrick, 39f, 52f, 1.2f, 3.2f);

        // Fall Death Barrier (Y: -8)
        GameObject fallDeath = new GameObject("FallDeathBarrier");
        fallDeath.transform.parent = geom.transform;
        fallDeath.transform.position = new Vector3(25f, -8f, 0f);
        fallDeath.tag = "Hazard";
        var deathCol = fallDeath.AddComponent<BoxCollider2D>();
        deathCol.size = new Vector2(100f, 2f);
        deathCol.isTrigger = true;

        // Platforms (Library Wood Shelves/Ladders)
        // Crossing Chasm 1
        CreatePlatform(geom, sprPlat, 13.0f, 1.6f);

        // Ascending to Upper Library Tower / Balcony
        CreatePlatform(geom, sprPlat, 18.0f, 3.2f);
        CreatePlatform(geom, sprPlat, 22.0f, 4.6f);
        CreatePlatform(geom, sprPlat, 25.5f, 6.0f);
        CreatePlatform(geom, sprPlat, 29.0f, 7.4f);
        CreatePlatform(geom, sprPlat, 33.0f, 8.8f); // High Reading Alcove! Holds Secret Bookshelf & Lever!

        // Stepping return platform
        CreatePlatform(geom, sprPlat, 27.5f, 2.8f);

        // Bookshelves (Library Stacks)
        CreateDecoration(geom, sprShelf, new Vector3(3.5f, 1.0f, 0f), Vector3.one, 0);
        CreateDecoration(geom, sprShelf, new Vector3(7.5f, 1.0f, 0f), Vector3.one, 0);
        CreateDecoration(geom, sprShelf, new Vector3(17.5f, 1.8f, 0f), Vector3.one, 0);
        CreateDecoration(geom, sprShelf, new Vector3(21.5f, 1.8f, 0f), Vector3.one, 0);
        CreateDecoration(geom, sprShelf, new Vector3(31.5f, 8.2f, 0f), Vector3.one, 0);

        // Wall Banners
        CreateDecoration(geom, sprBanner, new Vector3(0.5f, 2.5f, 0f), Vector3.one, -10);
        CreateDecoration(geom, sprBanner, new Vector3(9.5f, 2.5f, 0f), Vector3.one, -10);
        CreateDecoration(geom, sprBanner, new Vector3(23.5f, 3.3f, 0f), Vector3.one, -10);
        CreateDecoration(geom, sprBanner, new Vector3(36.0f, 3.5f, 0f), Vector3.one, -10);
        CreateDecoration(geom, sprBanner, new Vector3(43.0f, 3.5f, 0f), Vector3.one, -10);
        CreateDecoration(geom, sprBanner, new Vector3(48.5f, 3.5f, 0f), Vector3.one, -10);

        // Wall Torches
        CreateDecoration(geom, sprTorch, new Vector3(2.0f, 1.8f, 0f), Vector3.one, -5);
        CreateDecoration(geom, sprTorch, new Vector3(16.5f, 2.5f, 0f), Vector3.one, -5);
        CreateDecoration(geom, sprTorch, new Vector3(30.0f, 8.0f, 0f), Vector3.one, -5);
        CreateDecoration(geom, sprTorch, new Vector3(39.0f, 3.0f, 0f), Vector3.one, -5);
        CreateDecoration(geom, sprTorch, new Vector3(46.0f, 3.0f, 0f), Vector3.one, -5);

        // ============================================
        // HIDDEN LEVER AT HIGH ALCOVE (X: 33.5, Y: 9.6)
        // ============================================
        GameObject shelfSecGo = new GameObject("SecretBookshelf");
        shelfSecGo.transform.parent = geom.transform;
        shelfSecGo.transform.position = new Vector3(33.5f, 9.8f, 0f);
        var sSr = shelfSecGo.AddComponent<SpriteRenderer>();
        sSr.sprite = sprShelfSec;
        sSr.sortingOrder = 5;

        GameObject leverGo = new GameObject("CastleLever");
        leverGo.transform.parent = geom.transform;
        leverGo.transform.position = new Vector3(33.5f, 9.6f, 0f);

        var lSr = leverGo.AddComponent<SpriteRenderer>();
        lSr.sprite = sprLevOff;
        lSr.sortingOrder = 10;

        var lCol = leverGo.AddComponent<CircleCollider2D>();
        lCol.isTrigger = true;
        lCol.radius = 2.2f;

        lever = leverGo.AddComponent<CastleLever>();
        SetPrivateField(lever, "leverOffSprite", sprLevOff);
        SetPrivateField(lever, "leverOnSprite", sprLevOn);
        SetPrivateField(lever, "leverRenderer", lSr);

        // ============================================
        // SECRET ROOM DOOR AT VAULT ENTRY (X: 39.5, Y: 2.4)
        // ============================================
        GameObject doorGo = new GameObject("SecretDoor");
        doorGo.transform.parent = geom.transform;
        doorGo.transform.position = new Vector3(39.5f, 2.4f, 0f);

        var dSr = doorGo.AddComponent<SpriteRenderer>();
        dSr.sprite = sprSecDoor;
        dSr.sortingOrder = 10;

        var dCol = doorGo.AddComponent<BoxCollider2D>();
        dCol.size = new Vector2(1.2f, 2.4f);

        door = doorGo.AddComponent<SecretDoor>();
        SetPrivateField(door, "doorCollider", dCol);
        SetPrivateField(door, "doorRenderer", dSr);

        // Door frame arch wall on top
        GameObject archWall = new GameObject("DoorArchWall");
        archWall.transform.parent = geom.transform;
        archWall.transform.position = new Vector3(39.5f, 5.0f, 0f);
        var archCol = archWall.AddComponent<BoxCollider2D>();
        archCol.size = new Vector2(1.4f, 3.0f);
        var awSr = archWall.AddComponent<SpriteRenderer>();
        awSr.sprite = sprBrick;
        awSr.drawMode = SpriteDrawMode.Tiled;
        awSr.size = new Vector2(1.4f, 3.0f);
        awSr.sortingOrder = 9;

        // ============================================
        // TIME CRYSTAL INSIDE SECRET VAULT (X: 46.0, Y: 2.6)
        // ============================================
        GameObject crystalGo = new GameObject("TimeCrystal");
        crystalGo.transform.parent = geom.transform;
        crystalGo.transform.position = new Vector3(46.0f, 2.6f, 0f);

        var cSr = crystalGo.AddComponent<SpriteRenderer>();
        cSr.sprite = sprCrystal;
        cSr.sortingOrder = 15;

        var cCol = crystalGo.AddComponent<CircleCollider2D>();
        cCol.isTrigger = true;
        cCol.radius = 1.0f;

        crystal = crystalGo.AddComponent<TimeCrystal>();
        crystal.SetInitialPosition(new Vector3(46.0f, 2.6f, 0f));
        EditorUtility.SetDirty(crystal);

        // Crystal Pedestal
        GameObject dais = new GameObject("VaultPedestal");
        dais.transform.parent = geom.transform;
        dais.transform.position = new Vector3(46.0f, 1.6f, 0f);
        dais.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
        var daisSr = dais.AddComponent<SpriteRenderer>();
        daisSr.sprite = LoadSprite("spr_altar.png");
        daisSr.sortingOrder = 5;

        // Secret vault decorative bookshelves
        CreateDecoration(geom, sprShelf, new Vector3(42.5f, 2.2f, 0f), Vector3.one, 0);
        CreateDecoration(geom, sprShelf, new Vector3(49.5f, 2.2f, 0f), Vector3.one, 0);
    }

    private static void CreateCastleFloor(GameObject parent, Sprite sprBrick, float startX, float endX, float topY, float depth)
    {
        float width = endX - startX;
        float centerX = startX + width / 2f;
        float centerY = topY - depth / 2f;

        GameObject slab = new GameObject($"Floor_{startX:F0}_{endX:F0}");
        slab.transform.parent = parent.transform;
        slab.transform.position = new Vector3(centerX, centerY, 0f);

        var col = slab.AddComponent<BoxCollider2D>();
        col.size = new Vector2(width, depth);

        int count = Mathf.CeilToInt(width);
        for (int i = 0; i < count; i++)
        {
            float tx = startX + 0.5f + i;
            if (tx > endX) break;

            GameObject topTile = new GameObject("BrickTop_" + i);
            topTile.transform.parent = slab.transform;
            topTile.transform.position = new Vector3(tx, topY - 0.5f, 0f);
            var sr = topTile.AddComponent<SpriteRenderer>();
            sr.sprite = sprBrick;
            sr.sortingOrder = 0;

            GameObject subTile = new GameObject("BrickSub_" + i);
            subTile.transform.parent = slab.transform;
            subTile.transform.position = new Vector3(tx, topY - 1.5f, 0f);
            var srSub = subTile.AddComponent<SpriteRenderer>();
            srSub.sprite = sprBrick;
            srSub.sortingOrder = -5;
        }
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

        // 1. HUD Panel
        GameObject hud = CreateUIPanel(canvasGo, "HUDPanel", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, Color.clear);

        // Era Badge (Top-Left)
        GameObject eraBadge = CreateUIPanel(hud, "EraBadge", new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(420, 60), new Color(0.12f, 0.12f, 0.18f, 0.92f));
        eraBadge.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        CreateUIText(eraBadge, "EraText", "ERA: 1340 AD\nMEDIEVAL CASTLE LIBRARY", 18, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleLeft);

        // Pause Button
        GameObject pauseBtn = CreateUIButton(hud, "PauseButton", "[ESC] PAUSE", new Vector2(0, 1), new Vector2(20, -90), new Vector2(140, 36), () => gm.PauseGame());
        pauseBtn.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

        // Lives Badge
        GameObject livesPanel = CreateUIPanel(hud, "LivesBadge", new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -135), new Vector2(160, 36), new Color(0.12f, 0.12f, 0.18f, 0.92f));
        livesPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        Text livesText = CreateUIText(livesPanel, "LivesText", "LIVES: 5 / 5", 18, new Color(0.2f, 0.95f, 0.4f), TextAnchor.MiddleCenter);

        // Objective Banner (Top-Center)
        GameObject objPanel = CreateUIPanel(hud, "ObjectivePanel", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -25), new Vector2(760, 50), new Color(0.08f, 0.1f, 0.15f, 0.92f));
        objPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        Text objText = CreateUIText(objPanel, "ObjectiveText", "OBJECTIVE: Find the hidden lever and discover the secret room.", 19, new Color(0.3f, 0.95f, 1f), TextAnchor.MiddleCenter);

        // Notification Banner
        GameObject notifPanel = CreateUIPanel(hud, "NotificationPanel", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -85), new Vector2(640, 42), new Color(0.15f, 0.2f, 0.28f, 0.95f));
        notifPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        Text notifText = CreateUIText(notifPanel, "NotificationText", "Level 2: Medieval Castle", 18, Color.white, TextAnchor.MiddleCenter);
        notifPanel.SetActive(false);

        // Interaction Prompt Box
        GameObject promptBox = CreateUIPanel(hud, "PromptBox", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(650, 75), new Color(0.08f, 0.12f, 0.18f, 0.95f));
        promptBox.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
        Text promptText = CreateUIText(promptBox, "PromptText", "Press [E] to Pull Hidden Lever", 22, new Color(1f, 0.9f, 0.3f), TextAnchor.MiddleCenter);
        promptBox.SetActive(false);

        // 2. Main Menu Panel
        GameObject menu = CreateUIPanel(canvasGo, "MainMenuPanel", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(0.05f, 0.07f, 0.11f, 0.96f));

        GameObject menuContent = CreateUIPanel(menu, "MenuContent", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 680), Color.clear);
        Text titleText = CreateUIText(menuContent, "Title", "CHRONO RIFT", 64, new Color(1f, 0.82f, 0.2f), TextAnchor.UpperCenter);
        titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);

        Text subTitle = CreateUIText(menuContent, "Subtitle", "RESTORE THE TIMELINE — LEVEL 2: MEDIEVAL CASTLE", 22, new Color(0.2f, 0.9f, 1f), TextAnchor.UpperCenter);
        subTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -95);

        GameObject storyBox = CreateUIPanel(menuContent, "StoryBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(680, 180), new Color(0.08f, 0.11f, 0.16f, 0.9f));
        CreateUIText(storyBox, "StoryText", "The timeline rupture extends into the Medieval Era (1340 AD)!\n\nA temporal crystal is locked inside the royal castle's secret archives. Search the vast castle library, pull the hidden lever concealed among the ancient tomes, open the secret vault, and recover the Time Crystal!", 19, new Color(0.9f, 0.95f, 1f), TextAnchor.MiddleCenter);

        CreateUIButton(menuContent, "StartBtn", "START LEVEL 2", new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(320, 54), () => gm.StartGame());
        CreateUIButton(menuContent, "HowToPlayBtn", "HOW TO PLAY", new Vector2(0.5f, 0.5f), new Vector2(0, -180), new Vector2(320, 54), () => ui.ShowHowToPlay(true));
        CreateUIButton(menuContent, "QuitBtn", "QUIT GAME", new Vector2(0.5f, 0.5f), new Vector2(0, -250), new Vector2(320, 54), () => gm.QuitGame());

        // 3. How to Play Modal
        GameObject howToPlay = CreateUIPanel(canvasGo, "HowToPlayPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(750, 600), new Color(0.07f, 0.09f, 0.14f, 0.98f));
        CreateUIText(howToPlay, "HtpTitle", "MISSION MANUAL & CONTROLS", 32, new Color(1f, 0.85f, 0.2f), TextAnchor.UpperCenter);

        GameObject htpBody = CreateUIPanel(howToPlay, "HtpBody", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 15), new Vector2(680, 420), Color.clear);
        string htpContent = "CONTROLS:\n" +
            "  • [A] / [D] or [Left / Right Arrows] : Move Left & Right\n" +
            "  • [Space] / [W] / [Up Arrow] : Jump (Hold for higher jump)\n" +
            "  • [E] : Pull Hidden Lever\n" +
            "  • [Escape] : Pause Game & View Menu\n\n" +
            "MISSION OBJECTIVES:\n" +
            "  1. Traverse the castle library shelves and avoid spike hazards.\n" +
            "  2. You have 5 LIVES per level. Falling into spikes costs 1 life.\n" +
            "  3. Ascend to the high library gallery to find the hidden lever.\n" +
            "  4. Press [E] to pull the lever and slide open the secret vault door.\n" +
            "  5. Enter the secret room and recover the Time Crystal!";
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
        CreateUIText(victory, "VicSub", "Level 2 Complete — Medieval Era Anomaly Neutralized", 20, new Color(0.3f, 0.9f, 1f), TextAnchor.UpperCenter).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -70);

        GameObject statsBox = CreateUIPanel(victory, "StatsBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(580, 170), new Color(0.1f, 0.14f, 0.2f, 0.9f));
        Text vicStatsText = CreateUIText(statsBox, "StatsText", "TIME: 00:00\nLIVES REMAINING: 5 / 5\nSECRET ROOM: UNLOCKED\nTIME CRYSTAL: RECOVERED\nTIMELINE STABILITY: 100%", 22, Color.white, TextAnchor.MiddleCenter);

        GameObject nextBtn = CreateUIButton(victory, "NextLevelBtn", "NEXT LEVEL", new Vector2(0.5f, 0.5f), new Vector2(0, -85), new Vector2(280, 52), () => ui.OnNextLevelClicked());
        CreateUIButton(victory, "PlayAgainBtn", "PLAY AGAIN", new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(280, 52), () => gm.RestartGame());
        CreateUIButton(victory, "VicMenuBtn", "MAIN MENU", new Vector2(0.5f, 0.5f), new Vector2(0, -215), new Vector2(280, 52), () => gm.GoToMainMenu());
        victory.SetActive(false);

        // Wire references to UIManager
        SetPrivateField(ui, "hudPanel", hud);
        SetPrivateField(ui, "mainMenuPanel", menu);
        SetPrivateField(ui, "pauseMenuPanel", pause);
        SetPrivateField(ui, "howToPlayPanel", howToPlay);
        SetPrivateField(ui, "victoryPanel", victory);

        SetPrivateField(ui, "livesText", livesText);
        SetPrivateField(ui, "nextLevelBtn", nextBtn);
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
        txt.color = new Color(1f, 0.92f, 0.5f);
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
