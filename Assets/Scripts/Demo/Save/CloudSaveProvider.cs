using System;
using UnityEngine;
#if FIREBASE_FIRESTORE
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
#endif

/// <summary>
/// Firebase Firestore 雲端存檔提供者。
/// 需先匯入 Firebase Firestore SDK 並在 Player Settings 加入 FIREBASE_FIRESTORE 定義。
/// </summary>
public class CloudSaveProvider : ISaveProvider
{
    private readonly string _userId;

#if FIREBASE_FIRESTORE
    private FirebaseFirestore _db;
    private DocumentReference _saveDoc;
#endif

    public CloudSaveProvider(string userId)
    {
        _userId = userId;

#if FIREBASE_FIRESTORE
        _db = FirebaseFirestore.DefaultInstance;
        _saveDoc = _db.Collection("users").Document(_userId).Collection("saves").Document("main");
#endif
    }

    public bool Save(GameSaveData data)
    {
#if FIREBASE_FIRESTORE
        try
        {
            string json = JsonUtility.ToJson(data);
            var dict = new Dictionary<string, object>
            {
                { "json", json },
                { "lastSavedUtc", data.lastSavedUtc },
                { "version", data.version },
                { "updatedAt", FieldValue.ServerTimestamp }
            };

            _saveDoc.SetAsync(dict).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"[CloudSave] 寫入失敗: {task.Exception}");
                else
                    Debug.Log("[CloudSave] 雲端存檔成功");
            });

            return true; // fire-and-forget，非同步進行
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] 存檔例外: {e.Message}");
            return false;
        }
#else
        return false;
#endif
    }

    public GameSaveData Load()
    {
#if FIREBASE_FIRESTORE
        // 同步讀取不適合 Unity，此方法回傳 null，使用 LoadAsync 代替
        Debug.LogWarning("[CloudSave] 請使用 LoadAsync 進行非同步讀取");
        return null;
#else
        return null;
#endif
    }

    /// <summary>非同步讀取雲端存檔。</summary>
    public void LoadAsync(Action<GameSaveData> callback)
    {
#if FIREBASE_FIRESTORE
        _saveDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[CloudSave] 讀取失敗: {task.Exception}");
                callback?.Invoke(null);
                return;
            }

            var snapshot = task.Result;
            if (!snapshot.Exists || !snapshot.ContainsField("json"))
            {
                callback?.Invoke(null);
                return;
            }

            string json = snapshot.GetValue<string>("json");
            var data = JsonUtility.FromJson<GameSaveData>(json);
            Debug.Log("[CloudSave] 雲端讀檔成功");
            callback?.Invoke(data);
        });
#else
        callback?.Invoke(null);
#endif
    }

    public void Delete()
    {
#if FIREBASE_FIRESTORE
        _saveDoc.DeleteAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                Debug.LogError($"[CloudSave] 刪除失敗: {task.Exception}");
            else
                Debug.Log("[CloudSave] 雲端存檔已刪除");
        });
#endif
    }

    public bool HasSave()
    {
        // 雲端無法同步判斷，SaveManager 使用 LoadAsync 取代
        return false;
    }
}
