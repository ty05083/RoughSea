using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable] // 序列化后可在Inspector面板编辑
public class RewardData : MonoBehaviour
{
   
   
    
        public string keyWord; // 触发关键词
        public Sprite rewardSprite; // 奖励图片
        public int rewardNumber; // 奖励数值（供天平比较）
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
