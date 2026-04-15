using UnityEngine;
#if UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;
#endif

// ============================================================
// TransparentWindowHelper.cs
// 靜態工具類：桌面透明視窗功能
//
// 使用前準備：
// 1. Edit > Project Settings > Player
//    - Resolution and Presentation > Fullscreen Mode > Windowed
//    - Run In Background ✓ 打勾
// 2. Edit > Project Settings > Player > Other Settings
//    - Color Space: Gamma（透明視窗需要）
// 3. Camera 的 Background 設成純黑色，Alpha = 0
//    （Clear Flags 設為 Solid Color）
// ============================================================

public static class TransparentWindowHelper
{
    // --------------------------------------------------------
    // Windows 原生 API 宣告
    // --------------------------------------------------------

#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hwnd, IntPtr hwndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(
        IntPtr hwnd, ref MARGINS margins);

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int left, right, top, bottom;
    }

    private const int GWL_STYLE         = -16;
    private const uint WS_POPUP         = 0x80000000;
    private const uint WS_VISIBLE       = 0x10000000;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_NOMOVE       = 0x0002;
    private const uint SWP_NOSIZE       = 0x0001;
    private static readonly IntPtr HWND_TOPMOST   = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
#endif

    // --------------------------------------------------------
    // 公開狀態
    // --------------------------------------------------------

    private static bool _isTransparentMode = false;
    public static bool IsTransparentMode => _isTransparentMode;

    // --------------------------------------------------------
    // 套用透明視窗
    // --------------------------------------------------------

    public static void Apply(bool borderless, bool alwaysOnTop)
    {
        if (Application.isEditor)
        {
            Debug.Log("[TalkDemo] Editor 模式，跳過透明視窗設定。");
            return;
        }

#if UNITY_STANDALONE_WIN
        ApplyWindows(borderless, alwaysOnTop);
        _isTransparentMode = true;
#elif UNITY_STANDALONE_OSX
        ApplyMac(borderless);
        _isTransparentMode = true;
#else
        Debug.Log("[TalkDemo] 此平台不支援透明視窗，使用一般模式。");
#endif
    }

    // --------------------------------------------------------
    // 還原一般視窗
    // --------------------------------------------------------

    public static void Restore()
    {
        if (Application.isEditor) return;

#if UNITY_STANDALONE_WIN
        RestoreWindows();
        _isTransparentMode = false;
#elif UNITY_STANDALONE_OSX
        if (Camera.main != null)
            Camera.main.backgroundColor = new Color(0, 0, 0, 1);
        Debug.Log("[TalkDemo] Mac：已還原不透明背景。");
        _isTransparentMode = false;
#endif
    }

    // --------------------------------------------------------
    // 切換透明／一般模式
    // --------------------------------------------------------

    public static void Toggle(bool borderless, bool alwaysOnTop)
    {
        if (_isTransparentMode)
            Restore();
        else
            Apply(borderless, alwaysOnTop);
    }

    // --------------------------------------------------------
    // Windows 實作
    // --------------------------------------------------------

#if UNITY_STANDALONE_WIN
    private static void ApplyWindows(bool borderless, bool alwaysOnTop)
    {
        IntPtr hwnd = GetActiveWindow();

        if (borderless)
            SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        var margins = new MARGINS { left = -1, right = -1, top = -1, bottom = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);

        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE);

        if (alwaysOnTop)
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE);

        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0, 0, 0, 0);
        }

        Debug.Log("[TalkDemo] Windows 透明視窗已套用。");
    }

    private static void RestoreWindows()
    {
        IntPtr hwnd = GetActiveWindow();
        uint normalStyle = 0x00CF0000; // WS_OVERLAPPEDWINDOW
        SetWindowLong(hwnd, GWL_STYLE, normalStyle);
        SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0,
            SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE);

        if (Camera.main != null)
            Camera.main.backgroundColor = new Color(0, 0, 0, 1);

        Debug.Log("[TalkDemo] Windows 視窗已還原正常模式。");
    }
#endif

    // --------------------------------------------------------
    // Mac 實作
    // --------------------------------------------------------

#if UNITY_STANDALONE_OSX
    private static void ApplyMac(bool borderless)
    {
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0, 0, 0, 0);
        }

        if (borderless)
            Screen.fullScreenMode = FullScreenMode.Windowed;

        Debug.Log("[TalkDemo] Mac 透明視窗已套用（需配合 Player Settings）。");
    }
#endif
}
