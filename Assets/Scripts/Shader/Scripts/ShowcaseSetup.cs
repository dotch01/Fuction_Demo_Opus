using UnityEngine;

// ============================================================
// ShowcaseSetup.cs
// 靜態工具類：為每個 Shader 效果建立展示用的 3D 物件與材質
// ============================================================

public static class ShowcaseSetup
{
    // ---- Portal 傳送門 ----
    public static GameObject CreatePortal(Transform parent, RenderTexture portalRT)
    {
        var root = new GameObject("Portal_Showcase");
        root.transform.SetParent(parent, false);

        var shader = FindShader("Showcase/Portal");
        var mat = new Material(shader);
        mat.SetTexture("_PortalTex", portalRT);
        mat.SetColor("_EdgeColor", new Color(0.2f, 0.6f, 1f, 1f));
        mat.SetFloat("_EdgeWidth", 0.05f);
        mat.SetFloat("_EdgeGlow", 2.5f);

        var portal = GameObject.CreatePrimitive(PrimitiveType.Quad);
        portal.name = "PortalSurface";
        portal.transform.SetParent(root.transform, false);
        portal.transform.localScale = new Vector3(3f, 4f, 1f);
        portal.transform.localPosition = new Vector3(0, 0, 0);
        portal.GetComponent<Renderer>().material = mat;
        RemoveCollider(portal);

        root.SetActive(false);
        return root;
    }

    // ---- Outline 描邊 ----
    public static GameObject CreateOutline(Transform parent)
    {
        var root = new GameObject("Outline_Showcase");
        root.transform.SetParent(parent, false);

        var shader = FindShader("Showcase/Outline");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", new Color(0.9f, 0.3f, 0.2f, 1f));
        mat.SetColor("_OutlineColor", Color.black);
        mat.SetFloat("_OutlineWidth", 0.035f);

        var mat2 = new Material(shader);
        mat2.SetColor("_BaseColor", new Color(0.2f, 0.6f, 0.9f, 1f));
        mat2.SetColor("_OutlineColor", new Color(1f, 0.9f, 0.2f, 1f));
        mat2.SetFloat("_OutlineWidth", 0.04f);

        CreatePrimitive(root.transform, PrimitiveType.Sphere, new Vector3(-1.5f, 0, 0), Vector3.one * 1.5f, mat);
        CreatePrimitive(root.transform, PrimitiveType.Cube, new Vector3(1.5f, 0, 0), Vector3.one * 1.3f, mat2);

        root.SetActive(false);
        return root;
    }

    // ---- RGB Chromatic Aberration ----
    public static GameObject CreateChromaticAberration(Transform parent)
    {
        var root = new GameObject("ChromAberr_Showcase");
        root.transform.SetParent(parent, false);

        var shader = FindShader("Showcase/ChromaticAberration");
        var mat = new Material(shader);
        mat.SetFloat("_Intensity", 0.025f);

        var tex = CreateCheckerTexture(256, 16,
            new Color(0.9f, 0.2f, 0.3f), new Color(0.2f, 0.5f, 0.9f));
        mat.SetTexture("_MainTex", tex);

        CreatePrimitive(root.transform, PrimitiveType.Quad,
            Vector3.zero, new Vector3(4f, 3f, 1f), mat);

        var normalMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        normalMat.mainTexture = tex;
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(-3.5f, 0, 0), new Vector3(2f, 1.5f, 1f), normalMat);

        AddLabel(root.transform, new Vector3(-3.5f, -1.2f, 0), "Original", 14);
        AddLabel(root.transform, new Vector3(0, -2f, 0), "<- RGB Chromatic Aberration ->", 16);

