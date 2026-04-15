using UnityEngine;
using UnityEngine.Rendering.Universal;

// ============================================================
// PortalRenderer.cs
// 純 C# 類別：管理 Portal 的第二鏡頭與 RenderTexture
// ============================================================

public class PortalRenderer
{
    private Camera portalCamera;
    private RenderTexture portalRT;
    private GameObject portalWorldRoot;

    public const int PortalLayer = 31; // 使用第 31 層專門給 Portal 世界

    public RenderTexture Initialize(Transform parent)
    {
        portalRT = new RenderTexture(512, 512, 16);
        portalRT.name = "PortalRT";

        // 建立 Portal 鏡頭
        var camGo = new GameObject("PortalCamera");
        camGo.transform.SetParent(parent, false);
        camGo.transform.localPosition = new Vector3(0, 2, -2);
        camGo.transform.localRotation = Quaternion.Euler(10, 0, 0);

        portalCamera = camGo.AddComponent<Camera>();
        portalCamera.targetTexture = portalRT;
        portalCamera.cullingMask = 1 << PortalLayer;
        portalCamera.clearFlags = CameraClearFlags.SolidColor;
        portalCamera.backgroundColor = new Color(0.02f, 0.01f, 0.05f, 1f);
        portalCamera.depth = -2;

        // URP 需要額外設定
        var urpData = camGo.GetComponent<UniversalAdditionalCameraData>();
        if (urpData == null) urpData = camGo.AddComponent<UniversalAdditionalCameraData>();
        urpData.renderType = CameraRenderType.Base;

        // 建立「另一個世界」的物件
        BuildPortalWorld(parent);

        Debug.Log("[PortalRenderer] Portal 鏡頭和世界建立完成。");

        return portalRT;
    }

    private void BuildPortalWorld(Transform parent)
    {
        portalWorldRoot = new GameObject("PortalWorld");
        portalWorldRoot.transform.SetParent(parent, false);
        portalWorldRoot.transform.localPosition = new Vector3(0, 2, 0);

        var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");

        // 外星地面
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "AlienGround";
        ground.transform.SetParent(portalWorldRoot.transform, false);
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale = new Vector3(2, 1, 2);
        SetLayer(ground, PortalLayer);
        var groundMat = new Material(unlitShader);
        groundMat.color = new Color(0.15f, 0.05f, 0.2f);
        ground.GetComponent<Renderer>().material = groundMat;

        // 外星柱子
        CreateAlienPillar(portalWorldRoot.transform, new Vector3(-2, 1.5f, 3), new Color(0.8f, 0.2f, 1f));
        CreateAlienPillar(portalWorldRoot.transform, new Vector3(2, 2f, 5), new Color(0.2f, 1f, 0.8f));
        CreateAlienPillar(portalWorldRoot.transform, new Vector3(0, 1f, 7), new Color(1f, 0.5f, 0.2f));

        // 外星球體
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "AlienOrb";
        sphere.transform.SetParent(portalWorldRoot.transform, false);
        sphere.transform.localPosition = new Vector3(0, 4, 6);
        sphere.transform.localScale = Vector3.one * 1.5f;
        SetLayer(sphere, PortalLayer);
        var orbMat = new Material(unlitShader);
        orbMat.color = new Color(1f, 0.7f, 0.3f);
        sphere.GetComponent<Renderer>().material = orbMat;

        // Portal 世界的光源
        var lightGo = new GameObject("PortalLight");
        lightGo.transform.SetParent(portalWorldRoot.transform, false);
        lightGo.transform.localPosition = new Vector3(0, 5, 3);
        lightGo.transform.localRotation = Quaternion.Euler(50, -30, 0);
        SetLayer(lightGo, PortalLayer);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.6f, 0.4f, 1f);
        light.intensity = 1.5f;
        light.cullingMask = 1 << PortalLayer;
    }

    private void CreateAlienPillar(Transform parent, Vector3 pos, Color color)
    {
        var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "AlienPillar";
        pillar.transform.SetParent(parent, false);
        pillar.transform.localPosition = pos;
        pillar.transform.localScale = new Vector3(0.4f, pos.y, 0.4f);
        SetLayer(pillar, PortalLayer);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = color;
        pillar.GetComponent<Renderer>().material = mat;
    }

    private void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            child.gameObject.layer = layer;
    }

    public void SetActive(bool active)
    {
        if (portalCamera != null) portalCamera.enabled = active;
        if (portalWorldRoot != null) portalWorldRoot.SetActive(active);
    }

    public void Cleanup()
    {
        if (portalRT != null)
        {
            portalRT.Release();
            Object.Destroy(portalRT);
        }
        if (portalCamera != null) Object.Destroy(portalCamera.gameObject);
        if (portalWorldRoot != null) Object.Destroy(portalWorldRoot);
    }
}
