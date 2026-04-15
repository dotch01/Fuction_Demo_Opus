using UnityEngine;
using UnityEngine.InputSystem;

// ============================================================
// DragHandle.cs
// 3D 可拖曳控制點 — 滑鼠拖曳到指定平面上
// 使用 New Input System（Mouse.current）進行射線互動
// ============================================================

public class DragHandle : MonoBehaviour
{
    public enum DragPlane { XY, XZ, Free }

    public DragPlane dragPlane = DragPlane.XY;
    public ExhibitBase parentExhibit;
    public Vector3 minBounds = new Vector3(-5, -5, -5);
    public Vector3 maxBounds = new Vector3(5, 5, 5);

    private bool isDragging = false;
    private bool isHovered = false;
    private bool interactable = true;
    private Camera mainCam;
    private float dragDepth;
    private Renderer rend;
    private Color baseColor;
    private Color hoverColor;

    // 位置改變事件
    public System.Action<Vector3> onPositionChanged;

    void Start()
    {
        mainCam = Camera.main;
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            baseColor = rend.material.color;
            hoverColor = Color.Lerp(baseColor, Color.white, 0.4f);
        }

        // WebGL 需要 Rigidbody 才能讓動態建立的碰撞器被 Raycast 正確偵測
        if (GetComponent<Rigidbody>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
        if (!value)
        {
            if (isDragging) EndDrag();
            if (isHovered) SetHover(false);
        }
    }

    void Update()
    {
        if (!interactable || mainCam == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePos);
        // QueryTriggerInteraction.Ignore 避免 ExhibitTrigger 的 BoxCollider(isTrigger) 擋截射線
        bool hitSelf = Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore)
                       && hit.collider != null
                       && hit.collider.gameObject == gameObject;

        // 懸停偵測
        if (hitSelf && !isHovered)
            SetHover(true);
        else if (!hitSelf && !isDragging && isHovered)
            SetHover(false);

        // 開始拖曳
        if (hitSelf && mouse.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            dragDepth = mainCam.WorldToScreenPoint(transform.position).z;

            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.SetLookEnabled(false);
        }

        // 拖曳中
        if (isDragging)
        {
            Vector3 screenPos = new Vector3(mousePos.x, mousePos.y, dragDepth);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);

            Vector3 localPos = transform.parent.InverseTransformPoint(worldPos);

            switch (dragPlane)
            {
                case DragPlane.XY:
                    localPos.z = transform.localPosition.z;
                    break;
                case DragPlane.XZ:
                    localPos.y = transform.localPosition.y;
                    break;
                case DragPlane.Free:
                    break;
            }

            localPos.x = Mathf.Clamp(localPos.x, minBounds.x, maxBounds.x);
            localPos.y = Mathf.Clamp(localPos.y, minBounds.y, maxBounds.y);
            localPos.z = Mathf.Clamp(localPos.z, minBounds.z, maxBounds.z);

            transform.localPosition = localPos;
            onPositionChanged?.Invoke(localPos);
        }

        // 結束拖曳
        if (isDragging && mouse.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    private void SetHover(bool hover)
    {
        isHovered = hover;
        if (rend != null)
            rend.material.color = hover ? hoverColor : baseColor;
    }

    private void EndDrag()
    {
        isDragging = false;
        SetHover(false);

        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) pc.SetLookEnabled(true);
    }

    /// <summary>取得在父展品局部空間中的位置</summary>
    public Vector3 LocalPosition => transform.localPosition;
}
