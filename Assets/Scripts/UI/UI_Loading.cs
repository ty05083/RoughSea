using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_Loading : MonoBehaviour
{
    public Button btn_NewGame;
    public Button btn_Continue;
    public Button btn_Setting;
    public Button btn_Quit;
    public PanelShowHideController panelShowHideController;
    public void Init() 
    {
      
        btn_NewGame.onClick.AddListener(() => 
        {
            Hide();

            GameMgr.I.isNewGameTriggered = true;
            if (UI_DialogText.Instance != null)
            {
                UI_DialogText.Instance.TriggerNewGameDialog();
            }
            GameMgr.I.deck.Show();
            GameMgr.I.InitNewGame();
           
        });
        
        btn_Quit.onClick.AddListener(() =>
        {
            Application.Quit(); // ÍË³öÓÎÏ·
        });

    }
    public void Show()
    {  
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);


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
