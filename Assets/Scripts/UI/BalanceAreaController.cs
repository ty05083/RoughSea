using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class BalanceAreaController : MonoBehaviour
{
    [Header("天平配置")]
    public int balanceNumber; // 天平区域代表的数字（如1、2、3）
    public RectTransform balanceAreaRect; // 天平区域的RectTransform
  

    void Awake()
    {
        // 自动获取RectTransform（若未手动赋值）
        if (balanceAreaRect == null)
        {
            balanceAreaRect = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// 比较奖励图片数字和天平数字
    /// </summary>
    /// <param name="rewardNum">奖励图片的数字</param>
    public void CompareNumber(int rewardNum)
    {
        // 核心比较逻辑
        if (rewardNum == balanceNumber)
        {
            Debug.Log($"比较结果：相等！图片数字{rewardNum} = 天平数字{balanceNumber}");
          
        }
        else
        {
            Debug.Log($"比较结果：不等！图片数字{rewardNum} ≠ 天平数字{balanceNumber}");
           
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
