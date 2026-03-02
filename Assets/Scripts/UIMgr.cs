using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMgr : MonoBehaviour
{
    public static UIMgr I;

    public UI_Loading ui_Loading;
    public UI_DialogPanel uI_DialogPanel;
    public UI_DialogText uI_DialogText;
    public PanelShowHideController ui_relate;


    void Awake()
    {
        // 单例+跨场景持久化
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject); // 场景切换时不销毁UIMgr物体
        ui_Loading.Init();
       
   
       
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
