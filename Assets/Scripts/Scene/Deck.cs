using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public TMPro.TextMeshProUGUI dialogText;
    public GameObject dialogPanel;
    public GameObject targetSceneObj; // ƒø±ÍGameObject≥°æ∞

    public void Init ()
    {
        Hide();


    }

    public void Show() 
    {
        gameObject.SetActive(true);
        Invoke("InitDialogAfterDelay", 0.1f);
    }
    private void InitDialogAfterDelay()
    {
        if (UI_DialogText.Instance == null)
        {
            UI_DialogText.Instance = FindObjectOfType<UI_DialogText>();
            if (UI_DialogText.Instance == null)
            {
                Debug.LogError("Œ¥’“µΩUI_DialogText µ¿˝£¨«ÎºÏ≤ÈΩ≈±æπ“‘ÿ");
                return;
            }
        }
        if (targetSceneObj == null)
        {
            Debug.LogError("Target Scene ObjŒ¥∏≥÷µ£¨«ÎºÏ≤ÈInspector≈‰÷√");
            return;
        }
        UI_DialogText.Instance.InitSceneDialogAfterJump(targetSceneObj);
        dialogPanel.SetActive(false);
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
        dialogPanel.SetActive(false);
    }


    // Start is called before the first frame update
    void Start()
    {
        dialogPanel.SetActive(true);

    }
  

    // Update is called once per frame
    void Update()
    {
        
    }
}
