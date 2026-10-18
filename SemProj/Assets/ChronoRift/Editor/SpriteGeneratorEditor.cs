using UnityEngine;
using UnityEditor;
using System.IO;

public static class SpriteGeneratorEditor
{
    public static void SaveSprite(string path, Texture2D tex, int ppu = 32)
    {
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    public static void Clear(Texture2D tex, Color c)
    {
        Color[] cols = new Color[tex.width * tex.height];
        for (int i = 0; i < cols.Length; i++) cols[i] = c;
        tex.SetPixels(cols);
    }

    public static void DrawRect(Texture2D tex, int x0, int y0, int w, int h, Color c)
    {
        for (int x = x0; x < x0 + w; x++)
        {
            for (int y = y0; y < y0 + h; y++)
            {
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    tex.SetPixel(x, y, c);
            }
        }
    }

    [MenuItem("ChronoRift/Generate Sprites")]
    public static void GenerateAll()
    {
        string dir = "Assets/ChronoRift/Sprites/";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        GenPlayer(dir);
        GenGround(dir);
        GenPlatforms(dir);
        GenRuinsAndTemple(dir);
        GenCollectibles(dir);
        GenDecorations(dir);
        GenBackgrounds(dir);
        GenUI(dir);

        AssetDatabase.Refresh();
        Debug.Log("Chrono Rift: Sprites generated successfully!");
    }

    private static void GenPlayer(string dir)
    {
        Texture2D idle = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Clear(idle, Color.clear);
        DrawPlayerBase(idle, 0, 0, 0);
        SaveSprite(dir + "spr_player_idle.png", idle, 32);

        Texture2D walk1 = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Clear(walk1, Color.clear);
        DrawPlayerBase(walk1, -2, 2, 1);
        SaveSprite(dir + "spr_player_walk1.png", walk1, 32);

        Texture2D walk2 = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Clear(walk2, Color.clear);
        DrawPlayerBase(walk2, 2, -2, -1);
        SaveSprite(dir + "spr_player_walk2.png", walk2, 32);

        Texture2D jump = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Clear(jump, Color.clear);
        DrawPlayerBase(jump, 1, 1, 2);
        SaveSprite(dir + "spr_player_jump.png", jump, 32);
    }

    private static void DrawPlayerBase(Texture2D tex, int legOffsetL, int legOffsetR, int bob)
    {
        Color helmetDark = new Color(0.18f, 0.22f, 0.28f);
        Color helmetLight = new Color(0.32f, 0.38f, 0.48f);
        Color visorCyan = new Color(0.15f, 0.95f, 1f);
        Color coatDark = new Color(0.24f, 0.35f, 0.26f);
        Color coatLight = new Color(0.35f, 0.5f, 0.38f);
        Color beltGold = new Color(0.95f, 0.75f, 0.2f);
        Color pants = new Color(0.22f, 0.18f, 0.14f);
        Color boots = new Color(0.4f, 0.25f, 0.15f);
        Color chronogear = new Color(0f, 0.85f, 0.95f);

        int cy = bob;

        // Legs & Boots
        DrawRect(tex, 11 + legOffsetL, 3, 4, 7, pants);
        DrawRect(tex, 10 + legOffsetL, 1, 5, 3, boots);
        DrawRect(tex, 17 + legOffsetR, 3, 4, 7, pants);
        DrawRect(tex, 17 + legOffsetR, 1, 5, 3, boots);

        // Coat / Torso
        DrawRect(tex, 10, 10 + cy, 12, 10, coatDark);
        DrawRect(tex, 11, 11 + cy, 10, 8, coatLight);
        DrawRect(tex, 10, 9 + cy, 12, 2, beltGold);
        tex.SetPixel(15, 10 + cy, Color.white);

        // Chrono Pack (left side)
        DrawRect(tex, 7, 11 + cy, 3, 8, helmetDark);
        DrawRect(tex, 6, 13 + cy, 2, 4, chronogear);
        tex.SetPixel(7, 15 + cy, Color.white);

        // Helmet & Visor
        DrawRect(tex, 10, 20 + cy, 12, 10, helmetDark);
        DrawRect(tex, 11, 22 + cy, 10, 7, helmetLight);
        DrawRect(tex, 14, 23 + cy, 8, 4, visorCyan);
        tex.SetPixel(15, 25 + cy, Color.white);
        tex.SetPixel(16, 25 + cy, Color.white);

        // Scarf
        DrawRect(tex, 11, 19 + cy, 10, 2, beltGold);
        DrawRect(tex, 8, 17 + cy, 3, 3, beltGold);
    }

    private static void GenGround(string dir)
    {
        Texture2D top = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Clear(top, new Color(0.28f, 0.19f, 0.13f));
        Color grassDark = new Color(0.12f, 0.48f, 0.16f);
        Color grassMid = new Color(0.24f, 0.72f, 0.25f);
        Color grassLight = new Color(0.55f, 0.88f, 0.32f);
        Color dirtLight = new Color(0.38f, 0.26f, 0.18f);
        Color stoneSpeck = new Color(0.5f, 0.45f, 0.4f);

        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if ((x * 7 + y * 13) % 11 == 0) top.SetPixel(x, y, dirtLight);
                if ((x * 19 + y * 23) % 29 == 0) top.SetPixel(x, y, stoneSpeck);
            }
        }

        for (int x = 0; x < 32; x++)
        {
            int h = 25 + (int)(Mathf.Sin(x * 0.7f) * 2f + Mathf.Cos(x * 1.5f) * 1.5f);
            for (int y = h; y < 32; y++) top.SetPixel(x, y, grassMid);
            top.SetPixel(x, Mathf.Min(31, h + 3), grassLight);
            top.SetPixel(x, h, grassDark);
            if (x % 3 == 0 && h > 2)
            {
                top.SetPixel(x, h - 1, grassMid);
                top.SetPixel(x, h - 2, grassDark);
            }
        }
        SaveSprite(dir + "spr_ground_top.png", top, 32);

