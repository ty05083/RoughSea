using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class UI_RewardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("固定显示坐标")]
    public Vector2 showPos = new Vector2(-176, -140); // 你要的坐标
    [Header("奖励数字")]
    public int rewardNum; // 用于天平比较的数字
    [Header("天平区域")]
    public RectTransform balanceArea; // 你的透明检测区（保留不变）
    [Header("天平控制器关联")] // 新增：仅加这一个字段
    public UI_BalanceController balanceController; // 关联天平状态对象的控制器

    private RectTransform _rect;
    private Canvas _canvas;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        gameObject.SetActive(false); // 初始隐藏
    }

    // 外部调用：显示奖励图片（常驻，不自动隐藏）
    public void ShowRewardImage()
    {
        gameObject.SetActive(true);
        _rect.anchoredPosition = showPos;
        // 强制置顶，避免被对话面板遮挡
        transform.SetAsLastSibling();
    }

    // 开始拖拽：可选重置天平为平衡（新增，可选删除）
    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetAsLastSibling(); // 置顶不被遮挡
        // 重新拖拽时，天平恢复平衡（体验优化，可删）
        if (balanceController != null)
        {
            balanceController.ResetBalance();
        }
    }

    // 拖拽中跟随鼠标（保留不变）
    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.GetComponent<RectTransform>(),
            eventData.position,
            _canvas.worldCamera,
            out Vector2 localPos
        );
        _rect.anchoredPosition = localPos;
    }

    // 结束拖拽：检测透明区+调用天平比较（仅加2行代码）
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("===== 开始检测拖入区域 =====");
        Debug.Log($"透明检测区是否为空：{balanceArea == null}");
        Debug.Log($"鼠标松开位置（屏幕坐标）：{eventData.position}");

        if (balanceArea == null)
        {
            Debug.LogError("⚠️ balanceArea未赋值！请拖入透明检测区的RectTransform");
            return;
        }

        // 打印透明检测区的关键信息（排查坐标/大小）
        Debug.Log($"透明检测区坐标：{balanceArea.anchoredPosition}");
        Debug.Log($"透明检测区宽高：{balanceArea.sizeDelta}");
        Debug.Log($"透明检测区锚点：{balanceArea.anchorMin} | {balanceArea.anchorMax}");

        // 检测是否在区域内
        bool isInArea = RectTransformUtility.RectangleContainsScreenPoint(
            balanceArea,
            eventData.position,
            _canvas.worldCamera
        );

        Debug.Log($"是否拖入透明区：{isInArea}"); // 重点看这个值！

        if (isInArea)
        {
            Debug.Log($"✅ 拖入成功！奖励数字：{rewardNum}");
            if (balanceController != null)
            {
                balanceController.CompareRewardNumber(rewardNum);
            }
        }
        else
        {
            Debug.Log("❌ 未拖入透明区，奖励图片返回原位置");
            _rect.anchoredPosition = showPos;
        }
    }
}