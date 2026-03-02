using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    public static GameMgr I;
    public Deck deck;
    public Turn turn;
    public Restroom restroom;
    public bool isNewGameTriggered = false; // NewGame是否触发
    void Awake()
    {
        I = this;
        deck.Init();
        turn.Init();
        restroom.Init();
    }

    public void InitNewGame()
    {
        // 新游戏初始化逻辑（如加载初始场景、重置数据）
    }



    void Start()
    {
       
    }
// Update is called once per frame
void Update()
    {
        
    }
}