        Texture2D stone = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Clear(stone, new Color(0.22f, 0.16f, 0.12f));
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                if ((x + y * 2) % 6 == 0) stone.SetPixel(x, y, new Color(0.26f, 0.2f, 0.16f));
                if ((x * 3 + y * 7) % 17 == 0) stone.SetPixel(x, y, new Color(0.35f, 0.3f, 0.28f));
                if (x == 12 && y == 16) stone.SetPixel(x, y, new Color(0.95f, 0.65f, 0.1f));
                if (x == 13 && y == 16) stone.SetPixel(x, y, new Color(0.95f, 0.65f, 0.1f));
            }
        }
        SaveSprite(dir + "spr_ground_stone.png", stone, 32);
    }

    private static void GenPlatforms(string dir)
    {
        Texture2D plat = new Texture2D(64, 24, TextureFormat.RGBA32, false);
        Clear(plat, Color.clear);
        Color stoneDark = new Color(0.2f, 0.22f, 0.26f);
        Color stoneMid = new Color(0.32f, 0.36f, 0.42f);
        Color stoneLight = new Color(0.48f, 0.54f, 0.6f);
        Color runeCyan = new Color(0.2f, 0.85f, 0.95f);
        Color moss = new Color(0.35f, 0.72f, 0.28f);
        Color mossLight = new Color(0.58f, 0.88f, 0.35f);

        for (int x = 4; x < 60; x++)
            for (int y = 4; y < 20; y++)
                plat.SetPixel(x, y, stoneMid);

        DrawRect(plat, 4, 4, 56, 1, stoneDark);
        DrawRect(plat, 4, 19, 56, 1, stoneLight);
        DrawRect(plat, 3, 5, 1, 14, stoneDark);
        DrawRect(plat, 60, 5, 1, 14, stoneDark);

        for (int x = 8; x < 56; x++)
        {
            if (x % 6 == 0 || x % 6 == 1 || (x % 12 >= 8))
            {
                plat.SetPixel(x, 11, runeCyan);
                plat.SetPixel(x, 12, Color.white);
            }
            else plat.SetPixel(x, 11, stoneDark);
        }

        for (int x = 2; x < 62; x++)
        {
            int mh = 19 + (int)(Mathf.Sin(x * 0.5f) * 2f);
            plat.SetPixel(x, mh, moss);
            plat.SetPixel(x, mh + 1, mossLight);
            if (x % 5 == 0) plat.SetPixel(x, mh - 1, moss);
        }

        int[] vineX = { 10, 18, 35, 48 };
        int[] vineLen = { 6, 8, 5, 7 };
        for (int i = 0; i < vineX.Length; i++)
        {
            for (int vy = 0; vy < vineLen[i]; vy++)
            {
                plat.SetPixel(vineX[i], 4 - vy, moss);
                if (vy % 2 == 0) plat.SetPixel(vineX[i] + 1, 4 - vy, mossLight);
            }
        }
        SaveSprite(dir + "spr_platform.png", plat, 32);
    }

    private static void GenRuinsAndTemple(string dir)
    {
        Texture2D pillar = new Texture2D(32, 96, TextureFormat.RGBA32, false);
        Clear(pillar, Color.clear);
        Color sD = new Color(0.22f, 0.24f, 0.28f);
        Color sM = new Color(0.36f, 0.4f, 0.46f);
        Color sL = new Color(0.5f, 0.55f, 0.62f);
        Color rune = new Color(0.2f, 0.85f, 0.95f);
        Color moss = new Color(0.35f, 0.72f, 0.28f);

        DrawRect(pillar, 4, 0, 24, 96, sM);
        DrawRect(pillar, 2, 0, 28, 8, sD);
        DrawRect(pillar, 2, 88, 28, 8, sD);
        for (int y = 0; y < 96; y++)
        {
            pillar.SetPixel(4, y, sL);
            pillar.SetPixel(27, y, sD);
            if ((y + (int)(Mathf.Sin(y * 0.3f) * 6f)) % 16 == 0)
            {
                for (int rx = 8; rx < 24; rx++) pillar.SetPixel(rx, y, rune);
            }
        }
        for (int y = 10; y < 70; y++)
        {
            int vx = 14 + (int)(Mathf.Sin(y * 0.2f) * 8f);
            pillar.SetPixel(vx, y, moss);
            pillar.SetPixel(vx + 1, y, moss);
        }
        SaveSprite(dir + "spr_ruin_pillar.png", pillar, 32);

        Texture2D arch = new Texture2D(64, 96, TextureFormat.RGBA32, false);
        Clear(arch, Color.clear);
        DrawRect(arch, 0, 0, 16, 96, sM);
        DrawRect(arch, 0, 0, 3, 96, sL);
        DrawRect(arch, 14, 0, 2, 96, sD);
        DrawRect(arch, 48, 0, 16, 96, sM);
        DrawRect(arch, 48, 0, 3, 96, sL);
        DrawRect(arch, 62, 0, 2, 96, sD);
        DrawRect(arch, 0, 76, 64, 20, sD);
        DrawRect(arch, 2, 78, 60, 16, sM);
        for (int x = 24; x < 40; x++)
        {
            for (int y = 80; y < 92; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(32, 86));
                if (d < 5f) arch.SetPixel(x, y, rune);
                if (d < 2.5f) arch.SetPixel(x, y, Color.white);
            }
        }
        SaveSprite(dir + "spr_temple_arch.png", arch, 32);

        Texture2D door = new Texture2D(34, 76, TextureFormat.RGBA32, false);
        Clear(door, Color.clear);
        Color doorGlow = new Color(0.1f, 0.65f, 0.95f, 0.85f);
        Color doorCore = new Color(0.85f, 0.98f, 1f, 0.95f);
        for (int x = 0; x < 34; x++)
        {
            for (int y = 0; y < 76; y++)
            {
                float wave = Mathf.Sin(x * 0.4f + y * 0.3f);
                if (x == 0 || x == 33 || y == 0 || y == 75) door.SetPixel(x, y, doorCore);
                else if (wave > 0.4f) door.SetPixel(x, y, doorCore);
                else door.SetPixel(x, y, doorGlow);
            }
        }
        int[] sigilY = { 56, 38, 20 };
        foreach (int sy in sigilY)
        {
            for (int x = 12; x <= 22; x++)
            {
                for (int y = sy - 6; y <= sy + 6; y++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(17, sy));
                    if (Mathf.Abs(d - 5f) < 1.2f) door.SetPixel(x, y, Color.white);
                }
            }
        }
        SaveSprite(dir + "spr_temple_gate.png", door, 32);

        Texture2D altar = new Texture2D(48, 40, TextureFormat.RGBA32, false);
        Clear(altar, Color.clear);
        DrawRect(altar, 2, 0, 44, 10, sD);
        DrawRect(altar, 6, 10, 36, 12, sM);
        DrawRect(altar, 4, 22, 40, 10, sL);
        int[] sockX = { 10, 24, 38 };
        foreach (int sx in sockX)
        {
            for (int x = sx - 4; x <= sx + 4; x++)
            {
                for (int y = 26; y <= 34; y++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(sx, 30));
                    if (d < 4f) altar.SetPixel(x, y, new Color(0.12f, 0.14f, 0.18f));
                    if (Mathf.Abs(d - 3.8f) < 1f) altar.SetPixel(x, y, rune);
                }
            }
        }
        for (int x = 10; x <= 38; x++)
        {
            altar.SetPixel(x, 24, rune);
            altar.SetPixel(x, 25, Color.white);
        }
        SaveSprite(dir + "spr_altar.png", altar, 32);
    }

    private static void GenCollectibles(string dir)
    {
        GenEgg(dir + "spr_egg_emerald.png",
            new Color(0.08f, 0.45f, 0.2f),
            new Color(0.18f, 0.85f, 0.35f),
            new Color(0.6f, 1f, 0.5f),
            new Color(0.95f, 0.85f, 0.2f));

        GenEgg(dir + "spr_egg_ruby.png",
            new Color(0.5f, 0.08f, 0.15f),
            new Color(0.95f, 0.22f, 0.25f),
            new Color(1f, 0.6f, 0.3f),
            new Color(1f, 0.95f, 0.4f));

        GenEgg(dir + "spr_egg_sapphire.png",
            new Color(0.08f, 0.2f, 0.55f),
            new Color(0.2f, 0.55f, 0.98f),
            new Color(0.45f, 0.88f, 1f),
            new Color(0.9f, 0.95f, 1f));

        Texture2D crystal = new Texture2D(32, 48, TextureFormat.RGBA32, false);
        Clear(crystal, Color.clear);
        Vector2 c = new Vector2(16, 24);
        Color core = Color.white;
        Color cCyan = new Color(0.2f, 0.95f, 1f);
        Color cViolet = new Color(0.7f, 0.25f, 1f);
        Color cDeep = new Color(0.35f, 0.08f, 0.65f);

        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 48; y++)
            {
                float dx = Mathf.Abs(x - c.x) / 12f;
                float dy = Mathf.Abs(y - c.y) / 20f;
                if (dx + dy <= 1f)
                {
                    if (x < 16 && y > 24) crystal.SetPixel(x, y, cCyan);
                    else if (x >= 16 && y > 24) crystal.SetPixel(x, y, Color.Lerp(cCyan, core, 0.5f));
                    else if (x < 16 && y <= 24) crystal.SetPixel(x, y, cViolet);
                    else crystal.SetPixel(x, y, cDeep);

                    if (dx + dy < 0.35f) crystal.SetPixel(x, y, core);
                    if (x == 16 || y == 24 || Mathf.Abs(dx - dy) < 0.05f) crystal.SetPixel(x, y, Color.white);
                }
            }
        }
        SaveSprite(dir + "spr_time_crystal.png", crystal, 32);
    }

    private static void GenEgg(string path, Color baseDark, Color baseMid, Color highlight, Color runeCol)
    {
        Texture2D egg = new Texture2D(24, 28, TextureFormat.RGBA32, false);
        Clear(egg, Color.clear);
        Vector2 center = new Vector2(12, 12);

        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 28; y++)
            {
                float dx = (x - center.x) / 9f;
                float dy = (y - center.y);
                float radiusY = (y > center.y) ? 14f : 11f;
                float taper = 1f - 0.25f * (y / 28f);
                dx /= taper;
                dy /= radiusY;

                if (dx * dx + dy * dy <= 1f)
                {
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    Color col = (d < 0.6f && x > 9 && y > 12) ? baseMid : baseDark;
                    if (x > 12 && y > 15 && d < 0.45f) col = highlight;
                    if (((x * 5 + y * 7) % 9 == 0) && d > 0.3f) col = runeCol;
                    egg.SetPixel(x, y, col);
                }
            }
        }
        egg.SetPixel(15, 20, Color.white);
        egg.SetPixel(16, 20, Color.white);
        egg.SetPixel(15, 21, Color.white);
        SaveSprite(path, egg, 32);
    }

    private static void GenDecorations(string dir)
    {
        Texture2D fern = new Texture2D(48, 36, TextureFormat.RGBA32, false);
        Clear(fern, Color.clear);
        Color fg1 = new Color(0.15f, 0.55f, 0.2f);
        Color fg2 = new Color(0.35f, 0.85f, 0.3f);
        Color fg3 = new Color(0.6f, 0.95f, 0.4f);

        for (int frond = 0; frond < 7; frond++)
        {
            float angle = -60f + frond * 20f;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dirVec = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
            for (int step = 0; step < 26; step++)
            {
                Vector2 pos = new Vector2(24, 4) + dirVec * step;
                int px = Mathf.RoundToInt(pos.x);
                int py = Mathf.RoundToInt(pos.y);
                if (px >= 0 && px < 48 && py >= 0 && py < 36)
                {
                    fern.SetPixel(px, py, fg2);
                    if (step % 2 == 0)
                    {
                        if (px > 0) fern.SetPixel(px - 1, py, fg1);
                        if (px < 47) fern.SetPixel(px + 1, py, fg3);
                    }
                }
            }
        }
        SaveSprite(dir + "spr_fern.png", fern, 32);

        Texture2D shroom = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Clear(shroom, Color.clear);
        Color mCap = new Color(0.1f, 0.9f, 0.95f);
        Color mGlow = new Color(0.8f, 0.2f, 0.95f);
        Color mStem = new Color(0.85f, 0.9f, 0.95f);

        DrawRect(shroom, 14, 2, 4, 16, mStem);
        for (int x = 6; x <= 26; x++)
        {
            for (int y = 14; y <= 26; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                if (d < 10f && y >= 14) shroom.SetPixel(x, y, mCap);
            }
        }
        shroom.SetPixel(12, 22, mGlow);
        shroom.SetPixel(13, 22, Color.white);
        shroom.SetPixel(19, 20, mGlow);
        shroom.SetPixel(20, 20, Color.white);

        DrawRect(shroom, 4, 2, 3, 10, mStem);
        for (int x = 1; x <= 10; x++)
        {
            for (int y = 10; y <= 17; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(5, 11));
                if (d < 5f && y >= 10) shroom.SetPixel(x, y, mGlow);
            }
        }
        SaveSprite(dir + "spr_mushroom.png", shroom, 32);

        Texture2D vine = new Texture2D(16, 64, TextureFormat.RGBA32, false);
        Clear(vine, Color.clear);
        for (int y = 0; y < 64; y++)
        {
            int vx = 8 + (int)(Mathf.Sin(y * 0.15f) * 4f);
            vine.SetPixel(vx, y, fg1);
            vine.SetPixel(vx + 1, y, fg2);
            if (y % 8 == 0)
            {
                vine.SetPixel(vx - 2, y, fg3);
                vine.SetPixel(vx - 1, y, fg2);
                vine.SetPixel(vx + 2, y, fg3);
            }
        }
        SaveSprite(dir + "spr_vine.png", vine, 32);

        Texture2D spike = new Texture2D(32, 24, TextureFormat.RGBA32, false);
        Clear(spike, Color.clear);
        Color spDark = new Color(0.35f, 0.12f, 0.12f);
        Color spRed = new Color(0.85f, 0.22f, 0.18f);
        Color spTip = new Color(1f, 0.6f, 0.3f);
        for (int i = 0; i < 4; i++)
        {
            int cx = 4 + i * 8;
            for (int y = 0; y < 22; y++)
            {
                int w = (22 - y) / 3;
                for (int x = cx - w; x <= cx + w; x++)
                {
                    if (x >= 0 && x < 32)
                    {
                        if (y > 18) spike.SetPixel(x, y, spTip);
                        else if (x < cx) spike.SetPixel(x, y, spRed);
                        else spike.SetPixel(x, y, spDark);
                    }
                }
            }
        }
        SaveSprite(dir + "spr_hazard_spikes.png", spike, 32);

        Texture2D sparkle = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Clear(sparkle, Color.clear);
        for (int i = 0; i < 16; i++)
        {
            sparkle.SetPixel(8, i, new Color(1f, 1f, 1f, 1f - Mathf.Abs(i - 8) / 8f));
            sparkle.SetPixel(i, 8, new Color(1f, 1f, 1f, 1f - Mathf.Abs(i - 8) / 8f));
        }
        DrawRect(sparkle, 7, 7, 3, 3, Color.white);
        SaveSprite(dir + "spr_sparkle.png", sparkle, 32);
    }

    private static void GenBackgrounds(string dir)
    {
        Texture2D sky = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        Color skyTop = new Color(0.06f, 0.08f, 0.18f);
        Color skyMid = new Color(0.12f, 0.2f, 0.32f);
        Color skyBottom = new Color(0.18f, 0.38f, 0.35f);
        Color riftCyan = new Color(0.2f, 0.85f, 0.95f, 0.4f);
        Color riftViolet = new Color(0.65f, 0.2f, 0.85f, 0.35f);

        for (int y = 0; y < 128; y++)
        {
            float t = y / 128f;
            Color baseCol = t < 0.5f ? Color.Lerp(skyBottom, skyMid, t * 2f) : Color.Lerp(skyMid, skyTop, (t - 0.5f) * 2f);
            for (int x = 0; x < 128; x++)
            {
                float rx = (x - 64) / 64f;
                float ry = (y - 90) / 38f;
                float riftDist = Mathf.Abs(ry - Mathf.Sin(rx * 3f) * 0.25f);
                Color c = baseCol;
                if (riftDist < 0.2f && y > 60) c = Color.Lerp(c, riftCyan, (0.2f - riftDist) * 3f);
                else if (riftDist < 0.35f && y > 50) c = Color.Lerp(c, riftViolet, (0.35f - riftDist) * 2f);
                if ((x * 37 + y * 89) % 197 == 0 && y > 50) c = Color.white;
                sky.SetPixel(x, y, c);
            }
        }
        SaveSprite(dir + "spr_bg_sky.png", sky, 32);

        Texture2D mtn = new Texture2D(128, 64, TextureFormat.RGBA32, false);
        Clear(mtn, Color.clear);
        Color mtnCol = new Color(0.1f, 0.18f, 0.24f, 0.92f);
        Color mtnHaze = new Color(0.18f, 0.3f, 0.36f, 0.8f);

        for (int x = 0; x < 128; x++)
        {
            float h1 = 28f + Mathf.Sin(x * 0.08f) * 16f + Mathf.Cos(x * 0.16f) * 8f;
            float h2 = 20f + Mathf.Sin(x * 0.05f + 1f) * 14f;
            float maxH = Mathf.Max(h1, h2);
            for (int y = 0; y < 64; y++)
            {
                if (y < maxH)
                {
                    float fade = (float)y / maxH;
                    mtn.SetPixel(x, y, Color.Lerp(mtnCol, mtnHaze, fade * 0.4f));
                }
            }
        }
        SaveSprite(dir + "spr_bg_mountains.png", mtn, 32);

        Texture2D can = new Texture2D(128, 64, TextureFormat.RGBA32, false);
        Clear(can, Color.clear);
        Color canDark = new Color(0.08f, 0.22f, 0.16f, 0.95f);
        Color canLight = new Color(0.14f, 0.35f, 0.25f, 0.95f);

        for (int x = 0; x < 128; x++)
        {
            float treeH = 32f + Mathf.Sin(x * 0.2f) * 10f + Mathf.Sin(x * 0.45f) * 6f;
            for (int y = 0; y < 64; y++)
            {
                if (y < treeH) can.SetPixel(x, y, y > treeH - 5 ? canLight : canDark);
            }
        }
        SaveSprite(dir + "spr_bg_canopy.png", can, 32);
    }

    private static void GenUI(string dir)
    {
        Texture2D panel = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        Color bg = new Color(0.08f, 0.11f, 0.16f, 0.94f);
        Color borderDark = new Color(0.35f, 0.25f, 0.1f);
        Color borderGold = new Color(0.88f, 0.72f, 0.25f);
        Color borderRune = new Color(0.2f, 0.85f, 0.95f);

        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                bool isOuter = (x < 2 || x >= 62 || y < 2 || y >= 62);
                bool isBorder = (x < 5 || x >= 59 || y < 5 || y >= 59);
                bool isCorner = (x < 8 && y < 8) || (x >= 56 && y < 8) || (x < 8 && y >= 56) || (x >= 56 && y >= 56);

                if (isOuter) panel.SetPixel(x, y, borderDark);
                else if (isCorner) panel.SetPixel(x, y, borderRune);
                else if (isBorder) panel.SetPixel(x, y, borderGold);
                else panel.SetPixel(x, y, bg);
            }
        }
        SaveSprite(dir + "spr_ui_panel.png", panel, 32);

        Texture2D btn = new Texture2D(64, 24, TextureFormat.RGBA32, false);
        Color btnBg = new Color(0.15f, 0.22f, 0.28f);
        Color btnLight = new Color(0.25f, 0.35f, 0.45f);
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (x == 0 || x == 63 || y == 0 || y == 23) btn.SetPixel(x, y, borderGold);
                else if (y == 22) btn.SetPixel(x, y, btnLight);
                else btn.SetPixel(x, y, btnBg);
            }
        }
        SaveSprite(dir + "spr_ui_button.png", btn, 32);
    }

    [MenuItem("ChronoRift/Generate Castle Sprites")]
    public static void GenerateCastle()
    {
        string dir = "Assets/ChronoRift/Sprites/";

        // 1. Castle Stone Brick (32x32)
        Texture2D brick = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Clear(brick, new Color(0.22f, 0.22f, 0.26f));
        Color mortar = new Color(0.14f, 0.14f, 0.17f);
        Color brickLight = new Color(0.35f, 0.35f, 0.4f);
        Color brickDark = new Color(0.26f, 0.26f, 0.3f);

        for (int row = 0; row < 4; row++)
        {
            int y0 = row * 8;
            DrawRect(brick, 0, y0, 32, 1, mortar);
            int shift = (row % 2 == 0) ? 0 : 8;
            for (int col = 0; col < 3; col++)
            {
                int x0 = (col * 16 + shift) % 32;
                DrawRect(brick, x0, y0, 1, 8, mortar);
            }
        }
        for (int x = 0; x < 32; x++)
        {
            brick.SetPixel(x, 31, brickLight);
            if (x % 4 == 0) brick.SetPixel(x, 30, brickDark);
        }
        SaveSprite(dir + "spr_castle_brick.png", brick, 32);

        // 2. Castle Wooden Library Platform (64x20)
        Texture2D plat = new Texture2D(64, 20, TextureFormat.RGBA32, false);
        Clear(plat, Color.clear);
        Color woodDark = new Color(0.22f, 0.13f, 0.08f);
        Color woodMid = new Color(0.38f, 0.22f, 0.14f);
        Color woodLight = new Color(0.55f, 0.34f, 0.22f);
        Color iron = new Color(0.18f, 0.2f, 0.24f);
        Color gold = new Color(0.85f, 0.7f, 0.25f);

        DrawRect(plat, 2, 4, 60, 14, woodMid);
        DrawRect(plat, 2, 17, 60, 2, woodLight);
        DrawRect(plat, 2, 3, 60, 2, woodDark);
        for (int x = 4; x < 60; x++)
        {
            if (x % 8 == 0) DrawRect(plat, x, 4, 1, 13, woodDark);
        }
        DrawRect(plat, 2, 2, 4, 16, iron);
        DrawRect(plat, 58, 2, 4, 16, iron);
        plat.SetPixel(3, 10, gold);
        plat.SetPixel(59, 10, gold);
        SaveSprite(dir + "spr_castle_platform.png", plat, 32);

        // 3. Bookshelf (48x64)
        Texture2D shelf = new Texture2D(48, 64, TextureFormat.RGBA32, false);
        Clear(shelf, Color.clear);
        DrawRect(shelf, 2, 0, 44, 64, woodMid);
        DrawRect(shelf, 0, 0, 4, 64, woodDark);
        DrawRect(shelf, 44, 0, 4, 64, woodDark);
        DrawRect(shelf, 0, 60, 48, 4, woodLight);
        DrawRect(shelf, 0, 0, 48, 4, woodDark);

        Color[] bookCols = {
            new Color(0.65f, 0.15f, 0.15f),
            new Color(0.15f, 0.35f, 0.65f),
            new Color(0.15f, 0.5f, 0.25f),
            new Color(0.75f, 0.6f, 0.15f),
            new Color(0.45f, 0.2f, 0.5f)
        };

        int[] shelfY = { 6, 24, 42 };
        int seed = 123;
        foreach (int sy in shelfY)
        {
            DrawRect(shelf, 4, sy, 40, 3, woodDark);
            int bx = 5;
            int idx = 0;
            while (bx < 42)
            {
                int bw = 3 + (seed % 3);
                seed = (seed * 9301 + 49297) % 233280;
                int bh = 11 + (seed % 3);
                seed = (seed * 9301 + 49297) % 233280;
                if (bx + bw > 43) bw = 43 - bx;
                Color bc = bookCols[(idx++) % bookCols.Length];
                DrawRect(shelf, bx, sy + 3, bw - 1, bh, bc);
                shelf.SetPixel(bx + bw / 2, sy + 7, gold);
                bx += bw;
            }
        }
        SaveSprite(dir + "spr_bookshelf.png", shelf, 32);

        // 4. Bookshelf Secret (48x64)
        Texture2D sShelf = new Texture2D(48, 64, TextureFormat.RGBA32, false);
        sShelf.SetPixels(shelf.GetPixels());
        DrawRect(sShelf, 18, 27, 12, 14, new Color(0.1f, 0.12f, 0.16f));
        DrawRect(sShelf, 22, 30, 4, 8, iron);
        sShelf.SetPixel(23, 36, new Color(0.2f, 0.9f, 1f));
        sShelf.SetPixel(24, 36, Color.white);
        SaveSprite(dir + "spr_bookshelf_secret.png", sShelf, 32);

        // 5. Lever Off (24x24)
        Texture2D levOff = new Texture2D(24, 24, TextureFormat.RGBA32, false);
        Clear(levOff, Color.clear);
        DrawRect(levOff, 4, 4, 16, 16, iron);
        DrawRect(levOff, 6, 6, 12, 12, new Color(0.28f, 0.3f, 0.35f));
        levOff.SetPixel(5, 18, gold);
        levOff.SetPixel(18, 18, gold);
        levOff.SetPixel(5, 5, gold);
        levOff.SetPixel(18, 5, gold);
        for (int i = 0; i < 9; i++)
        {
            levOff.SetPixel(12, 8 + i, gold);
            levOff.SetPixel(13, 8 + i, gold);
        }
        DrawRect(levOff, 10, 16, 5, 5, new Color(0.85f, 0.2f, 0.2f));
        SaveSprite(dir + "spr_lever_off.png", levOff, 32);

        // 6. Lever On (24x24)
        Texture2D levOn = new Texture2D(24, 24, TextureFormat.RGBA32, false);
        Clear(levOn, Color.clear);
        DrawRect(levOn, 4, 4, 16, 16, iron);
        DrawRect(levOn, 6, 6, 12, 12, new Color(0.28f, 0.3f, 0.35f));
        levOn.SetPixel(5, 18, gold);
        levOn.SetPixel(18, 18, gold);
        levOn.SetPixel(5, 5, gold);
        levOn.SetPixel(18, 5, gold);
        for (int i = 0; i < 9; i++)
        {
            levOn.SetPixel(12, 14 - i, gold);
            levOn.SetPixel(13, 14 - i, gold);
        }
        DrawRect(levOn, 10, 5, 5, 5, new Color(0.2f, 0.95f, 0.4f));
        SaveSprite(dir + "spr_lever_on.png", levOn, 32);

        // 7. Secret Door (36x76)
        Texture2D secDoor = new Texture2D(36, 76, TextureFormat.RGBA32, false);
        Clear(secDoor, Color.clear);
        DrawRect(secDoor, 2, 0, 32, 76, new Color(0.25f, 0.27f, 0.32f));
        for (int y = 0; y < 76; y += 12)
        {
            DrawRect(secDoor, 2, y, 32, 1, mortar);
        }
        for (int y = 6; y < 74; y += 12)
        {
            secDoor.SetPixel(6, y, gold);
            secDoor.SetPixel(29, y, gold);
        }
        DrawRect(secDoor, 15, 34, 6, 8, iron);
        secDoor.SetPixel(17, 38, new Color(0.2f, 0.85f, 1f));
        secDoor.SetPixel(18, 38, Color.white);
        SaveSprite(dir + "spr_secret_door.png", secDoor, 32);

        // 8. Castle Heraldic Banner (24x48)
        Texture2D banner = new Texture2D(24, 48, TextureFormat.RGBA32, false);
        Clear(banner, Color.clear);
        DrawRect(banner, 0, 44, 24, 3, iron);
        banner.SetPixel(0, 46, gold);
        banner.SetPixel(23, 46, gold);
        DrawRect(banner, 3, 10, 18, 34, new Color(0.7f, 0.12f, 0.16f));
        for (int x = 3; x <= 20; x++)
        {
            int cut = 10 - Mathf.Abs(x - 11);
            for (int y = 0; y < cut; y++)
                banner.SetPixel(x, y + 5, new Color(0.7f, 0.12f, 0.16f));
        }
        DrawRect(banner, 9, 24, 6, 8, gold);
        DrawRect(banner, 10, 31, 4, 3, gold);
        SaveSprite(dir + "spr_castle_banner.png", banner, 32);

        // 9. Castle Wall Torch (16x32)
        Texture2D torch = new Texture2D(16, 32, TextureFormat.RGBA32, false);
        Clear(torch, Color.clear);
        DrawRect(torch, 7, 4, 2, 14, iron);
        DrawRect(torch, 5, 14, 6, 3, iron);
        Color flameOuter = new Color(1f, 0.35f, 0.05f);
        Color flameMid = new Color(1f, 0.85f, 0.2f);
        DrawRect(torch, 6, 17, 4, 8, flameOuter);
        DrawRect(torch, 7, 18, 2, 7, flameMid);
        torch.SetPixel(7, 20, Color.white);
        SaveSprite(dir + "spr_castle_torch.png", torch, 32);

        // 10. Castle Interior Background (128x128)
        Texture2D bgCastle = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        Color bgStone = new Color(0.1f, 0.11f, 0.15f);
        Color bgWindow = new Color(0.12f, 0.2f, 0.35f);

        for (int x = 0; x < 128; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                bgCastle.SetPixel(x, y, bgStone);
            }
        }
        int[] winX = { 32, 96 };
        foreach (int wx in winX)
        {
            for (int x = wx - 14; x <= wx + 14; x++)
            {
                for (int y = 30; y < 100; y++)
                {
                    float dx = Mathf.Abs(x - wx);
                    float archTop = 90f - (dx * dx) / 5f;
                    if (y < archTop)
                    {
                        bgCastle.SetPixel(x, y, (x == wx - 14 || x == wx + 14 || y == 30 || Mathf.Abs(y - archTop) < 1.5f || x == wx) ? new Color(0.18f, 0.2f, 0.26f) : bgWindow);
                    }
                }
            }
        }
        SaveSprite(dir + "spr_bg_castle.png", bgCastle, 32);

        AssetDatabase.Refresh();
        Debug.Log("Castle sprites generated!");
    }


    [MenuItem("ChronoRift/Generate Lab Sprites")]
    public static void GenerateFuturisticLab()
    {
        string dir = "Assets/ChronoRift/Sprites/";

        Color metalDark = new Color(0.12f, 0.14f, 0.18f);
        Color metalMid = new Color(0.2f, 0.24f, 0.3f);
        Color metalLight = new Color(0.35f, 0.4f, 0.5f);
        Color neonCyan = new Color(0.1f, 0.9f, 1f);
        Color neonAmber = new Color(1f, 0.75f, 0.15f);
        Color neonMagenta = new Color(0.9f, 0.2f, 0.85f);

        // 1. Lab Metal Floor (32x32)
        Texture2D floor = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Clear(floor, metalDark);
        DrawRect(floor, 1, 1, 30, 30, metalMid);
        // Beveled tile edge
        DrawRect(floor, 0, 31, 32, 1, metalLight);
        DrawRect(floor, 0, 0, 32, 1, new Color(0.08f, 0.09f, 0.12f));
        // Cyan circuit inlays
        for (int x = 6; x < 26; x++)
        {
            if (x % 4 == 0)
            {
                floor.SetPixel(x, 16, neonCyan);
                floor.SetPixel(x, 17, Color.white);
            }
        }
        floor.SetPixel(16, 8, neonCyan);
        floor.SetPixel(16, 24, neonCyan);
        // Corner bolts
        floor.SetPixel(3, 3, metalLight);
        floor.SetPixel(28, 3, metalLight);
        floor.SetPixel(3, 28, metalLight);
        floor.SetPixel(28, 28, metalLight);
        SaveSprite(dir + "spr_lab_floor.png", floor, 32);

        // 2. High-Tech Hover Platform (64x20)
        Texture2D plat = new Texture2D(64, 20, TextureFormat.RGBA32, false);
        Clear(plat, Color.clear);
        DrawRect(plat, 2, 4, 60, 13, metalDark);
        DrawRect(plat, 3, 5, 58, 11, metalMid);
        DrawRect(plat, 2, 16, 60, 2, metalLight); // top rim
        // Neon cyan underglow strip
        DrawRect(plat, 6, 3, 52, 2, neonCyan);
        DrawRect(plat, 12, 4, 40, 1, Color.white);
        // Hexagonal vents
        for (int x = 10; x < 54; x += 8)
        {
            DrawRect(plat, x, 8, 4, 4, metalDark);
            plat.SetPixel(x + 1, 9, neonCyan);
        }
        SaveSprite(dir + "spr_lab_platform.png", plat, 32);

        // 3. Power Chip (24x24)
        Texture2D chip = new Texture2D(24, 24, TextureFormat.RGBA32, false);
        Clear(chip, Color.clear);
        // Golden connector pins (top, bottom, left, right)
        for (int i = 6; i <= 17; i += 3)
        {
            chip.SetPixel(i, 2, neonAmber);
            chip.SetPixel(i, 3, neonAmber);
            chip.SetPixel(i, 20, neonAmber);
            chip.SetPixel(i, 21, neonAmber);
            chip.SetPixel(2, i, neonAmber);
            chip.SetPixel(3, i, neonAmber);
            chip.SetPixel(20, i, neonAmber);
            chip.SetPixel(21, i, neonAmber);
        }
        // Black ceramic package
        DrawRect(chip, 4, 4, 16, 16, new Color(0.12f, 0.13f, 0.16f));
        DrawRect(chip, 5, 5, 14, 14, new Color(0.18f, 0.2f, 0.25f));
        // Glowing cyan energy core
        DrawRect(chip, 8, 8, 8, 8, neonCyan);
        DrawRect(chip, 10, 10, 4, 4, Color.white);
        // Corner gold notch
        chip.SetPixel(5, 18, neonAmber);
        SaveSprite(dir + "spr_power_chip.png", chip, 32);

        // 4. Security Blast Door (36x76)
        Texture2D door = new Texture2D(36, 76, TextureFormat.RGBA32, false);
        Clear(door, Color.clear);
        DrawRect(door, 2, 0, 32, 76, metalDark);
        DrawRect(door, 4, 2, 28, 72, metalMid);
        // Red laser security grid lines
        for (int y = 8; y < 70; y += 10)
        {
            DrawRect(door, 6, y, 24, 2, new Color(1f, 0.15f, 0.2f));
            door.SetPixel(17, y, Color.white);
            door.SetPixel(18, y, Color.white);
        }
        // Central biometric lock / warning chevron
        DrawRect(door, 13, 34, 10, 10, metalDark);
        DrawRect(door, 15, 36, 6, 6, neonAmber);
        door.SetPixel(17, 38, Color.white);
        door.SetPixel(18, 38, Color.white);
        SaveSprite(dir + "spr_security_door.png", door, 32);

        // 5. Futuristic Time Core (40x48)
        Texture2D core = new Texture2D(40, 48, TextureFormat.RGBA32, false);
        Clear(core, Color.clear);
        Vector2 c = new Vector2(20, 24);
        // Gyro containment rings
        for (int x = 0; x < 40; x++)
        {
            for (int y = 0; y < 48; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                // Outer ring
                if (Mathf.Abs(d - 17f) < 1.5f)
                {
                    core.SetPixel(x, y, metalLight);
                }
                // Middle plasma ring
                if (Mathf.Abs(d - 12f) < 1.4f)
                {
                    core.SetPixel(x, y, neonMagenta);
                }
                // Core plasma sphere
                if (d < 8f)
                {
                    core.SetPixel(x, y, neonCyan);
                }
                if (d < 4f)
                {
                    core.SetPixel(x, y, Color.white);
                }
            }
        }
        // Base pedestal stand
        DrawRect(core, 12, 0, 16, 6, metalDark);
        DrawRect(core, 14, 2, 12, 4, metalMid);
        core.SetPixel(19, 4, neonCyan);
        core.SetPixel(20, 4, neonCyan);
        SaveSprite(dir + "spr_time_core.png", core, 32);

        // 6. Lab Computer Console (32x48)
        Texture2D console = new Texture2D(32, 48, TextureFormat.RGBA32, false);
        Clear(console, Color.clear);
        DrawRect(console, 2, 0, 28, 48, metalDark);
        DrawRect(console, 4, 2, 24, 44, metalMid);
        // Holographic screen (cyan glow)
        DrawRect(console, 6, 24, 20, 18, new Color(0.08f, 0.25f, 0.35f));
        DrawRect(console, 8, 26, 16, 14, new Color(0.1f, 0.5f, 0.65f));
        // Screen text / sine wave
        for (int x = 8; x < 24; x++)
        {
            int sy = 33 + (int)(Mathf.Sin(x * 0.8f) * 3f);
            console.SetPixel(x, sy, neonCyan);
            console.SetPixel(x, 28, neonAmber);
        }
        // Server status LEDs
        for (int y = 6; y < 20; y += 4)
        {
            console.SetPixel(8, y, Color.green);
            console.SetPixel(14, y, neonCyan);
            console.SetPixel(20, y, neonAmber);
        }
        SaveSprite(dir + "spr_lab_console.png", console, 32);

        // 7. Lab & Neon City Backdrop (128x128)
        Texture2D bgLab = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        Color skyDark = new Color(0.05f, 0.06f, 0.12f);
        Color skyPurple = new Color(0.12f, 0.08f, 0.22f);
        Color cityBuilding = new Color(0.08f, 0.1f, 0.18f);

        for (int y = 0; y < 128; y++)
        {
            float t = (float)y / 128f;
            Color sky = Color.Lerp(skyPurple, skyDark, t);
            for (int x = 0; x < 128; x++)
            {
                bgLab.SetPixel(x, y, sky);
            }
        }
        // Cyberpunk skyscraper silhouettes
        int[] bldX = { 10, 35, 60, 85, 110 };
        int[] bldW = { 18, 22, 16, 24, 15 };
        int[] bldH = { 70, 95, 60, 85, 55 };
        for (int i = 0; i < bldX.Length; i++)
        {
            DrawRect(bgLab, bldX[i] - bldW[i] / 2, 0, bldW[i], bldH[i], cityBuilding);
            // Window neon dots
            for (int wx = bldX[i] - bldW[i] / 2 + 3; wx < bldX[i] + bldW[i] / 2 - 2; wx += 4)
            {
                for (int wy = 8; wy < bldH[i] - 6; wy += 8)
                {
                    if ((wx * 7 + wy * 13) % 5 != 0)
                    {
                        bgLab.SetPixel(wx, wy, ((wx + wy) % 2 == 0) ? neonCyan : neonAmber);
                    }
                }
            }
        }
        SaveSprite(dir + "spr_bg_lab.png", bgLab, 32);

        AssetDatabase.Refresh();
        Debug.Log("Lab sprites generated successfully!");
    }

}
