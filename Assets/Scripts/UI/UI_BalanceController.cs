using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UI_BalanceController : MonoBehaviour
{
    [Header("天平基础配置")]
    public int balanceNumber; // 天平代表的数字（左盘）
    [Header("天平状态图片（叠放同位置）")]
    public Image img_Balance; // 平衡的天平图片
    public Image img_LeftTilt; // 向左倾斜的天平图片
    public Image img_RightTilt; // 向右倾斜的天平图片
    public UI_RewardDragHandler rewardDragHandler;
    public GameObject balancePanel;
    public int targetNumber = 10;
    public float hideDelayTime = 1f;
    void Awake()
    {
        // 初始化：默认显示平衡天平，隐藏其他状态
        InitBalanceImage();
    }

    /// <summary>
    /// 初始化天平图片状态
    /// </summary>
    private void InitBalanceImage()
    {
        SetImageActive(img_Balance, true);
        SetImageActive(img_LeftTilt, false);
        SetImageActive(img_RightTilt, false);
    }

    /// <summary>
    /// 核心方法：奖励数字与天平数字比较，切换对应状态图
    /// </summary>
    /// <param name="rewardNum">奖励图片的数字</param>
    public void CompareRewardNumber(int rewardNum)
    {
        // 1. 先重置UI
        ResetBalanceUI();

        // 2. 执行比较逻辑
        if (rewardNum < targetNumber)
        {
            // 奖励数字小 → 右边重
            if (img_RightTilt != null) img_RightTilt.gameObject.SetActive(true);
        }
        else if (rewardNum > targetNumber)
        {
            // 奖励数字大 → 左边重
            if (img_LeftTilt != null) img_LeftTilt.gameObject.SetActive(true);
        }
        else
        {
            // 相等
            if (img_Balance != null) img_Balance.gameObject.SetActive(true);
        }

        // 3. ✨ 核心修改：比较完成后隐藏天平面板（可选延迟，更流畅）
        // 方式1：立即隐藏
        StartCoroutine(DelayHideBalancePanel());
    }
    private IEnumerator DelayHideBalancePanel()
    {
        // 等待设置的延迟时间（默认1秒，可在面板修改）
        yield return new WaitForSeconds(hideDelayTime);
        // 等待结束后执行隐藏逻辑
        HideBalancePanel();
    }
    /// <summary>
    /// 隐藏所有天平状态图片
    /// </summary>
    private void HideAllBalanceImage()
    {
        SetImageActive(img_Balance, false);
        SetImageActive(img_LeftTilt, false);
        SetImageActive(img_RightTilt, false);
    }

    /// <summary>
    /// 便捷设置图片激活状态（自动判空，避免空引用报错）
    /// </summary>
    private void SetImageActive(Image img, bool isActive)
    {
        if (img != null && img.gameObject != null)
        {
            img.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// 重置天平为平衡状态（供外部调用，比如重新拖拽奖励图时）
    /// </summary>
    public void ResetBalance()
    {
        InitBalanceImage();
    }
    private void HideBalancePanel()
    {
        if (balancePanel != null)
        {
            balancePanel.SetActive(false);
            Debug.Log("✅ 天平面板已隐藏");
        }
        else
        {
            Debug.LogError("⚠️ balancePanel未赋值！请拖入天平面板父对象");
        }
    }

    // 可选：手动显示天平面板（比如重新触发天平时调用）
    public void ShowBalancePanel()
    {
        if (balancePanel != null)
        {
            balancePanel.SetActive(true);
            ResetBalanceUI(); // 显示时重置天平状态
        }
    }
    private void ResetBalanceUI()
    {
        if (img_LeftTilt != null) img_LeftTilt.gameObject.SetActive(false);
        if (img_RightTilt != null) img_RightTilt.gameObject.SetActive(false);
        if (img_Balance != null) img_Balance.gameObject.SetActive(false);
    }

}
