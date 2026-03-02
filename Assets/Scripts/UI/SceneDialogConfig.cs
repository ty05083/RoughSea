using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDialogConfig : MonoBehaviour
{
    public List<SceneDialogueGroup> allSceneDialogGroups = new List<SceneDialogueGroup>();
    [System.Serializable]
    public class SceneDialogueLine
    {
        public string content;
        public string lineId;
        public Sprite characterSprite;
        public int keywordsCount;
        public UI_RewardDragHandler rewardDragHandler;
        public int rewardValue; // 奖励图片对应的数值（用于天平比对）
       
    }

    [System.Serializable]
    public class SceneDialogueGroup
    {
        public List<SceneDialogueLine> lines;
        public string groupId; // 对话组唯一ID
        public string preTriggerGroupId; // 前置对话组ID
        public bool isJumpToGameObject; // 是否跳转GameObject
        public GameObject targetGameObject; // 目标GameObject
        public GameObject targetHideGameObject; // 要隐藏的GameObject
        public bool setActive; // 激活/隐藏状态
        public bool hideDialogPanelOnFinish; // 结束后是否隐藏对话框

        // ============== 新增：标记该对话组所属的GameObject场景ID（与GameObject名称一致） ==============
        [Header("场景归属配置")]
        public string belongSceneId; // 所属GameObject场景名称（如：Deck、Turn、Restroom）
    }

    public List<SceneDialogueGroup> sceneDialogGroups; // 仅存储多文本对话组数据
    public string GetDialogContent(int groupIndex, int lineIndex)
    {
        // 校验对话组是否存在
        if (sceneDialogGroups == null || groupIndex < 0 || groupIndex >= sceneDialogGroups.Count)
        {
            Debug.LogError($"对话组索引[{groupIndex}]无效");
            return "";
        }
        SceneDialogueGroup targetGroup = sceneDialogGroups[groupIndex];

        // 校验对话行是否存在
        if (targetGroup.lines == null || lineIndex < 0 || lineIndex >= targetGroup.lines.Count)
        {
            Debug.LogError($"对话行索引[{lineIndex}]无效");
            return "";
        }
        return targetGroup.lines[lineIndex].content;
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
