using UnityEngine;

/// <summary>
/// 簡單的鏡頭跟隨：平滑追蹤目標，帶輕微預測。
/// 使用速度方向做前瞻，避免位置差造成的抖動。
/// </summary>
public class Demo2CameraFollow : MonoBehaviour
{
    private Transform _target;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private float lookAhead = 0.5f;
    [SerializeField] private float lookAheadSmooth = 5f;

    private Vector3 _velocity;
    private Vector2 _smoothLookAhead;

    public void Init(Transform target)
    {
        _target = target;
        // 初始化相機位置避免第一幀跳動
        if (target != null)
        {
            var pos = target.position;
            pos.z = transform.position.z;
            transform.position = pos;
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPos = _target.position;

        // 使用玩家速度方向做前瞻（而非相機→玩家方向，避免振動）
        var pc = Demo2PlayerController.Instance;
        Vector2 desiredAhead = Vector2.zero;
        if (pc != null && pc.IsMoving)
            desiredAhead = pc.Velocity.normalized * lookAhead;

        // 平滑前瞻值，避免方向切換時跳動
        _smoothLookAhead = Vector2.Lerp(_smoothLookAhead, desiredAhead,
            lookAheadSmooth * Time.deltaTime);

        targetPos += (Vector3)_smoothLookAhead;
        targetPos.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(
            transform.position, targetPos, ref _velocity, smoothTime);
    }
}
