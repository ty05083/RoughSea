using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class UI_DialogPanel : MonoBehaviour
{
   
    // 单例（全局访问面板UI）
    public static UI_DialogPanel Instance;
    public UI_DialogText uI_DialogText;
    private UI_HighLighter _highLighter;
    [Header("面板核心")]
    public TMP_Text dialogueTextHolder;  // 对话文本显示容器（最终文本显示在这里）
    public TextMeshProUGUI txtDialog;
    public SceneDialogConfig dialogConfig;
    public GameObject dialogPanel;
    private int _curGroupIdx = 0;
    private int _curLineIdx = 0;
    [Header("功能按钮")]
    public Button btn_PrevDialog;  // 回看上一句按钮
    public Button btn_NextDialog;  // 继续下一句按钮
    public Button btn_Highlighter; // 开启荧光笔按钮

    [Header("人物图片槽位")]
    public Image img_CharacterSlot;
    public Sprite defaultCharacterSprite; // 新增：声明默认人物图
    private RectTransform _txtRect;
    private Vector2 _initialOffsetMin;
    private Vector2 _initialOffsetMax;
    private Vector2 _initialAnchorMin;
    private Vector2 _initialAnchorMax;

    private Dictionary<string, bool> lockedGroups = new Dictionary<string, bool>();
    // Start is called before the first frame update

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        // 获取荧光笔单例（匹配UI_HighLighter的Instance）
        _highLighter = UI_HighLighter.Instance;

        
        InitTextAutoFit();
        // 初始化UI状态
        dialogPanel.SetActive(false); // 初始隐藏面板
        img_CharacterSlot.sprite = defaultCharacterSprite; // 人物槽默认图

        // 绑定按钮事件（仅转发到UI_DialogText）
        BindButtonEvents();
    }
    private void InitTextAutoFit()
    {
        txtDialog.enableAutoSizing = true;
        txtDialog.fontSizeMin = 16;
        txtDialog.fontSizeMax = 28;
        txtDialog.alignment = TextAlignmentOptions.Center;
        txtDialog.enableWordWrapping = true;

        RectTransform txtRect = txtDialog.GetComponent<RectTransform>();
        txtRect.sizeDelta = new Vector2(550, 250);
        txtRect.anchorMin = new Vector2(0.5f, 0.5f);
        txtRect.anchorMax = new Vector2(0.5f, 0.5f);
        txtRect.anchoredPosition = Vector2.zero;
    }
    // 荧光笔按钮点击逻辑（切换开启/关闭，并传递当前对话行）
    private void OnHighlighterClick()
    {
        var currentLine = UI_DialogText.Instance.GetCurrentDialogLine();
        if (currentLine == null) return;

     
    }

    // 辅助方法：获取当前对话行（匹配你的数据结构）
    // 修正返回类型
   
    public void UpdateDisplayText(string content)
{
    if (txtDialog == null) // txtDialog是绑定的文本组件
    {
        Debug.LogError("文本组件未绑定，无法更新内容");
        return;
    }
    txtDialog.text = content;
    Debug.Log($"面板文本已更新为：{content}"); // 新增日志验证
}
    // 绑定按钮：仅转发交互事件到逻辑层
    private void BindButtonEvents()
    {
        btn_PrevDialog.onClick.AddListener(() =>
        {
            if (UI_DialogText.Instance != null) UI_DialogText.Instance.OnPrevDialog();
        });

        btn_NextDialog.onClick.AddListener(() =>
        {
            if (UI_DialogText.Instance != null) UI_DialogText.Instance.OnNextDialog();
        });

        btn_Highlighter.onClick.AddListener(() =>
        {
            if (UI_DialogText.Instance != null)
            {
                OnHighlighterClick(); // 调用面板自身的荧光笔逻辑
            }
        });
    }

    // 显示/隐藏面板
    public void SetPanelActive(bool isActive)
    {
        Debug.Log($"UI_DialogPanel.SetPanelActive触发，目标状态：{isActive}");
        if (dialogPanel == null)
        {
            Debug.LogError("UI_DialogPanel的frame引用为空！");
            return;
        }

        // 强制激活父对象（如DialogPanel），避免层级阻碍
        if (dialogPanel.transform.parent != null)
        {
            GameObject parentObj = dialogPanel.transform.parent.gameObject;
            if (!parentObj.activeSelf)
            {
                parentObj.SetActive(true);
                Debug.Log($"激活frame的父对象：{parentObj.name}");
            }
        }

        dialogPanel.SetActive(isActive);
        Debug.Log($"frame最终状态：{dialogPanel.activeSelf}");
    }

    // 更新对话文本（由UI_DialogText调用，显示到面板的文本容器）
    public void UpdateDialogText(string text)
    {
        dialogueTextHolder.text = text;
    }

    // 更新人物槽图片（由UI_DialogText调用）
    public void UpdateCharacterSprite(Sprite sprite)
    {
        img_CharacterSlot.sprite = sprite ?? defaultCharacterSprite;
    }

    // 禁用/启用上一句按钮（由UI_DialogText调用）
    public void SetPrevButtonInteractable(bool interactable)
    {
        btn_PrevDialog.interactable = interactable;
    }
    

    // 上一页按钮逻辑（回看上一句）
  
   
    void Start()
    {

        _txtRect = txtDialog.GetComponent<RectTransform>();
        _initialOffsetMin = new Vector2(0f, 0f);
        _initialOffsetMax = new Vector2(0f, 0f);
        _initialAnchorMin = new Vector2(0f, 0f);
        _initialAnchorMax = new Vector2(1f, 1f);

        ResetLayoutToNegative();
        RefreshDialogText();

    }
    private void ResetLayoutToNegative()
    {
        if (_txtRect == null) return;
        _txtRect.anchorMin = _initialAnchorMin;
        _txtRect.anchorMax = _initialAnchorMax;
        _txtRect.offsetMin = _initialOffsetMin;
        _txtRect.offsetMax = _initialOffsetMax;
    }

    // 刷新文本显示（核心：将配置文字赋值给已有文本框）
    private void RefreshDialogText()
    {
        if (txtDialog == null)
        {
            Debug.LogError("txtDialog为空！");
            return;
        }
        if (_txtRect == null && txtDialog != null)
        {
            _txtRect = txtDialog.GetComponent<RectTransform>();
            if (_txtRect == null)
            {
                Debug.LogError("txtDialog上无RectTransform组件（UI对象默认存在，此情况异常）");
                return;
            }
        }
        string content = dialogConfig.GetDialogContent(_curGroupIdx, _curLineIdx);
        txtDialog.text = content;

        // 仅保留字体相关配置（不涉及布局）
        txtDialog.enableAutoSizing = true;
        txtDialog.fontSizeMin = 16;
        txtDialog.fontSizeMax = 28;

        // 仅保留文本排版配置（不涉及布局）
        txtDialog.alignment = TextAlignmentOptions.Left;
        txtDialog.enableWordWrapping = true;
        txtDialog.overflowMode = TextOverflowModes.Truncate;
        txtDialog.color = Color.black;

        _txtRect.offsetMin = _initialOffsetMin;
        _txtRect.offsetMax = _initialOffsetMax;
        _txtRect.anchorMin = _initialAnchorMin;
        _txtRect.anchorMax = _initialAnchorMax;
        ResetLayoutToNegative();
    }
    // Update is called once per frame

    void Update()
    {

    }
}