using UnityEngine;

/// <summary>
/// 簡單的鏡頭跟隨：平滑追蹤目標，帶輕微預測。
/// </summary>
public class Demo2CameraFollow : MonoBehaviour
{
    private Transform _target;
    [SerializeField] private float smoothSpeed = 6f;
    [SerializeField] private float lookAhead = 0.8f;

    private Vector3 _velocity;

    public void Init(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // 輕微預測移動方向
        Vector3 targetPos = _target.position;
        var pc = Demo2PlayerController.Instance;
        if (pc != null && pc.IsMoving)
        {
            Vector2 pos = pc.Position;
            Vector2 ahead = (pos - (Vector2)transform.position).normalized * lookAhead;
            targetPos += (Vector3)ahead;
        }

        targetPos.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(
            transform.position, targetPos, ref _velocity, 1f / smoothSpeed);
    }
}
