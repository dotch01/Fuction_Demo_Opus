using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// 本地存檔提供者：將 GameSaveData 序列化為 JSON 存放於 persistentDataPath。
/// 含 SHA256 checksum 驗證完整性。
/// </summary>
public class LocalSaveProvider : ISaveProvider
{
    private readonly string _filePath;

    public LocalSaveProvider(string fileName = "save.json")
    {
        _filePath = Path.Combine(Application.persistentDataPath, fileName);
    }

    public bool Save(GameSaveData data)
    {
        try
        {
            data.lastSavedUtc = DateTime.UtcNow.ToString("o");
            data.checksum = null; // 先清空再計算
            string json = JsonUtility.ToJson(data, true);
            data.checksum = ComputeChecksum(json);
            json = JsonUtility.ToJson(data, true);

            File.WriteAllText(_filePath, json, Encoding.UTF8);
            Debug.Log($"[LocalSave] 存檔成功: {_filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalSave] 存檔失敗: {e.Message}");
            return false;
        }
    }

    public GameSaveData Load()
    {
        try
        {
            if (!File.Exists(_filePath)) return null;

            string json = File.ReadAllText(_filePath, Encoding.UTF8);
            var data = JsonUtility.FromJson<GameSaveData>(json);

            // 驗證 checksum
            string savedChecksum = data.checksum;
            data.checksum = null;
            string verifyJson = JsonUtility.ToJson(data, true);
            string computed = ComputeChecksum(verifyJson);

            if (savedChecksum != computed)
            {
                Debug.LogWarning("[LocalSave] Checksum 不符，存檔可能已損壞或被修改");
                // 仍回傳資料，讓上層決定是否使用（雲端版本優先）
                data.checksum = savedChecksum;
            }
            else
            {
                data.checksum = savedChecksum;
            }

            Debug.Log("[LocalSave] 讀檔成功");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalSave] 讀檔失敗: {e.Message}");
            return null;
        }
    }

    public void Delete()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                Debug.Log("[LocalSave] 存檔已刪除");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalSave] 刪除失敗: {e.Message}");
        }
    }

    public bool HasSave() => File.Exists(_filePath);

    private static string ComputeChecksum(string content)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        var sb = new StringBuilder(64);
        foreach (byte b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
