using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI; // 新增这行
using System.Linq;
using System;
using System.Text.RegularExpressions;
[System.Serializable]
public class KeyWord2Reward
{
    public string keyWord;        // 匹配的关键词（和荧光笔选中的一致）
    public Sprite rewardSprite;   // 对应的奖励图片
    public int rewardNumber;      // 对应的奖励数值（供天平比较）
}
public class UI_HighLighter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private bool _isKeywordConfirmed = false;
    public List<string> keywords;
    // 单例
    public static UI_HighLighter Instance;

    // 配置项（Inspector可编辑）
    public TMP_Text targetText;          // 要高亮的文本
    public Color highlightColor = Color.yellow; // 高亮颜色
    public bool isHighlighterActive;     // 荧光笔激活状态

    // 内部变量
    private int selectStartIndex = -1;   // 选中起始字符索引
    private int selectEndIndex = -1;     // 选中结束字符索引
    private Color originalColor;         // 文本原始颜色
    [Header("奖励图片关联")]
    public UI_RewardDragHandler rewardImageCtrl; // 拖入RewardImage上的该脚本
    public UI_DialogReward dialogRewardCtrl; // 确保这个变量名在整个类中一致
    [Header("关键词-奖励映射配置")]
    public List<KeyWord2Reward> keyWord2RewardList;
    // 新增：内存映射字典，提高查询效率
    private Dictionary<string, KeyWord2Reward> _keyWord2RewardDic;
    void Awake()
    {

        // 单例初始化
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 自动添加必要组件
        if (!GetComponent<Canvas>()) gameObject.AddComponent<Canvas>();
        if (!GetComponent<GraphicRaycaster>()) gameObject.AddComponent<GraphicRaycaster>();
        // ===== 新增：初始化关键词-奖励字典 =====
        InitKeyWord2RewardDic();
    }
    private void InitKeyWord2RewardDic()
    {
        _keyWord2RewardDic = new Dictionary<string, KeyWord2Reward>();
        if (keyWord2RewardList == null || keyWord2RewardList.Count == 0)
        {
            Debug.LogWarning("关键词-奖励映射列表为空！请在Inspector面板配置");
            return;
        }
        // 遍历配置，转小写去空格存入字典（和匹配逻辑一致）
        foreach (var item in keyWord2RewardList)
        {
            string key = Regex.Replace(item.keyWord, @"\s+", "").ToLower();
            if (!_keyWord2RewardDic.ContainsKey(key))
                _keyWord2RewardDic.Add(key, item);
            else
                Debug.LogError($"重复关键词：{item.keyWord}，请检查配置列表");
        }
    }
    void Start()
    {
        // 记录文本原始颜色（修正逻辑判断与日志）
        if (targetText != null)
        {
            originalColor = targetText.color;
            targetText.raycastTarget = true;
            Debug.Log("TargetText绑定成功，已开启射线检测");
        }
        else
        {
            Debug.LogError("TargetText未在Inspector中绑定！请绑定TMP文本组件");
            isHighlighterActive = false; // 未绑定文本时禁用荧光笔
        }
    }
    public void ToggleHighlighterState()
    {
        isHighlighterActive = !isHighlighterActive;
        Debug.Log("荧光笔状态切换：" + isHighlighterActive);
    }
    private string GetHighlightedText()
    {
        if (targetText == null || selectStartIndex == -1 || selectEndIndex == -1)
        {
            return string.Empty;
        }
        // 确定起始和结束索引的正确顺序
        int start = Mathf.Min(selectStartIndex, selectEndIndex);
        int end = Mathf.Max(selectStartIndex, selectEndIndex);
        // 截取选中文本（TMP文本需用textInfo获取正确字符范围）
        return targetText.text.Substring(start, end - start + 1).Trim();
    }
    // 外部调用：切换荧光笔状态
    public void ToggleHighlighter(bool isActive, int keywordsCount)
    {

        isHighlighterActive = isActive;
      
        Debug.Log($"最终使用的关键词：{string.Join(",", this.keywords)}");// 验证是否有值
        if (!isActive) // 仅关闭时验证，逻辑倒置
        {
            _isKeywordConfirmed = false;
            ResetHighlight();
            selectStartIndex = -1;
            selectEndIndex = -1;
        }

    }

    // 鼠标按下：记录起始索引
    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. 基础校验：仅激活状态且目标文本不为空时执行
        if (!isHighlighterActive || targetText == null) return;

        // 2. 核心操作：获取鼠标点击位置对应的字符索引（Overlay模式相机传null）
        int charIndex = TMP_TextUtilities.FindIntersectingCharacter(
            targetText, eventData.position, eventData.pressEventCamera, true // 最后一个参数true：忽略富文本标签
        );

        // 3. 索引有效性判断
        if (charIndex != -1)
        {
            selectStartIndex = charIndex;
            selectEndIndex = charIndex;
            Debug.Log($"成功获取起始索引：{charIndex}，选中范围：{selectStartIndex} ~ {selectEndIndex}");
            UpdateHighlight(); // 按下时立即更新高亮（显示单个字符）
        }
        else
        {
            Debug.Log("未命中任何可见字符（可能点击了文本空白区域）");
            // 未命中时重置索引，避免残留旧值
            selectStartIndex = -1;
            selectEndIndex = -1;
            ResetHighlight();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isHighlighterActive || targetText == null || selectStartIndex == -1) return;

        int charIndex = TMP_TextUtilities.FindIntersectingCharacter(
            targetText, eventData.position, eventData.pressEventCamera, true);

        if (charIndex != -1)
        {
            selectEndIndex = charIndex;
            Debug.Log($"选中范围：{selectStartIndex} ~ {selectEndIndex}，截取文本：{GetHighlightedText()}");
            UpdateHighlight();
        }
    }

    // 鼠标抬起：确认选中范围
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isHighlighterActive || targetText == null) return;
        Debug.Log($"选中范围：{selectStartIndex} ~ {selectEndIndex}");

        // 仅保留索引重置
        if (!_isKeywordConfirmed)
        {

        }
    }
    public void ResetKeywordConfirmation()
    {
        _isKeywordConfirmed = false;
        ResetHighlight(); // 重置高亮（因_isKeywordConfirmed已设为false，会执行恢复颜色）
        selectStartIndex = -1;
        selectEndIndex = -1;
    }
    public void ResetAllHighlighterState(List<string> newKeywords = null)
    {
        _isKeywordConfirmed = false;
        selectStartIndex = -1;
        selectEndIndex = -1;
        ResetHighlight();
        isHighlighterActive = true;

        // 新增：同步新关键词
        if (newKeywords != null)
        {
            this.keywords = newKeywords;
            Debug.Log($"已同步新关键词：{string.Join(",", newKeywords)}");
        }
        Debug.Log("荧光笔状态已重置");
    }
    // 核心：更新文本高亮
    private void UpdateHighlight()
    {
        if (targetText == null || selectStartIndex == -1) return;

        targetText.ForceMeshUpdate();
        TMP_TextInfo textInfo = targetText.textInfo;

        // 重置所有字符颜色
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            Color32[] vertexColors = textInfo.meshInfo[matIndex].colors32;
            for (int j = 0; j < 4; j++)
            {
                vertexColors[charInfo.vertexIndex + j] = targetText.color;
            }
        }

        // 高亮选中字符
        int start = Mathf.Min(selectStartIndex, selectEndIndex);
        int end = Mathf.Max(selectStartIndex, selectEndIndex);
        for (int i = start; i <= end; i++)
        {
            if (i >= textInfo.characterCount) break;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            Color32[] vertexColors = textInfo.meshInfo[matIndex].colors32;
            for (int j = 0; j < 4; j++)
            {
                vertexColors[charInfo.vertexIndex + j] = highlightColor;
            }
        }

        // 应用修改
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            targetText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
    // 重置所有高亮
    private void ResetHighlight()
    {
        if (_isKeywordConfirmed) return;

        if (targetText == null) return;

        targetText.ForceMeshUpdate();
        TMP_TextInfo textInfo = targetText.textInfo;

        // 恢复所有字符为原始颜色
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            Color32[] vertexColors = textInfo.meshInfo[matIndex].colors32;

            for (int j = 0; j < 4; j++)
            {
                vertexColors[charInfo.vertexIndex + j] = originalColor;
            }
        }

        // 应用恢复后的颜色
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            targetText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
    public void OnKeywordHighlightSuccess(string matchedKey)
    {
        if (rewardImageCtrl == null)
        {
            Debug.LogError("未关联奖励图片控制器UI_RewardDragHandler！");
            return;
        }
        if (dialogRewardCtrl == null)
        {
            Debug.LogError("未关联奖励面板控制器UI_DialogReward！");
            return;
        }
        if (_keyWord2RewardDic == null || !_keyWord2RewardDic.ContainsKey(matchedKey))
        {
            Debug.LogError($"未找到关键词【{matchedKey}】对应的奖励配置！");
            return;
        }

        // 2. 获取匹配的奖励数据
        KeyWord2Reward rewardData = _keyWord2RewardDic[matchedKey];

        // 3. 给奖励拖拽脚本赋值【奖励数值】（供天平比较）
        rewardImageCtrl.rewardNum = rewardData.rewardNumber;
        // 4. 给奖励面板赋值【奖励图片】并显示
        dialogRewardCtrl.ShowReward(rewardData.rewardSprite);
        // 5. 显示奖励拖拽图片（原有逻辑）
        rewardImageCtrl.ShowRewardImage();

        Debug.Log($"✅ 奖励赋值成功！关键词：{matchedKey} | 奖励数值：{rewardData.rewardNumber} | 奖励图片：{rewardData.rewardSprite.name}");
    }
    void Update()
    {
        if (!isHighlighterActive || _isKeywordConfirmed) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            string highlightedText = GetHighlightedText().Trim();
            if (string.IsNullOrEmpty(highlightedText))
            {
                Debug.LogWarning("未选中有效文本，请重新选择关键词");
                return;
            }

            // 标准化文本（去空格+小写）
            string normalizedText = Regex.Replace(highlightedText, @"\s+", "").ToLower();
            Debug.Log($"标准化待匹配文本：{normalizedText}");

            if (this.keywords == null || this.keywords.Count == 0)
            {
                Debug.LogError("UI_HighLighter的keywords列表为空，请检查是否从对话行传递");
                return;
            }

            // 关键词匹配
            bool isMatch = this.keywords.Any(k =>
                Regex.Replace(k, @"\s+", "").ToLower() == normalizedText
            );

            if (isMatch)
            {
                Debug.Log("✅ 关键词匹配成功！准备显示奖励图片");
                _isKeywordConfirmed = true;
                ResetHighlight();
                ResetAllHighlighterState();

                // ========== 核心修复：触发奖励图片显示 ==========
                OnKeywordHighlightSuccess(normalizedText);

                // （保留原有奖励面板激活逻辑）
                UI_DialogReward rewardUI = FindObjectOfType<UI_DialogReward>();
                if (rewardUI != null && rewardUI.rewardPanel != null)
                {
                    rewardUI.rewardPanel.SetActive(true);
                    Debug.Log("奖励面板已激活");
                }
            }
            else
            {
                Debug.Log($"❌ 匹配失败，当前关键词列表：{string.Join(",", this.keywords)}");
                selectStartIndex = -1;
                selectEndIndex = -1;
                ResetHighlight();

            }
        }
    }
}