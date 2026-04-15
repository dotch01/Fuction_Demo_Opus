using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 對話 UI：底部對話條 + 左側立繪框。
/// 點擊或空白鍵推進對話，播完呼叫 onDone callback。
/// 由 GameSetup 建立 UI 元素後透過 Init() 注入。
/// </summary>
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    private GameObject _root;       // 整個對話 UI 根節點
    private Image _portraitImage;   // 左側立繪
    private Text _speakerText;      // 角色名
    private Text _dialogueText;     // 對話內容

    private DialogueLine[] _lines;
    private int _lineIndex;
    private Action _onDone;

    public bool IsPlaying => _root != null && _root.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Init(GameObject root, Image portrait, Text speaker, Text dialogue)
    {
        _root = root;
        _portraitImage = portrait;
        _speakerText = speaker;
        _dialogueText = dialogue;
        _root.SetActive(false);
    }

    private void Update()
    {
        if (!IsPlaying) return;

        bool clicked = UnityEngine.InputSystem.Mouse.current != null &&
                       UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
        bool spaced = UnityEngine.InputSystem.Keyboard.current != null &&
                      UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
        bool touched = UnityEngine.InputSystem.Touchscreen.current != null &&
                       UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (clicked || spaced || touched)
            Advance();
    }

    /// <summary>播放一組對話行，結束後呼叫 onDone。</summary>
    public void PlayChapter(ChapterEntry chapter, Action onDone)
    {
        if (chapter == null || chapter.dialogues == null || chapter.dialogues.Length == 0)
        {
            onDone?.Invoke();
            return;
        }

        _lines = chapter.dialogues;
        _lineIndex = 0;
        _onDone = onDone;
        _root.SetActive(true);
        ShowCurrentLine();
    }

    /// <summary>播放單句對話（場景物件互動用）。</summary>
    public void PlayLines(DialogueLine[] lines, Action onDone = null)
    {
        if (lines == null || lines.Length == 0)
        {
            onDone?.Invoke();
            return;
        }

        _lines = lines;
        _lineIndex = 0;
        _onDone = onDone;
        _root.SetActive(true);
        ShowCurrentLine();
    }

    private void Advance()
    {
        _lineIndex++;
        if (_lineIndex >= _lines.Length)
        {
            _root.SetActive(false);
            _onDone?.Invoke();
            _onDone = null;
            return;
        }
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        var line = _lines[_lineIndex];
        _speakerText.text = line.speaker ?? "";
        _dialogueText.text = line.text ?? "";

        // 立繪：用顏色區分角色（正式版可載入 Sprite）
        if (!string.IsNullOrEmpty(line.portrait))
        {
            _portraitImage.gameObject.SetActive(true);
            _portraitImage.color = GetPortraitColor(line.portrait);
        }
        else
        {
            _portraitImage.gameObject.SetActive(false);
        }
    }

    /// <summary>Placeholder：用角色名 hash 產生不同顏色方塊。</summary>
    private static Color GetPortraitColor(string portrait)
    {
        int hash = portrait.GetHashCode();
        float h = Mathf.Abs(hash % 360) / 360f;
        return Color.HSVToRGB(h, 0.5f, 0.8f);
    }
}
