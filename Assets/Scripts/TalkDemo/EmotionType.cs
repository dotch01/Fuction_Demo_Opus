// ============================================================
// EmotionType.cs
// 共用情緒列舉，供 API 回應解析與動畫模組共同引用
// ============================================================

public enum EmotionType
{
    Neutral,    // 無標籤，預設
    Happy,      // [HAPPY]  → 跳動
    Sad,        // [SAD]    → 往下沈
    Surprised,  // [SURPRISED] → 往後搖
    Angry,      // [ANGRY]  → 左右搖晃
    Calm        // [CALM]   → 輕微浮動
}
