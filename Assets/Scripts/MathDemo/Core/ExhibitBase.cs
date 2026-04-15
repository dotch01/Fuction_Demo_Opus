using System.Collections.Generic;
using UnityEngine;

// ============================================================
// ExhibitBase.cs
// 所有展品的抽象基類 — 每個數學概念繼承此類
// ============================================================

public abstract class ExhibitBase : MonoBehaviour
{
    [Header("展品資訊")]
    public string exhibitName;
    public string description;
    public string formula; // 數學公式（顯示用）
    public string challengeDescription; // 挑戰任務說明

    [Header("互動")]
    protected List<DragHandle> dragHandles = new List<DragHandle>();
    protected bool isActive = false;
    protected bool challengeCompleted = false;
    protected bool challengeActive = false;

    // 展品佔據的世界位置（由 Manager 設定）
    [HideInInspector] public Vector3 exhibitWorldCenter;

    // --------------------------------------------------------
    // 生命週期
    // --------------------------------------------------------

    protected virtual void Awake()
    {
        // 子類可 override 做初始化
    }

    protected virtual void Update()
    {
        if (isActive)
        {
            UpdateVisualization();

            if (challengeActive && !challengeCompleted)
            {
                if (CheckChallengeComplete())
                {
                    challengeCompleted = true;
                    challengeActive = false;
                    OnChallengeCompleted();
                }
            }
        }
    }

    // --------------------------------------------------------
    // 展品啟用/停用（玩家走近/離開）
    // --------------------------------------------------------

    public void Activate()
    {
        isActive = true;
        foreach (var h in dragHandles) h.SetInteractable(true);
        OnExhibitActivated();
    }

    public void Deactivate()
    {
        isActive = false;
        foreach (var h in dragHandles) h.SetInteractable(false);
        OnExhibitDeactivated();
    }

    // --------------------------------------------------------
    // 子類必須實作
    // --------------------------------------------------------

    /// <summary>每幀更新視覺化（畫線、更新數值等）</summary>
    public abstract void UpdateVisualization();

    /// <summary>建立展品的 3D 物件、拖曳點等，由 Manager 呼叫</summary>
    public abstract void BuildExhibit();

    // --------------------------------------------------------
    // 子類可選擇覆寫
    // --------------------------------------------------------

    protected virtual void OnExhibitActivated() { }
    protected virtual void OnExhibitDeactivated() { }

    /// <summary>檢查挑戰是否完成</summary>
    public virtual bool CheckChallengeComplete() => false;

    protected virtual void OnChallengeCompleted()
    {
        Debug.Log($"[Exhibit] 挑戰完成: {exhibitName}");
    }

    // --------------------------------------------------------
    // 挑戰系統
    // --------------------------------------------------------

    public void StartChallenge()
    {
        if (challengeCompleted) return;
        challengeActive = true;
        OnChallengeStart();
    }

    protected virtual void OnChallengeStart() { }

    // --------------------------------------------------------
    // 工具方法
    // --------------------------------------------------------

    /// <summary>建立一個可拖曳的控制點球體</summary>
    protected DragHandle CreateDragHandle(Vector3 localPos, Color color, float radius = 0.15f, DragHandle.DragPlane plane = DragHandle.DragPlane.XY)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "DragHandle";
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * radius * 2f;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = color;
        go.GetComponent<Renderer>().material = mat;

        var handle = go.AddComponent<DragHandle>();
        handle.dragPlane = plane;
        handle.parentExhibit = this;

        dragHandles.Add(handle);
        return handle;
    }

    /// <summary>建立靜態 3D 文字標籤</summary>
    protected TextMesh CreateLabel(Vector3 localPos, string text, int fontSize = 40, Color? color = null)
    {
        var go = new GameObject("Label_" + text);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 0.05f;

        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = fontSize;
        tm.characterSize = 0.15f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color ?? Color.white;

        return tm;
    }

    /// <summary>建立一個靜態基本幾何體</summary>
    protected GameObject CreateStaticPrimitive(PrimitiveType type, Vector3 localPos, Vector3 localScale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = color;
        go.GetComponent<Renderer>().material = mat;

        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);

        return go;
    }
}
