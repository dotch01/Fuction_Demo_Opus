/// <summary>
/// 存檔提供者介面：統一本地和雲端的讀寫 API。
/// </summary>
public interface ISaveProvider
{
    bool Save(GameSaveData data);
    GameSaveData Load();
    void Delete();
    bool HasSave();
}
