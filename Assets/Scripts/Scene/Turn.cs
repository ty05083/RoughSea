using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Turn : MonoBehaviour
{
    public TMPro.TextMeshProUGUI dialogText; // 公开字段
    public GameObject dialogPanel;
    public SceneDialogConfig sceneDialogConfig;
    public UI_jump targetScript;
    public GameObject targetSceneObj; // 目标GameObject场景
    [Header("透明点击跳转配置")]
    public Button transBtn_Scene1;    // 透明点击按钮1（如客厅）
    public Button transBtn_Scene2;    // 透明点击按钮2（如卧室）
    public Button transBtn_Restroom;  // 可选：厕所的透明点击按钮（替换原有UI_jump按钮）
    public GameObject scene1Obj;      // 场景1的根GameObject（对应transBtn_Scene1）
    public GameObject scene2Obj;      // 场景2的根GameObject（对应transBtn_Scene2）
    [Header("当前显示的场景对象")]
    public GameObject currentShowScene; // 记录当前激活的场景，用于跳转时隐藏
    // Start is called before the first frame update
    public void Init()
    {
        Hide();


    }
    void Start()
    {
        dialogPanel.SetActive(true);
        // 先校验targetScript和btn_restroom是否为空
        if (targetScript == null)
        {
            Debug.LogError("Turn脚本的targetScript未赋值！");
            return;
        }
        if (targetScript.btn_restroom == null)
        {
            Debug.LogError("UI_jump脚本的btn_restroom未赋值！");
            return;
        }

        Debug.Log("Turn脚本的Start方法执行，开始绑定按钮事件");
        targetScript.btn_restroom.onClick.AddListener(() =>
        {
            Debug.Log("restroom按钮点击事件触发！"); // 核心日志
            Hide();
            GameMgr.I.restroom.Show();
            if (currentShowScene != null) currentShowScene.SetActive(false);
        });
        BindTransparentButtonEvents();
    }
    private void BindTransparentButtonEvents()
    {
        // 透明按钮1 → 跳转场景1
        if (transBtn_Scene1 != null && scene1Obj != null)
        {
            transBtn_Scene1.onClick.AddListener(() =>
            {
                Debug.Log("透明点击区1触发，跳转场景1");
                JumpToTargetScene(scene1Obj);
            });
        }
        else
        {
            Debug.LogError("透明按钮1或场景1对象未赋值！");
        }

        // 透明按钮2 → 跳转场景2
        if (transBtn_Scene2 != null && scene2Obj != null)
        {
            transBtn_Scene2.onClick.AddListener(() =>
            {
                Debug.Log("透明点击区2触发，跳转场景2");
                JumpToTargetScene(scene2Obj);
            });
        }
        else
        {
            Debug.LogError("透明按钮2或场景2对象未赋值！");
        }

        // 透明按钮 → 跳转厕所（可选，替换原有UI_jump的厕所按钮）
        if (transBtn_Restroom != null)
        {
            transBtn_Restroom.onClick.AddListener(() =>
            {
                Debug.Log("厕所透明点击区触发");
                Hide();
                GameMgr.I.restroom.Show();
                if (currentShowScene != null) currentShowScene.SetActive(false);
            });
        }
    }
    private void JumpToTargetScene(GameObject targetObj)
    {
        // 1. 隐藏当前Turn面板（复用原有Hide方法）
        Hide();
        // 2. 隐藏当前显示的场景对象
        if (currentShowScene != null && currentShowScene != targetObj)
        {
            currentShowScene.SetActive(false);
        }
        // 3. 激活目标场景对象
        targetObj.SetActive(true);
        // 4. 更新当前显示的场景记录
        currentShowScene = targetObj;
        // 可选：隐藏对话面板（若需要）
        if (dialogPanel != null) dialogPanel.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Invoke("InitDialogAfterDelay", 0.1f);
        dialogPanel.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
