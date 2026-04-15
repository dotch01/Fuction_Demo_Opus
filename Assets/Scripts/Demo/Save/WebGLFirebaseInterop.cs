using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// WebGL 端 Firebase JS SDK 橋接：透過 jslib 呼叫瀏覽器內的 Firebase。
/// 僅在 UNITY_WEBGL && !UNITY_EDITOR 下編譯。
/// </summary>
public static class WebGLFirebaseInterop
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] public static extern void FirebaseBridge_Init(string configJson);
    [DllImport("__Internal")] public static extern void FirebaseBridge_SignInAnonymously();
    [DllImport("__Internal")] public static extern void FirebaseBridge_SignInWithEmail(string email, string password);
    [DllImport("__Internal")] public static extern void FirebaseBridge_RegisterWithEmail(string email, string password);
    [DllImport("__Internal")] public static extern void FirebaseBridge_LinkEmailToAnonymous(string email, string password);
    [DllImport("__Internal")] public static extern void FirebaseBridge_SignOut();
    [DllImport("__Internal")] public static extern void FirebaseBridge_SignInWithGoogle();
    [DllImport("__Internal")] public static extern void FirebaseBridge_LinkGoogleToAnonymous();
    [DllImport("__Internal")] public static extern void FirebaseBridge_SaveToFirestore(string json);
    [DllImport("__Internal")] public static extern void FirebaseBridge_LoadFromFirestore();
    [DllImport("__Internal")] public static extern void FirebaseBridge_DeleteFromFirestore();
    [DllImport("__Internal")] public static extern string FirebaseBridge_GetUserId();
    [DllImport("__Internal")] public static extern int FirebaseBridge_IsAuthenticated();
    [DllImport("__Internal")] public static extern int FirebaseBridge_IsAnonymous();
#else
    // Editor / Standalone stubs — 不會被呼叫
    public static void FirebaseBridge_Init(string configJson) { }
    public static void FirebaseBridge_SignInAnonymously() { }
    public static void FirebaseBridge_SignInWithEmail(string email, string password) { }
    public static void FirebaseBridge_RegisterWithEmail(string email, string password) { }
    public static void FirebaseBridge_LinkEmailToAnonymous(string email, string password) { }
    public static void FirebaseBridge_SignOut() { }
    public static void FirebaseBridge_SignInWithGoogle() { }
    public static void FirebaseBridge_LinkGoogleToAnonymous() { }
    public static void FirebaseBridge_SaveToFirestore(string json) { }
    public static void FirebaseBridge_LoadFromFirestore() { }
    public static void FirebaseBridge_DeleteFromFirestore() { }
    public static string FirebaseBridge_GetUserId() => "";
    public static int FirebaseBridge_IsAuthenticated() => 0;
    public static int FirebaseBridge_IsAnonymous() => 0;
#endif
}
