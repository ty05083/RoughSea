using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class UI_DialogText : MonoBehaviour
{
    // 单例（全局唯一逻辑层）
    public static UI_DialogText Instance;
    private UI_HighLighter _highLighter;
    public List<UI_DialogText.DialogueLine> dialogLines; // 对话行列表
    private UI_HighLighter _highlighter;
    [System.Serializable]
    // 单条对话行（单个文本）
    public class DialogueLine
    {
        public string content; // 对话文本
        public string lineId; // 行 ID
        public Sprite characterSprite; // 人物图
        public List<string> keywords; // 关键词（可为空）
        public Sprite rewardSprite; // 奖励图
    }
    [System.Serializable]
    // 对话组（多个文本组成）
    public class SceneDialogueGroup
    {
        public string groupId; // 组 ID
        public List<DialogueLine> lines; // 该组的所有文本行
        public string preTriggerGroupId; // 前置组 ID（可为空）
    }
    private string currentSceneGameObjectId; // 当前激活的GameObject场景ID
    private Dictionary<string, List<SceneDialogConfig.SceneDialogueGroup>> sceneDialogMap = new Dictionary<string, List<SceneDialogConfig.SceneDialogueGroup>>();// 场景-对话组
    private Dictionary<string, bool> triggeredGroups = new Dictionary<string, bool>(); // 已触发的
    private string currentSceneName; // 当前场景
    private Dictionary<string, List<string>> sceneTriggeredDialogMap = new Dictionary<string, List<string>>();
    private int currentGroupIndex = 0; // 当前组索引
    private int currentLineIndex = 0; // 当前行索引
    private bool isHighlighterActive = false; // 荧光笔状态
    private bool isNewGameTriggered = false; // NewGame 触发标记
    private string currentScene; // 当前场景名
    [Header("全局统一对话配置")]
    public SceneDialogConfig globalSceneDialogConfig; // 全局唯一SceneDialogConfig（挂载在全局Text组件上）
    private List<SceneDialogConfig.SceneDialogueGroup> allGlobalDialogGroups; // 全局所有对话组
    // 缓存当前场景的对话组（筛选后）
    private List<SceneDialogConfig.SceneDialogueGroup> currentSceneDialogGroups;
    [Header("场景GameObject配置")]
    public string sceneGameObjectTag = "SceneGameObject"; // 标记场景GameObject的统一标签
    public bool hideDialogOnSceneJump = true; // 控制GameObject跳转时是否隐藏对话框
    void Awake()
    {
        Debug.Log("UI_DialogText Awake开始执行");
        if (Instance != null && Instance != this)
        {
            Debug.Log("已存在UI_DialogText实例，销毁当前对象");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 强制全局持久化
        Debug.Log("UI_DialogText实例创建成功，Instance已赋值");
        SceneDialogConfig dialogConfig = GetDialogConfigFromTextComponent();
        LoadGlobalDialogConfig();

        // 原有初始化分场景对话记录字典保留
        if (sceneTriggeredDialogMap == null)
        {
            sceneTriggeredDialogMap = new Dictionary<string, List<string>>();
        }
        _highlighter = UI_HighLighter.Instance;
        currentSceneName = SceneManager.GetActiveScene().name;
        currentScene = currentSceneName;

        if (dialogConfig != null)
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            sceneDialogMap[currentSceneName] = dialogConfig.sceneDialogGroups;
            Debug.Log($"初始加载[{currentSceneName}]对话组：{dialogConfig.sceneDialogGroups.Count}组");
        }

        // 2. 强制获取当前场景名作为 key
        currentScene = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(currentScene))
        {
            Debug.LogError("无法获取当前场景名，默认使用'GameStart");
            currentScene = "GameStart"; // 手动指定默认场景名
        }
        // 3. 确保字典中存在该 Key
        if (!sceneDialogMap.ContainsKey(currentScene))
        {
            // 使用完整类型名 SceneDialogConfig.SceneDialogueGroup
            sceneDialogMap.Add(currentScene, new List<SceneDialogConfig.SceneDialogueGroup>());
        }
        // 监听场景加载：切换场景后加载对应对话组
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void FilterDialogGroupsByCurrentScene()
    {
        currentSceneDialogGroups.Clear();
        if (string.IsNullOrEmpty(currentSceneGameObjectId) || allGlobalDialogGroups.Count == 0)
        {
            Debug.LogWarning("无法筛选对话组：场景ID为空或全局对话组无数据");
            return;
        }

        // 筛选逻辑：保留「所属场景ID」与当前场景ID一致的对话组
        foreach (var dialogGroup in allGlobalDialogGroups)
        {
            if (dialogGroup.belongSceneId == currentSceneGameObjectId)
            {
                currentSceneDialogGroups.Add(dialogGroup);
            }
        }

        Debug.Log($"当前GameObject场景[{currentSceneGameObjectId}]筛选出{currentSceneDialogGroups.Count}个对话组");
        // 重置当前场景的组索引（避免跨场景索引混乱）
        currentGroupIndex = 0;
    }

    private void LoadGlobalDialogConfig()
    {
        // 1. 自动查找全局Config（若未手动赋值，兜底查找）
        if (globalSceneDialogConfig == null)
        {
            globalSceneDialogConfig = FindObjectOfType<SceneDialogConfig>();
        }

        // 2. 校验全局Config是否有效
        if (globalSceneDialogConfig == null)
        {
            Debug.LogError("未找到全局唯一的SceneDialogConfig！请在全局Text组件上挂载并赋值");
            allGlobalDialogGroups = new List<SceneDialogConfig.SceneDialogueGroup>();
            currentSceneDialogGroups = new List<SceneDialogConfig.SceneDialogueGroup>();
            return;
        }

        // 3. 加载所有全局对话组，缓存起来
        allGlobalDialogGroups = globalSceneDialogConfig.allSceneDialogGroups;
        if (allGlobalDialogGroups == null || allGlobalDialogGroups.Count == 0)
        {
            Debug.LogWarning("全局Config中无任何对话组，请配置allSceneDialogGroups");
            currentSceneDialogGroups = new List<SceneDialogConfig.SceneDialogueGroup>();
            return;
        }

        Debug.Log($"成功加载全局对话配置，共包含{allGlobalDialogGroups.Count}个对话组（覆盖所有GameObject场景）");
        currentSceneDialogGroups = new List<SceneDialogConfig.SceneDialogueGroup>();
    }

    private SceneDialogConfig GetDialogConfigFromTextComponent()
    {

        TextMeshProUGUI[] tmpTexts = FindObjectsOfType<TextMeshProUGUI>(includeInactive: true);
        foreach (var tmpText in tmpTexts)
        {
            SceneDialogConfig config = tmpText.GetComponent<SceneDialogConfig>();
            if (config != null)
            {
                Debug.Log($"找到绑定Config的Text组件：{tmpText.gameObject.name}");
                return config;
            }
        }
        Debug.LogError("未找到绑定SceneDialogConfig的Text组件!");
        return null;
    }

private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        currentScene = currentSceneName;
        currentLineIndex = 0;
        // NewGame 已触发时，自动显示符合条件的对话
        SceneDialogConfig dialogConfig = FindObjectOfType<SceneDialogConfig>();
        if (dialogConfig != null)
        {
            if (sceneDialogMap.ContainsKey(currentSceneName))
                sceneDialogMap[currentSceneName] = dialogConfig.sceneDialogGroups;
            else
                sceneDialogMap.Add(currentSceneName, dialogConfig.sceneDialogGroups);
        }

        // NewGame触发时，显示符合条件的对话（保留原有逻辑）
        if (isNewGameTriggered)
        {
            ShowValidDialog();
            // 新增：强制激活对话框
            UI_DialogPanel.Instance.SetPanelActive(true);
        }
    }
 
    // NewGame 触发：显示对话面板
    public void TriggerNewGameDialog()
    {
        isNewGameTriggered = true;
        UI_DialogPanel.Instance.SetPanelActive(true); // 通知面板显示
        ShowValidDialog(); // 显示符合条件的对话
    }
    // 隐藏对话面板（由 UI_Loading 调用）
    public void HideDialogPanel()
    {
        UI_DialogPanel.Instance.SetPanelActive(false);
        isHighlighterActive = false;
        if (UI_DialogText.Instance != null)
        {
            UI_DialogText.Instance.TriggerNewGameDialog();
        }
    }
    // 显示符合条件的对话（无前置 或 前置组已触发）
    private void ShowValidDialog()
    {
        if (!sceneDialogMap.ContainsKey(currentSceneName) || sceneDialogMap[currentSceneName].Count == 0)
        {
            Debug.LogWarning($"场景[{currentSceneName}]无对话组数据");
            return;
        }

        var sceneGroups = sceneDialogMap[currentSceneName];
        // 关键修复：从当前组索引之后遍历，找下一个满足条件的组
        for (int i = currentGroupIndex; i < sceneGroups.Count; i++)
        {
            var group = sceneGroups[i];
            bool preConditionMet = string.IsNullOrEmpty(group.preTriggerGroupId)
                                  || triggeredGroups.ContainsKey(group.preTriggerGroupId);

            if (preConditionMet)
            {
                currentGroupIndex = i;
                currentLineIndex = 0; // 重置为组第一句
                ShowCurrentDialogLine();
                // 新增：确保对话框显示
                UI_DialogPanel.Instance.SetPanelActive(true);
                Debug.Log($"激活对话组：{group.groupId}（场景[{currentSceneName}]，索引[{i}]）");
                break;
            }
        }
    }
    // 显示当前对话行（核心：更新面板文本+人物图）
    private void ShowCurrentDialogLine()
    {
        if (currentSceneDialogGroups.Count == 0 || currentGroupIndex >= currentSceneDialogGroups.Count)
        {
            Debug.LogWarning("当前场景无有效对话组，无法显示对话");
            return;
        }

        var currentGroup = currentSceneDialogGroups[currentGroupIndex];
        if (currentGroup.lines == null || currentGroup.lines.Count == 0)
        {
            Debug.LogWarning($"对话组[{currentGroup.groupId}]无有效对话内容");
            return;
        }

        // 索引越界兜底（确保显示有效句子）
        if (currentLineIndex < 0) currentLineIndex = 0;
        if (currentLineIndex >= currentGroup.lines.Count) currentLineIndex = currentGroup.lines.Count - 1;

        var currentLine = currentGroup.lines[currentLineIndex];
        if (UI_DialogPanel.Instance != null && UI_DialogPanel.Instance.txtDialog != null)
        {
            // 强制更新文本（即使内容相同也刷新，避免缓存问题）
            UI_DialogPanel.Instance.txtDialog.text = currentLine.content;
            Debug.Log($"显示对话：组[{currentGroup.groupId}]第{currentLineIndex + 1}句 - {currentLine.content}");
        }
        else
        {
            Debug.LogError("UI_DialogPanel或txtDialog引用为空，无法显示对话");
        }
    }
    // 回看上一句
    public void OnPrevDialog()
    {
        if (currentLineIndex > 0)
        {
            currentLineIndex--;
            ShowCurrentDialogLine();
        }
    }
    public bool ToggleHighlighter()
    {
        var currentLine = GetCurrentDialogLine(); // 获取当前对话行（SceneDialogConfig.SceneDialogueLine）
        if (currentLine == null) return false;

        isHighlighterActive = !isHighlighterActive;
        if (UI_HighLighter.Instance != null)
        {
            // 核心：将当前对话行的keywords（需确保SceneDialogueLine有keywords字段）传给荧光笔
            List<string> currentKeywords = new List<string>();
            // 若你的SceneDialogueLine用的是keywordsCount，需手动补充关键词（示例：按数量生成，实际需从配置读）
            // 建议在SceneDialogueLine中添加List<string> keywords字段，直接配置关键词
            for (int i = 0; i < currentLine.keywordsCount; i++)
            {
                // 此处需替换为实际关键词（示例用"keyword1"等，实际应从对话行配置读）
                currentKeywords.Add($"keyword{i + 1}");
            }
            UI_HighLighter.Instance.ResetAllHighlighterState(currentKeywords); // 同步关键词
            UI_HighLighter.Instance.ToggleHighlighter(isHighlighterActive, currentLine.keywordsCount);
        }
        return isHighlighterActive;
    }
    private string GetCurrentActiveSceneGameObjectId()
    {
        GameObject[] sceneGameObjects = GameObject.FindGameObjectsWithTag(sceneGameObjectTag);
        foreach (var sceneObj in sceneGameObjects)
        {
            if (sceneObj.activeSelf)
            {
                Debug.Log($"识别到当前激活的GameObject场景：{sceneObj.name}");
                return sceneObj.name;
            }
        }

        Debug.LogWarning("未找到激活的GameObject场景，兜底使用全局默认");
        return "DefaultScene";
    }
    public void InitSceneDialogAfterJump(GameObject targetSceneObj)
    {
        if (targetSceneObj == null)
        {
            Debug.LogError("目标场景GameObject为空，无法初始化对话配置");
            return;
        }

        // 1. 跳转时隐藏对话框（保留原有需求）
        if (hideDialogOnSceneJump && UI_DialogPanel.Instance != null)
        {
            UI_DialogPanel.Instance.SetPanelActive(false);
            Debug.Log($"GameObject场景跳转：隐藏对话框，等待对话检查完成");
        }
        // 2. 更新当前场景ID，筛选对应对话组（核心修改）
        currentSceneGameObjectId = targetSceneObj.name;
        FilterDialogGroupsByCurrentScene();

        // 3. 延迟执行：检查历史对话，匹配新对话（保留原有逻辑）
        Invoke("CheckHistoryDialogAndShowNew", 0.1f);
    }
    private void CheckHistoryDialogAndShowNew()
    {
        string currentSceneKey = currentSceneGameObjectId;
        if (string.IsNullOrEmpty(currentSceneKey))
        {
            currentSceneKey = GetCurrentActiveSceneGameObjectId();
            Debug.Log($"自动识别当前场景：{currentSceneKey}");
        }

        // 初始化当前场景对话记录
        if (!sceneTriggeredDialogMap.ContainsKey(currentSceneKey))
        {
            sceneTriggeredDialogMap.Add(currentSceneKey, new List<string>());
            Debug.Log($"初始化场景[{currentSceneKey}]对话记录（首次进入）");
        }
        List<string> currentSceneTriggeredGroups = sceneTriggeredDialogMap[currentSceneKey];

        // 校验筛选后的当前场景对话组
        if (currentSceneDialogGroups.Count == 0)
        {
            Debug.LogError($"场景[{currentSceneKey}]无匹配对话组！检查Belong Scene Id");
            return;
        }

        bool foundNewGroup = false;
        // 重置当前组索引为0（确保从当前场景的第一个对话组开始筛选，而非全局第一个）
        currentGroupIndex = 0;
        for (int i = currentGroupIndex; i < currentSceneDialogGroups.Count; i++)
        {
            var group = currentSceneDialogGroups[i];
            bool preConditionMet = string.IsNullOrEmpty(group.preTriggerGroupId)
                                  || currentSceneTriggeredGroups.Contains(group.preTriggerGroupId);

            // 关键：只匹配「未触发过」的新对话组，避免重复匹配第一个组
            if (preConditionMet && !currentSceneTriggeredGroups.Contains(group.groupId))
            {
                // 1. 更新为新对话组的索引（核心：不再停留在0）
                currentGroupIndex = i;
                // 2. 重置对话行索引为0，从新组第一句开始显示（核心：避免重复最后一句）
                currentLineIndex = 0;
                // 3. 显示新组第一句，而非旧组最后一句
                ShowCurrentDialogLine();
                // 4. 激活frame面板
                if (UI_DialogPanel.Instance != null)
                {
                    UI_DialogPanel.Instance.SetPanelActive(true);
                }

                // 5. 归档新对话组，标记为已触发（避免重复匹配）
                currentSceneTriggeredGroups.Add(group.groupId);
                sceneTriggeredDialogMap[currentSceneKey] = currentSceneTriggeredGroups;
                if (!triggeredGroups.ContainsKey(group.groupId))
                {
                    triggeredGroups.Add(group.groupId, true);
                }

                foundNewGroup = true;
                Debug.Log($"成功切换到新对话组：[{group.groupId}]（场景：{currentSceneKey}，索引：{i}），从第一句开始显示");
                break;
            }
        }

        if (!foundNewGroup)
        {
            Debug.Log($"场景[{currentSceneKey}]无满足条件的新对话组（已触发所有组/前置条件未满足）");
        }
    }

    // 继续下一句
    public void OnNextDialog()
    {
        if (currentSceneDialogGroups.Count == 0)
        {
            Debug.LogError("当前场景无有效对话组，无法执行下一步");
            return;
        }

        var currentGroup = currentSceneDialogGroups[currentGroupIndex];
        if (currentGroup == null || currentGroup.lines == null || currentGroup.lines.Count == 0)
        {
            SwitchToNextValidGroup();
            return;
        }

        // ========== 核心修复：修正翻页条件 ==========
        // 原条件：currentLineIndex < currentGroup.lines.Count - 1（仅允许翻到倒数第二句）
        // 新条件：currentLineIndex < currentGroup.lines.Count（允许翻到所有句子，最后一句再触发组切换）
        if (currentLineIndex < currentGroup.lines.Count)
        {
            // 先显示当前行，再递增索引（避免索引越界）
            ShowCurrentDialogLine();
            currentLineIndex++;
            _highlighter?.ResetAllHighlighterState();
            Debug.Log($"翻页：当前组[{currentGroup.groupId}]，行索引从{currentLineIndex - 1}→{currentLineIndex}");
        }

        // 检查是否到达最后一句（索引等于行数-1时触发组切换）
        if (currentLineIndex >= currentGroup.lines.Count)
        {
            Debug.Log($"当前组[{currentGroup.groupId}]已到最后一句，触发组切换");
            string currentSceneKey = currentSceneGameObjectId;
            if (!sceneTriggeredDialogMap.ContainsKey(currentSceneKey))
            {
                sceneTriggeredDialogMap.Add(currentSceneKey, new List<string>());
            }
            List<string> currentSceneTriggeredGroups = sceneTriggeredDialogMap[currentSceneKey];

            // 标记当前组为已触发
            if (!currentSceneTriggeredGroups.Contains(currentGroup.groupId))
            {
                currentSceneTriggeredGroups.Add(currentGroup.groupId);
                sceneTriggeredDialogMap[currentSceneKey] = currentSceneTriggeredGroups;
                triggeredGroups.Add(currentGroup.groupId, true);
            }

            // 隐藏目标GameObject（原有逻辑）
            if (currentGroup.targetHideGameObject != null)
            {
                currentGroup.targetHideGameObject.SetActive(false);
            }

            // 执行GameObject跳转（若有），然后切换新组
            if (currentGroup.isJumpToGameObject && currentGroup.targetGameObject != null)
            {
                currentGroup.targetGameObject.SetActive(currentGroup.setActive);
                InitSceneDialogAfterJump(currentGroup.targetGameObject);
            }
            else
            {
                // 无跳转：直接切换到下一个新组
                SwitchToNextValidGroup();
            }
            return;
        }
    }
    // 切换到下一个符合条件的对话组
    private void SwitchToNextValidGroup()
    {
        if (currentSceneDialogGroups.Count == 0)
        {
            Debug.LogError("当前场景无有效对话组");
            return;
        }

        string currentSceneKey = currentSceneGameObjectId;
        List<string> currentSceneTriggeredGroups = sceneTriggeredDialogMap.ContainsKey(currentSceneKey)
            ? sceneTriggeredDialogMap[currentSceneKey]
            : new List<string>();

        bool foundNextGroup = false;
        // 从当前组索引+1开始查找（不回退到第一组）
        for (int i = currentGroupIndex + 1; i < currentSceneDialogGroups.Count; i++)
        {
            var group = currentSceneDialogGroups[i];
            bool preConditionMet = string.IsNullOrEmpty(group.preTriggerGroupId)
                                  || currentSceneTriggeredGroups.Contains(group.preTriggerGroupId);

            if (preConditionMet && !currentSceneTriggeredGroups.Contains(group.groupId))
            {
                currentGroupIndex = i;
                currentLineIndex = 0; // 重置行索引为新组第一句
                ShowCurrentDialogLine();
                UI_DialogPanel.Instance?.SetPanelActive(true);
                foundNextGroup = true;
                Debug.Log($"成功切换到下一组：[{group.groupId}]（场景：{currentSceneKey}）");
                break;
            }
        }

        if (!foundNextGroup)
        {
            Debug.Log("当前场景无更多有效对话组，隐藏面板");
            UI_DialogPanel.Instance?.SetPanelActive(false);
        }
    }
    // 切换荧光笔状态（供面板按钮调用）
    // 获取当前对话行（传给荧光笔系统）
    public SceneDialogConfig.SceneDialogueGroup GetCurrentDialogGroup()
    {
        // 匹配你的对话组索引逻辑（示例）
        if (sceneDialogMap.ContainsKey(currentSceneName)
        && currentGroupIndex >= 0
        && currentGroupIndex < sceneDialogMap[currentSceneName].Count)
        {
            return sceneDialogMap[currentSceneName][currentGroupIndex];
        }
        Debug.LogWarning("当前对话组不存在");
        return null;
    }
    public SceneDialogConfig.SceneDialogueLine GetCurrentDialogLine()
    {
        // 确保当前场景、对话组、对话行索引有效
        if (!sceneDialogMap.ContainsKey(currentSceneName)) return null;
        var currentGroup = sceneDialogMap[currentSceneName][currentGroupIndex];
        if (currentGroup == null || currentLineIndex >= currentGroup.lines.Count) return null;
        // 返回当前显示的对话行
        return currentGroup.lines[currentLineIndex];
    }
    public void SwitchToGroup(string targetGroupId)
    {
        // 遍历当前场景的对话组，找到目标 groupId 对应的组
        if (sceneDialogMap.ContainsKey(currentSceneName))
        {
            var sceneGroups = sceneDialogMap[currentSceneName];
            for (int i = 0; i < sceneGroups.Count; i++)
            {
                if (sceneGroups[i].groupId == targetGroupId)
                {
                    currentGroupIndex = i;
                    currentLineIndex = 0; // 重置为对话组第一行
                    ShowCurrentDialogLine(); // 显示新对话组的内容
                    return;
                }
            }
        }
        Debug.LogWarning($"未找到 ID 为{targetGroupId}的对话组");
    }
    // 荧光笔划中关键词：显示奖励（由 UI_Highlighter 调用）
    public void ShowReward(Sprite rewardSprite)
    {
        if (rewardSprite != null)
        {
            // 示例：显示奖励图片（需在面板中添加奖励显示 UI，或扩展 Panel 接口）
            UI_DialogReward.Instance.ShowReward(rewardSprite);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
    }
}