        root.SetActive(false);
        return root;
    }

    // ---- Noise (Hash / Perlin / Voronoi) ----
    public static GameObject CreateNoise(Transform parent)
    {
        var root = new GameObject("Noise_Showcase");
        root.transform.SetParent(parent, false);

        // Hash Noise
        var hashMat = new Material(FindShader("Showcase/HashNoise"));
        hashMat.SetFloat("_Scale", 30f);
        hashMat.SetFloat("_Speed", 2f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(-2.8f, 0, 0), new Vector3(2.2f, 2.2f, 1f), hashMat);
        AddLabel(root.transform, new Vector3(-2.8f, -1.5f, 0), "Hash Noise", 14);

        // Perlin Noise
        var perlinMat = new Material(FindShader("Showcase/PerlinNoise"));
        perlinMat.SetFloat("_Scale", 5f);
        perlinMat.SetFloat("_Speed", 0.5f);
        perlinMat.SetFloat("_Octaves", 4f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(0, 0, 0), new Vector3(2.2f, 2.2f, 1f), perlinMat);
        AddLabel(root.transform, new Vector3(0, -1.5f, 0), "Perlin FBM", 14);

        // Voronoi Noise
        var voronoiMat = new Material(FindShader("Showcase/VoronoiNoise"));
        voronoiMat.SetFloat("_CellDensity", 6f);
        voronoiMat.SetFloat("_Speed", 1f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(2.8f, 0, 0), new Vector3(2.2f, 2.2f, 1f), voronoiMat);
        AddLabel(root.transform, new Vector3(2.8f, -1.5f, 0), "Voronoi", 14);

        root.SetActive(false);
        return root;
    }

    // ---- Water ----
    public static GameObject CreateWater(Transform parent)
    {
        var root = new GameObject("Water_Showcase");
        root.transform.SetParent(parent, false);

        var mat = new Material(FindShader("Showcase/Water"));
        var plane = CreatePrimitive(root.transform, PrimitiveType.Plane,
            new Vector3(0, -1f, 0), new Vector3(0.8f, 1f, 0.4f), mat);
        plane.transform.localRotation = Quaternion.identity;

        var cubeMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        cubeMat.color = new Color(0.6f, 0.3f, 0.1f);
        CreatePrimitive(root.transform, PrimitiveType.Cube,
            new Vector3(-1.5f, 0.3f, 0), new Vector3(0.5f, 0.8f, 0.5f), cubeMat);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(1.5f, 0.2f, 0), Vector3.one * 0.6f, cubeMat);

        root.SetActive(false);
        return root;
    }

    // ---- Fire ----
    public static GameObject CreateFire(Transform parent)
    {
        var root = new GameObject("Fire_Showcase");
        root.transform.SetParent(parent, false);

        var mat = new Material(FindShader("Showcase/Fire"));
        mat.SetFloat("_FireSpeed", 2f);
        mat.SetFloat("_Distortion", 0.8f);
        mat.SetFloat("_NoiseScale", 6f);

        var quad = CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(0, 0.5f, 0), new Vector3(3f, 4f, 1f), mat);
        RemoveCollider(quad);

        var baseMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        baseMat.color = new Color(0.3f, 0.15f, 0.05f);
        CreatePrimitive(root.transform, PrimitiveType.Cylinder,
            new Vector3(0, -1.8f, 0), new Vector3(0.8f, 0.3f, 0.8f), baseMat);

        root.SetActive(false);
        return root;
    }

    // ---- Lightning ----
    public static GameObject CreateLightning(Transform parent)
    {
        var root = new GameObject("Lightning_Showcase");
        root.transform.SetParent(parent, false);

        var mat = new Material(FindShader("Showcase/Lightning"));
        mat.SetColor("_Color", new Color(0.7f, 0.85f, 1f));
        mat.SetColor("_GlowColor", new Color(0.3f, 0.5f, 1f));
        mat.SetFloat("_Thickness", 0.015f);
        mat.SetFloat("_Branches", 2f);
        mat.SetFloat("_FlickerSpeed", 8f);

        CreatePrimitive(root.transform, PrimitiveType.Quad,
            Vector3.zero, new Vector3(4f, 5f, 1f), mat);

        root.SetActive(false);
        return root;
    }

    // ---- Stencil ----
    public static GameObject CreateStencil(Transform parent)
    {
        var root = new GameObject("Stencil_Showcase");
        root.transform.SetParent(parent, false);

        // 被遮罩的內容（先渲染，Queue 較低）
        var readMat = new Material(FindShader("Showcase/StencilRead"));
        readMat.SetColor("_Color", new Color(1f, 0.4f, 0.2f));
        readMat.renderQueue = 2001;

        CreatePrimitive(root.transform, PrimitiveType.Cube,
            new Vector3(0, 0, 0.2f), new Vector3(4f, 4f, 0.1f), readMat);

        // 遮罩球形（Stencil Write，Queue 更低先渲染）
        var writeMat = new Material(FindShader("Showcase/StencilWrite"));
        writeMat.SetColor("_Color", new Color(0.2f, 0.5f, 1f, 0.15f));
        writeMat.renderQueue = 2000;

        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(0, 0, 0), Vector3.one * 2.5f, writeMat);

        AddLabel(root.transform, new Vector3(0, -2.5f, 0), "Stencil Mask: Cube visible only inside Sphere", 14);

        root.SetActive(false);
        return root;
    }

    // ---- Fog + Rain ----
    public static GameObject CreateFogRain(Transform parent)
    {
        var root = new GameObject("FogRain_Showcase");
        root.transform.SetParent(parent, false);

        var fogMat = new Material(FindShader("Showcase/Fog"));
        fogMat.SetColor("_FogColor", new Color(0.6f, 0.65f, 0.7f));
        fogMat.SetFloat("_Density", 2f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(-2, 0, 0), new Vector3(3f, 3f, 1f), fogMat);
        AddLabel(root.transform, new Vector3(-2, -2, 0), "Fog", 14);

        var rainMat = new Material(FindShader("Showcase/Rain"));
        rainMat.SetFloat("_Speed", 8f);
        rainMat.SetFloat("_Density", 80f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(2, 0, 0), new Vector3(3f, 3f, 1f), rainMat);
        AddLabel(root.transform, new Vector3(2, -2, 0), "Rain", 14);

        root.SetActive(false);
        return root;
    }

    // ---- Bubble + Glow ----
    public static GameObject CreateBubbleGlow(Transform parent)
    {
        var root = new GameObject("BubbleGlow_Showcase");
        root.transform.SetParent(parent, false);

        var bubbleMat = new Material(FindShader("Showcase/Bubble"));
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(-1.5f, 0.5f, 0), Vector3.one * 1.5f, bubbleMat);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(-0.3f, -0.5f, 0.5f), Vector3.one * 0.8f, bubbleMat);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(-2.5f, -0.3f, 0.3f), Vector3.one * 0.6f, bubbleMat);
        AddLabel(root.transform, new Vector3(-1.5f, -1.8f, 0), "Bubble", 14);

        var glowMat = new Material(FindShader("Showcase/Glow"));
        glowMat.SetColor("_GlowColor", new Color(0.3f, 0.6f, 1f));
        glowMat.SetFloat("_Intensity", 3f);

        var glowMat2 = new Material(FindShader("Showcase/Glow"));
        glowMat2.SetColor("_GlowColor", new Color(1f, 0.3f, 0.5f));
        glowMat2.SetFloat("_Intensity", 2.5f);

        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(2, 0.3f, 0), new Vector3(2f, 2f, 1f), glowMat);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(3, -0.5f, 0.2f), new Vector3(1.2f, 1.2f, 1f), glowMat2);
        AddLabel(root.transform, new Vector3(2, -1.8f, 0), "Glow", 14);

        root.SetActive(false);
        return root;
    }

    // ---- Particles ----
    public static GameObject CreateParticles(Transform parent)
    {
        var root = new GameObject("Particles_Showcase");
        root.transform.SetParent(parent, false);

        var mat = new Material(FindShader("Showcase/Particles"));
        mat.SetColor("_TintColor", new Color(0.3f, 0.8f, 1f));

        var mat2 = new Material(FindShader("Showcase/Particles"));
        mat2.SetColor("_TintColor", new Color(1f, 0.5f, 0.2f));

        var mat3 = new Material(FindShader("Showcase/Particles"));
        mat3.SetColor("_TintColor", new Color(0.4f, 1f, 0.4f));

        float[] xs = { -2, -1, 0, 1, 2, -1.5f, 0.5f, 1.5f, -0.5f, 0.8f };
        float[] ys = { 0.5f, -0.3f, 1, -0.5f, 0.2f, 0.8f, -0.8f, 1.2f, -1, 0.6f };
        float[] scales = { 0.4f, 0.6f, 0.5f, 0.35f, 0.55f, 0.45f, 0.3f, 0.5f, 0.4f, 0.35f };
        Material[] mats = { mat, mat2, mat3, mat, mat2, mat3, mat, mat2, mat3, mat };

        for (int i = 0; i < xs.Length; i++)
        {
            CreatePrimitive(root.transform, PrimitiveType.Quad,
                new Vector3(xs[i], ys[i], i * 0.05f), Vector3.one * scales[i], mats[i]);
        }

        root.SetActive(false);
        return root;
    }

    // ---- SSS 次表面散射 ----
    public static GameObject CreateSSS(Transform parent)
    {
        var root = new GameObject("SSS_Showcase");
        root.transform.SetParent(parent, false);

        // 皮膚色球體
        var skinMat = new Material(FindShader("Showcase/SSS"));
        skinMat.SetColor("_BaseColor", new Color(0.85f, 0.55f, 0.45f));
        skinMat.SetColor("_SSSColor", new Color(1f, 0.3f, 0.15f));
        skinMat.SetFloat("_SSSPower", 4f);
        skinMat.SetFloat("_SSSStrength", 1.2f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(-2f, 0, 0), Vector3.one * 2f, skinMat);
        AddLabel(root.transform, new Vector3(-2f, -1.5f, 0), "Skin", 14);

        // 蠟燭色球體
        var waxMat = new Material(FindShader("Showcase/SSS"));
        waxMat.SetColor("_BaseColor", new Color(0.9f, 0.85f, 0.6f));
        waxMat.SetColor("_SSSColor", new Color(1f, 0.6f, 0.1f));
        waxMat.SetFloat("_SSSPower", 2f);
        waxMat.SetFloat("_SSSStrength", 2f);
        waxMat.SetFloat("_Thickness", 0.7f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(0, 0, 0), Vector3.one * 1.6f, waxMat);
        AddLabel(root.transform, new Vector3(0, -1.3f, 0), "Wax", 14);

        // 樹葉色球體
        var leafMat = new Material(FindShader("Showcase/SSS"));
        leafMat.SetColor("_BaseColor", new Color(0.2f, 0.5f, 0.15f));
        leafMat.SetColor("_SSSColor", new Color(0.4f, 0.9f, 0.1f));
        leafMat.SetFloat("_SSSPower", 3f);
        leafMat.SetFloat("_SSSStrength", 1.5f);
        leafMat.SetFloat("_Thickness", 0.3f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(2f, 0, 0), Vector3.one * 1.6f, leafMat);
        AddLabel(root.transform, new Vector3(2f, -1.3f, 0), "Leaf", 14);

        root.SetActive(false);
        return root;
    }

    // ---- POM 視差貼圖 ----
    public static GameObject CreatePOM(Transform parent)
    {
        var root = new GameObject("POM_Showcase");
        root.transform.SetParent(parent, false);

        var mat = new Material(FindShader("Showcase/POM"));
        mat.SetFloat("_HeightScale", 0.1f);
        mat.SetFloat("_Steps", 32f);
        mat.SetFloat("_BrickScale", 4f);

        // 大面板展示磚牆效果
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(0, 0, 0), new Vector3(5f, 4f, 1f), mat);

        // 旁邊放一個無 POM 的對照
        var flatMat = new Material(FindShader("Showcase/POM"));
        flatMat.SetFloat("_HeightScale", 0f);
        flatMat.SetFloat("_BrickScale", 4f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(-3.8f, 0, 0), new Vector3(2f, 1.6f, 1f), flatMat);

        AddLabel(root.transform, new Vector3(-3.8f, -1.2f, 0), "Flat (no POM)", 12);
        AddLabel(root.transform, new Vector3(0, -2.5f, 0), "Parallax Occlusion Mapping", 14);

        root.SetActive(false);
        return root;
    }

    // ---- Volumetric Cloud 體積雲 ----
    public static GameObject CreateVolumetricCloud(Transform parent)
    {
        var root = new GameObject("Volumetric_Showcase");
        root.transform.SetParent(parent, false);

        var mat = new Material(FindShader("Showcase/VolumetricCloud"));
        mat.SetFloat("_Density", 1.5f);
        mat.SetFloat("_CloudScale", 3f);
        mat.SetFloat("_Steps", 40f);

        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(0, 0, 0), new Vector3(6f, 4.5f, 1f), mat);

        root.SetActive(false);
        return root;
    }

    // ---- Cloth/Flag 布料旗幟 ----
    public static GameObject CreateClothFlag(Transform parent)
    {
        var root = new GameObject("ClothFlag_Showcase");
        root.transform.SetParent(parent, false);

        var mat = new Material(FindShader("Showcase/ClothFlag"));

        // 需要一個有足夠頂點的 Plane 才能看到頂點位移效果
        // 用 Plane（10x10 面 = 100 頂點夠用）
        var flag = GameObject.CreatePrimitive(PrimitiveType.Plane);
        flag.name = "Flag";
        flag.transform.SetParent(root.transform, false);
        flag.transform.localPosition = new Vector3(0, 0, 0);
        flag.transform.localScale = new Vector3(0.4f, 1f, 0.3f);
        // 旋轉使其面朝鏡頭
        flag.transform.localRotation = Quaternion.Euler(90, 0, 0);
        flag.GetComponent<Renderer>().material = mat;
        RemoveCollider(flag);

        // 旗桿
        var poleMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        poleMat.color = new Color(0.4f, 0.35f, 0.3f);
        CreatePrimitive(root.transform, PrimitiveType.Cylinder,
            new Vector3(-2.1f, 0, 0), new Vector3(0.08f, 2f, 0.08f), poleMat);

        root.SetActive(false);
        return root;
    }

    // ---- SSR 螢幕空間反射 ----
    public static GameObject CreateSSR(Transform parent)
    {
        var root = new GameObject("SSR_Showcase");
        root.transform.SetParent(parent, false);

        var floorMat = new Material(FindShader("Showcase/SSR"));

        // 反射地板（傾斜面朝鏡頭）
        var floor = CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(0, -1f, 0.5f), new Vector3(6f, 4f, 1f), floorMat);
        floor.transform.localRotation = Quaternion.Euler(55, 0, 0);

        // 放幾個彩色物件在地板上方（shader 內部會程序化追蹤）
        var objMat1 = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        objMat1.color = new Color(1f, 0.3f, 0.2f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(-0.8f, 0.8f, 0), Vector3.one * 0.8f, objMat1);

        var objMat2 = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        objMat2.color = new Color(0.2f, 0.6f, 1f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(0.8f, 0.6f, 0.3f), Vector3.one * 0.6f, objMat2);

        AddLabel(root.transform, new Vector3(0, -2.5f, 0), "SSR: Floor reflects spheres above", 14);

        root.SetActive(false);
        return root;
    }

    // ---- Rim Light 邊緣光 ----
    public static GameObject CreateRimLight(Transform parent)
    {
        var root = new GameObject("RimLight_Showcase");
        root.transform.SetParent(parent, false);

        var shader = FindShader("Showcase/RimLight");

        // 藍色邊緣光
        var mat1 = new Material(shader);
        mat1.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.2f));
        mat1.SetColor("_RimColor", new Color(0.3f, 0.6f, 1f));
        mat1.SetFloat("_RimPower", 3f);
        mat1.SetFloat("_RimIntensity", 1.5f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(-2.2f, 0, 0), Vector3.one * 1.8f, mat1);
        AddLabel(root.transform, new Vector3(-2.2f, -1.5f, 0), "Blue Rim", 14);

        // 綠色邊緣光
        var mat2 = new Material(shader);
        mat2.SetColor("_BaseColor", new Color(0.12f, 0.18f, 0.12f));
        mat2.SetColor("_RimColor", new Color(0.2f, 1f, 0.4f));
        mat2.SetFloat("_RimPower", 2f);
        mat2.SetFloat("_RimIntensity", 2f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(0, 0, 0), Vector3.one * 1.6f, mat2);
        AddLabel(root.transform, new Vector3(0, -1.3f, 0), "Green Rim (wide)", 14);

        // 橙色邊緣光
        var mat3 = new Material(shader);
        mat3.SetColor("_BaseColor", new Color(0.2f, 0.12f, 0.1f));
        mat3.SetColor("_RimColor", new Color(1f, 0.5f, 0.1f));
        mat3.SetFloat("_RimPower", 5f);
        mat3.SetFloat("_RimIntensity", 2.5f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(2.2f, 0, 0), Vector3.one * 1.6f, mat3);
        AddLabel(root.transform, new Vector3(2.2f, -1.3f, 0), "Orange Rim (tight)", 14);

        root.SetActive(false);
        return root;
    }

    // ---- Fresnel 菲涅爾 ----
    public static GameObject CreateFresnel(Transform parent)
    {
        var root = new GameObject("Fresnel_Showcase");
        root.transform.SetParent(parent, false);

        var shader = FindShader("Showcase/Fresnel");

        // 玻璃球
        var glassMat = new Material(shader);
        glassMat.SetColor("_BaseColor", new Color(0.05f, 0.1f, 0.15f, 0.1f));
        glassMat.SetColor("_FresnelColor", new Color(0.5f, 0.7f, 1f));
        glassMat.SetFloat("_FresnelPower", 3f);
        glassMat.SetFloat("_Opacity", 0.15f);
        glassMat.SetFloat("_EnvReflect", 0.6f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(-1.8f, 0, 0), Vector3.one * 2f, glassMat);
        AddLabel(root.transform, new Vector3(-1.8f, -1.5f, 0), "Glass", 14);

        // 能量護盾
        var shieldMat = new Material(shader);
        shieldMat.SetColor("_BaseColor", new Color(0.1f, 0.02f, 0.15f, 0.05f));
        shieldMat.SetColor("_FresnelColor", new Color(0.6f, 0.2f, 1f));
        shieldMat.SetFloat("_FresnelPower", 2f);
        shieldMat.SetFloat("_Opacity", 0.08f);
        shieldMat.SetFloat("_EnvReflect", 0.3f);
        CreatePrimitive(root.transform, PrimitiveType.Sphere,
            new Vector3(1.8f, 0, 0), Vector3.one * 2f, shieldMat);
        AddLabel(root.transform, new Vector3(1.8f, -1.5f, 0), "Energy Shield", 14);

        // 護盾內部放一個小物件
        var innerMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        innerMat.color = new Color(0.8f, 0.6f, 0.3f);
        CreatePrimitive(root.transform, PrimitiveType.Cube,
            new Vector3(1.8f, 0, 0), Vector3.one * 0.6f, innerMat);

        root.SetActive(false);
        return root;
    }

    // ---- Color Grading 顏色分級 ----
    public static GameObject CreateColorGrading(Transform parent)
    {
        var root = new GameObject("ColorGrading_Showcase");
        root.transform.SetParent(parent, false);

        var shader = FindShader("Showcase/ColorGrading");

        // 產生一張彩色測試圖
        var tex = CreateColorfulTexture(256);

        // 原圖
        var origMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        origMat.mainTexture = tex;
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(-3f, 0, 0), new Vector3(2.4f, 2.4f, 1f), origMat);
        AddLabel(root.transform, new Vector3(-3f, -1.6f, 0), "Original", 14);

        // 電影感 (暖色調, 低飽和, 高對比)
        var cinemaMat = new Material(shader);
        cinemaMat.mainTexture = tex;
        cinemaMat.SetFloat("_GammaR", 0.85f);
        cinemaMat.SetFloat("_GammaG", 1.05f);
        cinemaMat.SetFloat("_GammaB", 1.2f);
        cinemaMat.SetFloat("_Contrast", 1.3f);
        cinemaMat.SetFloat("_Saturation", 0.7f);
        cinemaMat.SetFloat("_Brightness", 0.95f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(0, 0, 0), new Vector3(2.4f, 2.4f, 1f), cinemaMat);
        AddLabel(root.transform, new Vector3(0, -1.6f, 0), "Cinematic", 14);

        // 賽博龐克 (高對比, 紫藍偏冷, 高飽和)
        var cyberMat = new Material(shader);
        cyberMat.mainTexture = tex;
        cyberMat.SetFloat("_GammaR", 1.3f);
        cyberMat.SetFloat("_GammaG", 1.1f);
        cyberMat.SetFloat("_GammaB", 0.7f);
        cyberMat.SetFloat("_Contrast", 1.5f);
        cyberMat.SetFloat("_Saturation", 1.6f);
        cyberMat.SetFloat("_Brightness", 1.1f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(3f, 0, 0), new Vector3(2.4f, 2.4f, 1f), cyberMat);
        AddLabel(root.transform, new Vector3(3f, -1.6f, 0), "Cyberpunk", 14);

        root.SetActive(false);
        return root;
    }

    // ---- Normal Map 法線貼圖 ----
    public static GameObject CreateNormalMap(Transform parent)
    {
        var root = new GameObject("NormalMap_Showcase");
        root.transform.SetParent(parent, false);

        var shader = FindShader("Showcase/NormalMap");

        // 有法線的磚牆
        var normalMat = new Material(shader);
        normalMat.SetColor("_BaseColor", new Color(0.7f, 0.55f, 0.4f));
        normalMat.SetFloat("_NormalStrength", 1.5f);
        normalMat.SetFloat("_BrickScale", 4f);
        normalMat.SetFloat("_LightAngle", 45f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(1.2f, 0, 0), new Vector3(4f, 3.5f, 1f), normalMat);
        AddLabel(root.transform, new Vector3(1.2f, -2.2f, 0), "With Normal Map", 14);

        // 無法線的平面（NormalStrength = 0）
        var flatMat = new Material(shader);
        flatMat.SetColor("_BaseColor", new Color(0.7f, 0.55f, 0.4f));
        flatMat.SetFloat("_NormalStrength", 0f);
        flatMat.SetFloat("_BrickScale", 4f);
        flatMat.SetFloat("_LightAngle", 45f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(-2.8f, 0, 0), new Vector3(2f, 1.8f, 1f), flatMat);
        AddLabel(root.transform, new Vector3(-2.8f, -1.4f, 0), "Flat (no normals)", 12);

        root.SetActive(false);
        return root;
    }

    // ---- Dithering 抖動 ----
    public static GameObject CreateDithering(Transform parent)
    {
        var root = new GameObject("Dithering_Showcase");
        root.transform.SetParent(parent, false);

        var shader = FindShader("Showcase/Dithering");

        // 不同透明度的球體展示抖動效果
        float[] opacities = { 0.25f, 0.5f, 0.75f, 1.0f };
        string[] labels = { "25%", "50%", "75%", "100%" };
        float[] xs = { -3f, -1f, 1f, 3f };

        for (int i = 0; i < 4; i++)
        {
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", new Color(0.3f, 0.7f, 0.4f));
            mat.SetFloat("_Opacity", opacities[i]);
            mat.SetFloat("_DitherScale", 1f);
            CreatePrimitive(root.transform, PrimitiveType.Sphere,
                new Vector3(xs[i], 0.3f, 0), Vector3.one * 1.4f, mat);
            AddLabel(root.transform, new Vector3(xs[i], -1.2f, 0), labels[i], 14);
        }

        // 背後放一個彩色方塊讓 clip 的洞能看到東西
        var bgMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        bgMat.color = new Color(0.8f, 0.3f, 0.2f);
        CreatePrimitive(root.transform, PrimitiveType.Quad,
            new Vector3(0, 0, 1f), new Vector3(10f, 5f, 1f), bgMat);

        root.SetActive(false);
        return root;
    }

    // ============================================================
    // Utilities
    // ============================================================

    private static Shader FindShader(string name)
    {
        var shader = Shader.Find(name);
        if (shader == null)
        {
            Debug.LogError($"[ShowcaseSetup] Shader not found: {name}, using Fallback");
            shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                Debug.LogError("[ShowcaseSetup] Fallback shader also NULL!");
        }
        else
        {
            Debug.Log($"[ShowcaseSetup] Shader OK: {name}");
        }
        return shader;
    }

    private static GameObject CreatePrimitive(Transform parent, PrimitiveType type,
        Vector3 localPos, Vector3 localScale, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.GetComponent<Renderer>().material = mat;
        RemoveCollider(go);
        return go;
    }

    private static void RemoveCollider(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
    }

    private static Texture2D CreateCheckerTexture(int size, int cellSize, Color c1, Color c2)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isC1 = ((x / cellSize) + (y / cellSize)) % 2 == 0;
                tex.SetPixel(x, y, isC1 ? c1 : c2);
            }
        }
        tex.Apply();
        return tex;
    }

    private static void AddLabel(Transform parent, Vector3 localPos, string text, int fontSize)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = fontSize * 5;
        tm.characterSize = 0.15f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
    }

    /// <summary>產生一張彩色漸層測試圖（用於 Color Grading 展示）</summary>
    private static Texture2D CreateColorfulTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (float)x / size;
                float v = (float)y / size;

                // 彩色漸層 + 幾何圖案
                float r = u;
                float g = v;
                float b = 1f - u;

                // 加入圓形圖案
                float cx = u - 0.5f, cy = v - 0.5f;
                float circle = Mathf.Max(0, 1f - Mathf.Sqrt(cx * cx + cy * cy) * 3f);
                r = Mathf.Lerp(r, 1f, circle * 0.5f);
                g = Mathf.Lerp(g, 0.8f, circle * 0.3f);

                // 加入格子暗部
                bool checker = ((x / (size / 8)) + (y / (size / 8))) % 2 == 0;
                float checkerVal = checker ? 1f : 0.75f;
                r *= checkerVal;
                g *= checkerVal;
                b *= checkerVal;

                tex.SetPixel(x, y, new Color(r, g, b, 1f));
            }
        }
        tex.Apply();
        return tex;
    }
}
