using UnityEngine;
using UnityEngine.InputSystem;

// ============================================================
// PlayerController.cs
// 第一人稱控制器：WASD 移動 + 右鍵按住旋轉視角 + 重力
// 游標永遠可見，左鍵直接與展品互動，WebGL 友好
// ============================================================

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.6f;
    public float gravity = -15f;
    public float jumpHeight = 1.2f;

    [Header("視角")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    private CharacterController cc;
    private Transform camTransform;
    private float verticalVelocity;
    private float cameraPitch;
    private bool lookEnabled = true;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("PlayerCamera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
        }

        camTransform = cam.transform;
        camTransform.SetParent(transform, false);
        camTransform.localPosition = new Vector3(0, 0.8f, 0);
        camTransform.localRotation = Quaternion.identity;

        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 200f;
        cam.fieldOfView = 70f;

        // 游標永遠可見，不鎖定
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleInput();
    }

    // --------------------------------------------------------
    // 視角：右鍵按住時旋轉
    // --------------------------------------------------------

    private void HandleLook()
    {
        if (!lookEnabled) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        // 只有按住右鍵時才旋轉視角
        if (!mouse.rightButton.isPressed) return;

        float mouseX = mouse.delta.x.ReadValue() * mouseSensitivity * 0.1f;
        float mouseY = mouse.delta.y.ReadValue() * mouseSensitivity * 0.1f;

        transform.Rotate(Vector3.up, mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        camTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }

    // --------------------------------------------------------
    // 移動
    // --------------------------------------------------------

    private void HandleMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float dt = Mathf.Min(Time.deltaTime, 0.05f);

        float h = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;

        float v = 0f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;

        Vector3 move = transform.right * h + transform.forward * v;
        float speed = moveSpeed;
        if (kb.leftShiftKey.isPressed) speed *= sprintMultiplier;

        if (cc.isGrounded)
        {
            verticalVelocity = -1f;
            if (kb.spaceKey.wasPressedThisFrame)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVelocity += gravity * dt;
        }

        Vector3 finalMove = move * speed * dt;
        finalMove.y = verticalVelocity * dt;
        cc.Move(finalMove);
    }

    // --------------------------------------------------------
    // 輸入
    // --------------------------------------------------------

    private void HandleInput()
    {
        var kb = Keyboard.current;

        if (kb != null && kb.eKey.wasPressedThisFrame)
        {
            var trigger = FindNearestTrigger();
            if (trigger != null && trigger.IsPlayerInside && trigger.exhibit != null)
                trigger.exhibit.StartChallenge();
        }
    }

    private ExhibitTrigger FindNearestTrigger()
    {
        ExhibitTrigger nearest = null;
        float minDist = float.MaxValue;
        foreach (var trigger in FindObjectsByType<ExhibitTrigger>(FindObjectsSortMode.None))
        {
            float dist = Vector3.Distance(transform.position, trigger.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = trigger;
            }
        }
        return nearest;
    }

    // --------------------------------------------------------
    // 外部控制（DragHandle 拖曳時暫停視角）
    // --------------------------------------------------------

    public void SetLookEnabled(bool enabled)
    {
        lookEnabled = enabled;
    }
}
