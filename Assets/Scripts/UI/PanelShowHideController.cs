using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 面板显示/隐藏控制器
/// 点击？按钮显示面板，点击指定按钮隐藏面板
/// </summary>
public class PanelShowHideController : MonoBehaviour
{
    [Header("面板控制")]
    public GameObject panel;          // 需要显示/隐藏的面板对象
    public Button hidePanelButton;    // 用于隐藏面板的按钮
    public Button btn_call;
    void Start()
    {
        // 初始状态：隐藏面板
        panel.SetActive(false);
        // 绑定隐藏面板按钮的点击事件（确保按钮不为空）
        if (hidePanelButton != null)
        {
            hidePanelButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("未指定隐藏面板的按钮，请在Inspector中赋值hidePanelButton！");
        }
    }

    /// <summary>
    /// 显示面板（绑定到？按钮的OnClick事件）
    /// </summary>
    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    /// <summary>
    /// 隐藏面板（自动绑定到hidePanelButton）
    /// </summary>
    public void HidePanel()
    {
        panel.SetActive(false);
    }
}