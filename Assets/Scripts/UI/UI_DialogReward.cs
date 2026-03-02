using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UI_DialogReward : MonoBehaviour
{
    public static UI_DialogReward Instance;
    public GameObject rewardPanel;
    public Image img_Reward;
    public GameObject rewardItemPrefab; // 新增：奖励物品预设引用
 
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("UI_DialogReward单例初始化成功");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("存在多个UI_DialogReward实例，已销毁重复对象");
        }
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
            Debug.Log("奖励面板初始隐藏成功");
        }
        else
        {
            Debug.LogError("UI_DialogReward的rewardPanel未绑定！");
        }
    }
    // 显示奖励图片
  
    public void ShowReward(Sprite rewardSprite)
    {
        img_Reward.sprite = rewardSprite;
        rewardPanel.SetActive(true);
      
    }
    // 隐藏奖励面板
    public void HideReward()
    {
        rewardPanel.SetActive(false);
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
