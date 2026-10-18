using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Collections.Generic;

public static class LevelBuilderEditor
{
    private static Font defaultFont;
    private static Sprite sprPanel;
    private static Sprite sprButton;

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ChronoRift/Sprites/" + name);
    }

    [MenuItem("ChronoRift/Build Complete Level")]
    public static void BuildLevel()
    {
        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        sprPanel = LoadSprite("spr_ui_panel.png");
        sprButton = LoadSprite("spr_ui_button.png");

        // 1. Clean existing level roots if any
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var r in roots)
        {
            if (r.name != "Main Camera" && r.name != "Global Light 2D")
            {
                Object.DestroyImmediate(r);
            }
        }

        // Setup Main Camera
        SetupCamera();

        // Setup Managers
        SetupManagers(out GameManager gm, out SoundManager sm, out UIManager ui);

        // Setup Background & Parallax
        SetupBackground();

        // Setup Level Geometry & Platforms
        SetupGeometry();

        // Setup Altar & Temple Gate & Time Crystal
        SetupTempleAndAltar(out Altar altar, out TempleGate gate, out TimeCrystal crystal);

        // Setup Collectibles (Eggs)
        SetupEggs();

        // Setup Player
        SetupPlayer(out PlayerController player);

        // Setup UI Canvas
        SetupUI(gm, ui);

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Chrono Rift: Level 1 built and saved successfully!");
    }

    private static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }

        cam.orthographic = true;
        cam.orthographicSize = 5.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.1f, 0.16f);
        cam.transform.position = new Vector3(1f, 2f, -10f);

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();

        // Ensure AudioListener
        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();
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

        // 1. Sky layer
        Sprite sprSky = LoadSprite("spr_bg_sky.png");
        GameObject skyObj = new GameObject("SkyLayer");
        skyObj.transform.parent = bgRoot.transform;
        for (int i = -1; i <= 3; i++)
        {
            GameObject skyTile = new GameObject("Sky_" + i);
            skyTile.transform.parent = skyObj.transform;
            skyTile.transform.position = new Vector3(i * 24f, 10f, 0f);
            skyTile.transform.localScale = new Vector3(6f, 4f, 1f);
            var sr = skyTile.AddComponent<SpriteRenderer>();
            sr.sprite = sprSky;
            sr.sortingOrder = -100;
        }

        // 2. Mountains layer (Parallax factor 0.2)
        Sprite sprMtn = LoadSprite("spr_bg_mountains.png");
        GameObject mtnObj = new GameObject("MountainLayer");
        mtnObj.transform.parent = bgRoot.transform;
        var pMtn = mtnObj.AddComponent<ParallaxLayer>();
        SetPrivateField(pMtn, "parallaxFactorX", 0.18f);
        SetPrivateField(pMtn, "parallaxFactorY", 0.08f);

        for (int i = -1; i <= 4; i++)
        {
            GameObject mtnTile = new GameObject("Mountain_" + i);
            mtnTile.transform.parent = mtnObj.transform;
            mtnTile.transform.position = new Vector3(i * 20f, 3.5f, 0f);
            mtnTile.transform.localScale = new Vector3(5f, 3.5f, 1f);
            var sr = mtnTile.AddComponent<SpriteRenderer>();
            sr.sprite = sprMtn;
            sr.sortingOrder = -50;
        }

        // 3. Canopy layer (Parallax factor 0.45)
        Sprite sprCan = LoadSprite("spr_bg_canopy.png");
        GameObject canObj = new GameObject("CanopyLayer");
        canObj.transform.parent = bgRoot.transform;
        var pCan = canObj.AddComponent<ParallaxLayer>();
        SetPrivateField(pCan, "parallaxFactorX", 0.45f);
        SetPrivateField(pCan, "parallaxFactorY", 0.12f);

        for (int i = -1; i <= 5; i++)
        {
            GameObject canTile = new GameObject("Canopy_" + i);
            canTile.transform.parent = canObj.transform;
            canTile.transform.position = new Vector3(i * 18f, 5.5f, 0f);
            canTile.transform.localScale = new Vector3(4.5f, 3f, 1f);
            var sr = canTile.AddComponent<SpriteRenderer>();
            sr.sprite = sprCan;
            sr.sortingOrder = -30;
        }
    }

    private static void SetupGeometry()
    {
        GameObject geom = new GameObject("_LevelGeometry");
        Sprite sprTop = LoadSprite("spr_ground_top.png");
        Sprite sprStone = LoadSprite("spr_ground_stone.png");
        Sprite sprPlat = LoadSprite("spr_platform.png");
        Sprite sprSpike = LoadSprite("spr_hazard_spikes.png");
        Sprite sprPillar = LoadSprite("spr_ruin_pillar.png");
        Sprite sprFern = LoadSprite("spr_fern.png");
        Sprite sprShroom = LoadSprite("spr_mushroom.png");
        Sprite sprVine = LoadSprite("spr_vine.png");

        // Ground Section 1: Starting Clearing (X: -2 to 12, Y: 0)
        CreateGroundSlab(geom, sprTop, sprStone, -2f, 12f, 0f, 3f);

        // Chasm 1 (X: 12 to 16): Spikes at bottom Y: -3.5
        CreateSpikeHazard(geom, sprSpike, 12f, 16f, -3.2f);

        // Ground Section 2: Stepping Grove (X: 16 to 25, Y: 0)
        CreateGroundSlab(geom, sprTop, sprStone, 16f, 25f, 0f, 3f);

        // Ground Section 3: Sunken Ruins (X: 25 to 31, Y: -0.5)
        CreateGroundSlab(geom, sprTop, sprStone, 25f, 31f, -0.5f, 3f);

        // Chasm 2 (X: 31 to 35.5): Spikes at bottom Y: -3.5
        CreateSpikeHazard(geom, sprSpike, 31f, 35.5f, -3.2f);

        // Ground Section 4: Altar Plateau (X: 35.5 to 52, Y: 1.5)
        CreateGroundSlab(geom, sprTop, sprStone, 35.5f, 52f, 1.5f, 3.5f);

        // Ground Section 5: Temple Sanctum Floor (X: 52 to 66, Y: 1.5)
        CreateGroundSlab(geom, sprTop, sprStone, 52f, 66f, 1.5f, 3.5f);

        // Bottom Fall Hazard Barrier (Y: -8)
        GameObject fallDeath = new GameObject("FallDeathBarrier");
        fallDeath.transform.parent = geom.transform;
        fallDeath.transform.position = new Vector3(30f, -8f, 0f);
        fallDeath.tag = "Hazard";
        var deathCol = fallDeath.AddComponent<BoxCollider2D>();
        deathCol.size = new Vector2(100f, 2f);
        deathCol.isTrigger = true;

        // Floating Platforms
        // Zone 2: Canopy Ascent to Emerald Egg
        CreatePlatform(geom, sprPlat, 13.5f, 1.8f);
        CreatePlatform(geom, sprPlat, 17.0f, 3.4f);
        CreatePlatform(geom, sprPlat, 20.5f, 5.0f);
        CreatePlatform(geom, sprPlat, 24.0f, 6.6f); // Top Canopy: Emerald Egg here

        // Stepping return platform
        CreatePlatform(geom, sprPlat, 21.0f, 1.8f);

        // Zone 3: Sunken Ruins Crossing to Ruby Egg
        CreatePlatform(geom, sprPlat, 28.5f, 2.2f);
        CreatePlatform(geom, sprPlat, 33.5f, 2.8f);
        CreatePlatform(geom, sprPlat, 33.5f, 5.8f); // High Monolith: Ruby Egg here
        CreatePlatform(geom, sprPlat, 30.0f, 4.2f);

        // Zone 4: Ascent to Sapphire Egg (near Temple)
        CreatePlatform(geom, sprPlat, 39.0f, 3.6f);
        CreatePlatform(geom, sprPlat, 42.5f, 5.2f);
        CreatePlatform(geom, sprPlat, 46.5f, 6.8f);
        CreatePlatform(geom, sprPlat, 50.0f, 8.0f); // High Celestial Branch: Sapphire Egg here

        // Pillars
        CreateDecoration(geom, sprPillar, new Vector3(5f, 2.0f, 0f), new Vector3(1.2f, 1.2f, 1f), -10);
        CreateDecoration(geom, sprPillar, new Vector3(18f, 2.0f, 0f), new Vector3(1.2f, 1.2f, 1f), -10);
        CreateDecoration(geom, sprPillar, new Vector3(33.5f, 2.8f, 0f), new Vector3(1.2f, 2.0f, 1f), -10);
        CreateDecoration(geom, sprPillar, new Vector3(41f, 3.5f, 0f), new Vector3(1.2f, 1.5f, 1f), -10);
        CreateDecoration(geom, sprPillar, new Vector3(52.5f, 3.5f, 0f), new Vector3(1.2f, 1.5f, 1f), -10);

        // Ferns & Flora
        float[] fernX = { 0f, 4f, 8f, 17f, 23f, 27f, 37f, 44f, 48f, 63f };
        foreach (float fx in fernX)
        {
            float fy = (fx >= 35.5f) ? 2.0f : (fx >= 25f ? 0.0f : 0.5f);
            CreateDecoration(geom, sprFern, new Vector3(fx, fy, 0f), Vector3.one, 20);
        }

        // Bioluminescent Mushrooms
        Vector3[] shroomPositions = {
            new Vector3(2.5f, 0.7f, 0f),
            new Vector3(24.8f, 7.2f, 0f),
            new Vector3(34.2f, 6.4f, 0f),
            new Vector3(50.8f, 8.6f, 0f),
            new Vector3(45.0f, 2.2f, 0f),
            new Vector3(58.5f, 2.2f, 0f)
        };
        foreach (var spos in shroomPositions)
        {
            CreateDecoration(geom, sprShroom, spos, Vector3.one, 15);
        }

        // Hanging vines
        Vector3[] vinePositions = {
            new Vector3(13.5f, 1.3f, 0f),
            new Vector3(20.5f, 4.5f, 0f),
            new Vector3(33.5f, 5.3f, 0f),
            new Vector3(46.5f, 6.3f, 0f),
            new Vector3(55.0f, 4.5f, 0f)
        };
        foreach (var vpos in vinePositions)
        {
            CreateDecoration(geom, sprVine, vpos, Vector3.one, -5);
        }
    }

    private static void CreateGroundSlab(GameObject parent, Sprite sprTop, Sprite sprStone, float startX, float endX, float topY, float depth)
    {
        float width = endX - startX;
        float centerX = startX + width / 2f;
        float centerY = topY - depth / 2f;

        GameObject slab = new GameObject($"Ground_{startX:F0}_{endX:F0}");
        slab.transform.parent = parent.transform;
        slab.transform.position = new Vector3(centerX, centerY, 0f);

        var col = slab.AddComponent<BoxCollider2D>();
        col.size = new Vector2(width, depth);

        // Tile visual top sprites
        int tileCount = Mathf.CeilToInt(width);
        for (int i = 0; i < tileCount; i++)
        {
            float tx = startX + 0.5f + i;
            if (tx > endX) break;

            GameObject topTile = new GameObject("Top_" + i);
            topTile.transform.parent = slab.transform;
            topTile.transform.position = new Vector3(tx, topY - 0.5f, 0f);
            var sr = topTile.AddComponent<SpriteRenderer>();
            sr.sprite = sprTop;
            sr.sortingOrder = 0;

            // Stone below
            GameObject subTile = new GameObject("Stone_" + i);
            subTile.transform.parent = slab.transform;
            subTile.transform.position = new Vector3(tx, topY - 1.5f, 0f);
            var srSub = subTile.AddComponent<SpriteRenderer>();
            srSub.sprite = sprStone;
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

    private static void SetupTempleAndAltar(out Altar altar, out TempleGate gate, out TimeCrystal crystal)
    {
        GameObject ruinsRoot = new GameObject("_AncientRuins");

        // 1. Altar (At X: 45.5, Y: 2.1)
        GameObject altarGo = new GameObject("AncientAltar");
        altarGo.transform.parent = ruinsRoot.transform;
        altarGo.transform.position = new Vector3(45.5f, 2.15f, 0f);

        var aSr = altarGo.AddComponent<SpriteRenderer>();
        aSr.sprite = LoadSprite("spr_altar.png");
        aSr.sortingOrder = 5;

        var aCol = altarGo.AddComponent<CircleCollider2D>();
        aCol.isTrigger = true;
        aCol.radius = 2.4f;

        altar = altarGo.AddComponent<Altar>();

        // 3 Visual Sockets on Altar
        GameObject[] sockets = new GameObject[3];
        Sprite[] eggSprites = { LoadSprite("spr_egg_emerald.png"), LoadSprite("spr_egg_ruby.png"), LoadSprite("spr_egg_sapphire.png") };
        float[] sockOffsets = { -0.45f, 0f, 0.45f };

        for (int i = 0; i < 3; i++)
        {
            GameObject sGo = new GameObject("SocketEgg_" + i);
            sGo.transform.parent = altarGo.transform;
            sGo.transform.localPosition = new Vector3(sockOffsets[i], 0.35f, 0f);
            sGo.transform.localScale = Vector3.one * 0.55f;
            var sr = sGo.AddComponent<SpriteRenderer>();
            sr.sprite = eggSprites[i];
            sr.sortingOrder = 6;
            sGo.SetActive(false);
            sockets[i] = sGo;
        }
        SetPrivateField(altar, "eggSockets", sockets);

        // 2. Temple Arch & Gate (At X: 54.5, Y: 3.0)
        GameObject templeArch = new GameObject("TempleArch");
        templeArch.transform.parent = ruinsRoot.transform;
        templeArch.transform.position = new Vector3(54.5f, 3.0f, 0f);
        var archSr = templeArch.AddComponent<SpriteRenderer>();
        archSr.sprite = LoadSprite("spr_temple_arch.png");
        archSr.sortingOrder = 6;

        // Left pillar collider
        var leftCol = templeArch.AddComponent<BoxCollider2D>();
        leftCol.size = new Vector2(0.5f, 3.0f);
        leftCol.offset = new Vector2(-0.85f, 0f);

        // Right pillar collider
        var rightCol = templeArch.AddComponent<BoxCollider2D>();
        rightCol.size = new Vector2(0.5f, 3.0f);
        rightCol.offset = new Vector2(0.85f, 0f);

        // Gate barrier
        GameObject gateGo = new GameObject("TempleGate");
        gateGo.transform.parent = ruinsRoot.transform;
        gateGo.transform.position = new Vector3(54.5f, 2.75f, 0f);
        var gSr = gateGo.AddComponent<SpriteRenderer>();
        gSr.sprite = LoadSprite("spr_temple_gate.png");
        gSr.sortingOrder = 7;

        var gCol = gateGo.AddComponent<BoxCollider2D>();
        gCol.size = new Vector2(1.2f, 2.4f);

        gate = gateGo.AddComponent<TempleGate>();
        SetPrivateField(gate, "gateCollider", gCol);
        SetPrivateField(gate, "gateRenderer", gSr);

        // 3. Time Crystal (Inside Sanctum At X: 60.5, Y: 2.8)
        GameObject crystalGo = new GameObject("TimeCrystal");
        crystalGo.transform.parent = ruinsRoot.transform;
        crystalGo.transform.position = new Vector3(60.5f, 2.8f, 0f);

        var cSr = crystalGo.AddComponent<SpriteRenderer>();
        cSr.sprite = LoadSprite("spr_time_crystal.png");
        cSr.sortingOrder = 15;

        var cCol = crystalGo.AddComponent<CircleCollider2D>();
        cCol.isTrigger = true;
        cCol.radius = 1.0f;

        crystal = crystalGo.AddComponent<TimeCrystal>();
        crystal.SetInitialPosition(new Vector3(60.5f, 2.8f, 0f));
        UnityEditor.EditorUtility.SetDirty(crystal);

        // Sanctuary Dais pedestal beneath crystal
        GameObject dais = new GameObject("CrystalPedestal");
        dais.transform.parent = ruinsRoot.transform;
        dais.transform.position = new Vector3(60.5f, 1.8f, 0f);
        dais.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
        var dSr = dais.AddComponent<SpriteRenderer>();
        dSr.sprite = LoadSprite("spr_altar.png");
        dSr.sortingOrder = 5;
    }

    private static void SetupEggs()
    {
        GameObject eggRoot = new GameObject("_Collectibles");

        // Egg 0: Emerald (Canopy Peak X: 24.0, Y: 7.4)
        CreateEgg(eggRoot, 0, "Emerald Dragon Egg", LoadSprite("spr_egg_emerald.png"), new Vector3(24.0f, 7.4f, 0f));

        // Egg 1: Ruby (Ruins High Pillar X: 33.5, Y: 6.6)
        CreateEgg(eggRoot, 1, "Ruby Dragon Egg", LoadSprite("spr_egg_ruby.png"), new Vector3(33.5f, 6.6f, 0f));

        // Egg 2: Sapphire (Celestial Apex X: 50.0, Y: 8.8)
        CreateEgg(eggRoot, 2, "Sapphire Dragon Egg", LoadSprite("spr_egg_sapphire.png"), new Vector3(50.0f, 8.8f, 0f));
    }

    private static void CreateEgg(GameObject parent, int id, string name, Sprite spr, Vector3 pos)
    {
        GameObject eggGo = new GameObject("DragonEgg_" + id);
        eggGo.transform.parent = parent.transform;
        eggGo.transform.position = pos;

        var sr = eggGo.AddComponent<SpriteRenderer>();
        sr.sprite = spr;
        sr.sortingOrder = 15;

        var col = eggGo.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var egg = eggGo.AddComponent<DragonEgg>();
        SetPrivateField(egg, "eggId", id);
        SetPrivateField(egg, "eggName", name);
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
        col.offset = new Vector2(0f, 0f);

        player = pGo.AddComponent<PlayerController>();
        player.idleSprite = LoadSprite("spr_player_idle.png");
        player.walk1Sprite = LoadSprite("spr_player_walk1.png");
        player.walk2Sprite = LoadSprite("spr_player_walk2.png");
        player.jumpSprite = LoadSprite("spr_player_jump.png");

        // Attach CameraFollow target
        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.SetTarget(pGo.transform);
        }
    }

    private static void SetupUI(GameManager gm, UIManager ui)
    {
        // Canvas
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // EventSystem
        GameObject esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        // Add InputSystemUIInputModule or StandaloneInputModule
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
        GameObject eraBadge = CreateUIPanel(hud, "EraBadge", new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(420, 60), new Color(0.1f, 0.14f, 0.2f, 0.9f));
        eraBadge.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        CreateUIText(eraBadge, "EraText", "ERA: 65,000,000 BC\nPREHISTORIC JUNGLE", 18, Color.yellow, TextAnchor.MiddleLeft);

        // Pause button (below Era badge)
        GameObject pauseBtn = CreateUIButton(hud, "PauseButton", "[ESC] PAUSE", new Vector2(0, 1), new Vector2(20, -90), new Vector2(140, 36), () => gm.PauseGame());
        pauseBtn.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

        GameObject livesPanel = CreateUIPanel(hud, "LivesBadge", new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -135), new Vector2(160, 36), new Color(0.1f, 0.14f, 0.2f, 0.9f));
        livesPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        Text livesText = CreateUIText(livesPanel, "LivesText", "LIVES: 5 / 5", 18, new Color(0.2f, 0.95f, 0.4f), TextAnchor.MiddleCenter);

        // Egg HUD (Top-Right)
        GameObject eggPanel = CreateUIPanel(hud, "EggPanel", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(280, 70), new Color(0.1f, 0.14f, 0.2f, 0.9f));
        eggPanel.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

        Text eggCounterText = CreateUIText(eggPanel, "EggCounterText", "EGGS: 0 / 3", 22, new Color(1f, 0.85f, 0.3f), TextAnchor.UpperCenter);
        eggCounterText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);

        Image[] eggIcons = new Image[3];
        Sprite[] eggSprites = { LoadSprite("spr_egg_emerald.png"), LoadSprite("spr_egg_ruby.png"), LoadSprite("spr_egg_sapphire.png") };
        for (int i = 0; i < 3; i++)
        {
            GameObject iconGo = new GameObject("EggIcon_" + i);
            iconGo.transform.parent = eggPanel.transform;
            var rt = iconGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(26, 32);
            rt.anchoredPosition = new Vector2((i - 1) * 45f, 6f);
            var img = iconGo.AddComponent<Image>();
            img.sprite = eggSprites[i];
            img.color = new Color(0.25f, 0.25f, 0.25f, 0.6f);
            eggIcons[i] = img;
        }

        // Objective Banner (Top-Center)
        GameObject objPanel = CreateUIPanel(hud, "ObjectivePanel", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -25), new Vector2(720, 50), new Color(0.08f, 0.12f, 0.18f, 0.92f));
        objPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        Text objText = CreateUIText(objPanel, "ObjectiveText", "OBJECTIVE: Search the prehistoric jungle for the 3 Sacred Dragon Eggs.", 19, new Color(0.3f, 0.95f, 1f), TextAnchor.MiddleCenter);

        // Notification Banner (Upper Center)
        GameObject notifPanel = CreateUIPanel(hud, "NotificationPanel", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -85), new Vector2(620, 42), new Color(0.12f, 0.25f, 0.22f, 0.95f));
        notifPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        Text notifText = CreateUIText(notifPanel, "NotificationText", "Mission Started!", 18, Color.white, TextAnchor.MiddleCenter);
        notifPanel.SetActive(false);

        // Interaction Prompt Box (Bottom-Center)
        GameObject promptBox = CreateUIPanel(hud, "PromptBox", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(650, 75), new Color(0.08f, 0.14f, 0.22f, 0.95f));
        promptBox.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
        Text promptText = CreateUIText(promptBox, "PromptText", "Press [E] to Interact", 22, new Color(1f, 0.9f, 0.3f), TextAnchor.MiddleCenter);
        promptBox.SetActive(false);

        // 2. Main Menu Panel (Fullscreen)
        GameObject menu = CreateUIPanel(canvasGo, "MainMenuPanel", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(0.05f, 0.08f, 0.12f, 0.96f));

        GameObject menuContent = CreateUIPanel(menu, "MenuContent", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 680), Color.clear);
        Text titleText = CreateUIText(menuContent, "Title", "CHRONO RIFT", 64, new Color(1f, 0.82f, 0.2f), TextAnchor.UpperCenter);
        titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);

        Text subTitle = CreateUIText(menuContent, "Subtitle", "RESTORE THE TIMELINE — LEVEL 1: PREHISTORIC JUNGLE", 22, new Color(0.2f, 0.9f, 1f), TextAnchor.UpperCenter);
        subTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -95);

        GameObject storyBox = CreateUIPanel(menuContent, "StoryBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(680, 180), new Color(0.08f, 0.12f, 0.18f, 0.9f));
        CreateUIText(storyBox, "StoryText", "A catastrophic temporal rupture has scattered the legendary Time Crystals across history!\n\nAs the Chrono Agent, enter the prehistoric epoch, explore the perilous jungle, gather all 3 Sacred Dragon Eggs, activate the Ancient Altar, and reclaim the Time Crystal before the timeline shatters!", 19, new Color(0.9f, 0.95f, 1f), TextAnchor.MiddleCenter);

        CreateUIButton(menuContent, "StartBtn", "START ADVENTURE", new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(320, 54), () => gm.StartGame());
        CreateUIButton(menuContent, "HowToPlayBtn", "HOW TO PLAY", new Vector2(0.5f, 0.5f), new Vector2(0, -180), new Vector2(320, 54), () => ui.ShowHowToPlay(true));
        CreateUIButton(menuContent, "QuitBtn", "QUIT GAME", new Vector2(0.5f, 0.5f), new Vector2(0, -250), new Vector2(320, 54), () => gm.QuitGame());

        // 3. How to Play Panel (Modal)
        GameObject howToPlay = CreateUIPanel(canvasGo, "HowToPlayPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(750, 600), new Color(0.07f, 0.1f, 0.15f, 0.98f));
        CreateUIText(howToPlay, "HtpTitle", "MISSION MANUAL & CONTROLS", 32, new Color(1f, 0.85f, 0.2f), TextAnchor.UpperCenter);

        GameObject htpBody = CreateUIPanel(howToPlay, "HtpBody", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 15), new Vector2(680, 420), Color.clear);
        string htpContent = "CONTROLS:\n" +
            "  • [A] / [D] or [Left / Right Arrows] : Move Left & Right\n" +
            "  • [Space] / [W] / [Up Arrow] : Jump (Hold for higher jump)\n" +
            "  • [E] : Interact with Ancient Altar\n" +
            "  • [Escape] : Pause Game & View Menu\n\n" +
            "MISSION OBJECTIVES:\n" +
            "  1. Traverse the prehistoric canopy platforms and chasms.\n" +
            "  2. Find and collect all 3 Sacred Dragon Eggs (Emerald, Ruby, Sapphire).\n" +
            "  3. Bring the eggs to the Ancient Altar and press [E] to unlock the Temple.\n" +
            "  4. Enter the temple and recover the glowing Time Crystal to save the timeline!";
        CreateUIText(htpBody, "HtpText", htpContent, 20, Color.white, TextAnchor.MiddleLeft);

        CreateUIButton(howToPlay, "CloseHtpBtn", "CLOSE MANUAL", new Vector2(0.5f, 0), new Vector2(0, 35), new Vector2(240, 48), () => ui.ShowHowToPlay(false));
        howToPlay.SetActive(false);

        // 4. Pause Menu Panel
        GameObject pause = CreateUIPanel(canvasGo, "PauseMenuPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 480), new Color(0.06f, 0.09f, 0.14f, 0.98f));
        CreateUIText(pause, "PauseTitle", "CHRONO FIELD PAUSED", 32, new Color(1f, 0.85f, 0.2f), TextAnchor.UpperCenter);

        CreateUIButton(pause, "ResumeBtn", "RESUME GAME", new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(280, 50), () => gm.ResumeGame());
        CreateUIButton(pause, "PauseHtpBtn", "HOW TO PLAY", new Vector2(0.5f, 0.5f), new Vector2(0, -5), new Vector2(280, 50), () => ui.ShowHowToPlay(true));
        CreateUIButton(pause, "RestartBtn", "RESTART LEVEL", new Vector2(0.5f, 0.5f), new Vector2(0, -70), new Vector2(280, 50), () => gm.RestartGame());
        CreateUIButton(pause, "MainMenuBtn", "MAIN MENU", new Vector2(0.5f, 0.5f), new Vector2(0, -135), new Vector2(280, 50), () => gm.GoToMainMenu());
        pause.SetActive(false);

        // 5. Victory Panel
        GameObject victory = CreateUIPanel(canvasGo, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 540), new Color(0.06f, 0.12f, 0.16f, 0.98f));
        CreateUIText(victory, "VicTitle", "TIMELINE RESTORED!", 44, new Color(0.2f, 0.95f, 0.4f), TextAnchor.UpperCenter);
        CreateUIText(victory, "VicSub", "Level 1 Complete — Prehistoric Epoch Stabilized", 20, new Color(0.3f, 0.9f, 1f), TextAnchor.UpperCenter).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -70);

        GameObject statsBox = CreateUIPanel(victory, "StatsBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 15), new Vector2(580, 170), new Color(0.1f, 0.16f, 0.22f, 0.9f));
        Text vicStatsText = CreateUIText(statsBox, "StatsText", "TIME: 00:00\nSACRED DRAGON EGGS: 3 / 3\nTIME CRYSTAL: RECOVERED\nTIMELINE STABILITY: 100%", 22, Color.white, TextAnchor.MiddleCenter);

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
        SetPrivateField(ui, "eggCounterText", eggCounterText);
        SetPrivateField(ui, "eggIcons", eggIcons);
        SetPrivateField(ui, "objectiveText", objText);
        SetPrivateField(ui, "promptBox", promptBox);
        SetPrivateField(ui, "promptText", promptText);
        SetPrivateField(ui, "notificationText", notifText);
        SetPrivateField(ui, "victoryStatsText", vicStatsText);
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

        // Child Text
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
