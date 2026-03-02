using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
public class DraggableCharacterItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("组件内部UI")]
    public Image characterImage;       // 角色图片（固定显示一张）
    public Button leftBtn;             // 左切换按钮
    public Button rightBtn;            // 右切换按钮
    public TextMeshProUGUI nameText;   // 角色名称文本

    [Header("角色数据（由外部赋值）")]
    public string[] characterNames;    // 只需要名称数组
    private int currentIndex = 0;      // 当前显示的名称索引
  
    [Header("连接线设置")]
    public GameObject linePrefab;       // 连接线预制体（LinePrefab）
    public LayerMask characterLayer;       // 目标面板的层级（需给目标面板设置该层级）
    private LineRenderer currentLine;    // 当前绘制的线条
    private DraggableCharacterItem connectedItem; // 已连接的另一个角色组件
    private RectTransform rectTransform;
    private Vector2 dragOffset;
    private bool isDrawingLine = false;         // 是否正在绘制线条
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) rectTransform = gameObject.AddComponent<RectTransform>();

        // 绑定事件时添加日志，验证按钮是否为空
        if (leftBtn == null) Debug.LogError($"【{gameObject.name}】左按钮未赋值！");
        if (rightBtn == null) Debug.LogError($"【{gameObject.name}】右按钮未赋值！");

        leftBtn.onClick.AddListener(OnLeftBtnClick);
        rightBtn.onClick.AddListener(OnRightBtnClick);
        // 校验连接线预制体
        if (linePrefab == null) Debug.LogError("连接线预制体未赋值！");
        Debug.Log($"【{gameObject.name}】按钮事件绑定完成");
        // 确保Image可接收点击事件（面板中心点击的核心）
        if (characterImage != null)
        {
            characterImage.raycastTarget = true;
        }
        // 屏蔽按钮的射线检测优先级，避免点击按钮触发拉线条
        if (leftBtn != null) leftBtn.GetComponent<Image>().raycastTarget = true;
        if (rightBtn != null) rightBtn.GetComponent<Image>().raycastTarget = true;
    }

    /// <summary>
    /// 外部初始化：设置固定图片和名称列表
    /// </summary>
    /// <param name="fixedSprite">固定显示的角色图片</param>
    /// <param name="names">可切换的名称数组</param>
    public void Init(Sprite fixedSprite, string[] names)
    {
        if (fixedSprite != null && characterImage != null)
        {
            characterImage.sprite = fixedSprite;
            characterImage.SetNativeSize();
        }


        // 接收并赋值数组
        this.characterNames = names;
        Debug.Log($"Item 接收的名称数组长度: {this.characterNames.Length}");

        UpdateDisplay();
    }

    private void OnLeftBtnClick()
    {
        Debug.Log($"【{gameObject.name}】左按钮点击，当前索引：{currentIndex}");
        if (characterNames == null || characterNames.Length == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = characterNames.Length - 1;
        UpdateDisplay();
    }

    private void OnRightBtnClick()
    {
        Debug.Log($"【{gameObject.name}】右按钮点击，当前索引：{currentIndex}");
        if (characterNames == null || characterNames.Length == 0) return;
        currentIndex++;
        if (currentIndex >= characterNames.Length) currentIndex = 0;
        UpdateDisplay();

    }

    /// <summary>
    /// 只更新名称显示，不再切换图片
    /// </summary>
    private void UpdateDisplay()
    {
        Debug.Log($"【{gameObject.name}】UpdateDisplay 被调用，当前索引: {currentIndex}");

        if (characterNames == null || characterNames.Length == 0)
        {
            nameText.text = "默认名称";
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, characterNames.Length - 1);
        nameText.text = characterNames[currentIndex];
        Debug.Log($"【{gameObject.name}】更新名称为: {characterNames[currentIndex]}");
        nameText.ForceMeshUpdate();
    }

    #region 整体拖拽逻辑
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDrawingLine) return;

        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out mousePos);
        dragOffset = rectTransform.anchoredPosition - mousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDrawingLine) return;
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out mousePos);
        rectTransform.anchoredPosition = mousePos + dragOffset;
        if (currentLine != null && connectedItem != null)
        {
            UpdateLinePosition();
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        // 左键点击：触发拉线条（排除点击按钮、正在绘制、已有连接的情况）
        if (eventData.button == PointerEventData.InputButton.Left
            && !isDrawingLine
            && connectedItem == null
            && !IsClickOnButton(eventData)) // 排除点击左右切换按钮
        {
            StartDrawingLine();
        }
        // 右键点击：取消当前线条/连接
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            CancelLine();
            // 同时取消连接对象的关联
            if (connectedItem != null)
            {
                connectedItem.connectedItem = null;
                connectedItem.currentLine = null;
                connectedItem = null;
            }
        }
    }

    void Update()
    {
        // 绘制中：实时更新线条终点（跟随鼠标），并检测是否碰到其他面板
        if (isDrawingLine && currentLine != null)
        {
            // 鼠标屏幕坐标转UI世界坐标（严格适配UGUI，z轴置0）
            Vector3 mouseWorldPos = RectTransformUtility.WorldToScreenPoint(Camera.main, rectTransform.position);
            mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            // 实时更新线条终点
            currentLine.SetPosition(1, mouseWorldPos);
            // 检测鼠标是否碰到其他CharacterItem面板
            CheckOtherCharacterItem(mouseWorldPos);
        }
    }

    // 开始绘制线条
    private void StartDrawingLine()
    {
        // 先取消原有无效线条
        CancelLine();
        if (linePrefab == null) return;

        // 实例化线条，父物体设为当前面板的父物体（避免层级混乱）
        GameObject lineObj = Instantiate(linePrefab, transform.parent);
        currentLine = lineObj.GetComponent<LineRenderer>();
        if (currentLine == null)
        {
            Debug.LogError("线条预制体必须挂载LineRenderer组件！");
            Destroy(lineObj);
            return;
        }

        // 线条基础设置（2个点、宽度、关闭光照）
        currentLine.positionCount = 2;
        currentLine.startWidth = 0.05f;
        currentLine.endWidth = 0.05f;
        currentLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        currentLine.receiveShadows = false;

        // 线条起点：严格对齐**当前面板中心**的世界坐标
        Vector3 startPos = GetComponentCenterWorldPos();
        currentLine.SetPosition(0, startPos);
        currentLine.SetPosition(1, startPos); // 初始终点与起点重合

        isDrawingLine = true;
        connectedItem = null;
        Debug.Log($"【{gameObject.name}】开始绘制线条，起点：{startPos}");
    }
    // 检测鼠标位置是否碰到其他角色组件
    private void CheckOtherCharacterItem(Vector3 mousePos)
    {
        // 构建UI射线检测数据
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Camera.main.WorldToScreenPoint(mousePos);
        // 存储检测结果
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 遍历检测结果，找到目标面板
        foreach (var result in results)
        {
            if (result.gameObject == null) continue;
            // 只检测指定层级的面板
            if (((1 << result.gameObject.layer) & characterLayer) == 0) continue;
            // 获取目标面板的脚本
            DraggableCharacterItem targetItem = result.gameObject.GetComponent<DraggableCharacterItem>();
            // 排除自身、已有连接的面板
            if (targetItem != null && targetItem != this && targetItem.connectedItem == null)
            {
                connectedItem = targetItem;
                // 线条终点：严格对齐**目标面板中心**的世界坐标
                currentLine.SetPosition(1, connectedItem.GetComponentCenterWorldPos());
                isDrawingLine = false;
                // 双向绑定连接关系（目标面板也记录当前面板）
                targetItem.connectedItem = this;
                targetItem.currentLine = currentLine;
                Debug.Log($"【{gameObject.name}】已连接到【{connectedItem.name}】，终点：{connectedItem.GetComponentCenterWorldPos()}");
                break;
            }
        }
    }

    // 更新线条位置（拖拽组件时同步）
    private void UpdateLinePosition()
    {
        if (currentLine == null || connectedItem == null) return;

        // 起点：当前面板中心
        currentLine.SetPosition(0, GetComponentCenterWorldPos());
        // 终点：连接面板中心
        currentLine.SetPosition(1, connectedItem.GetComponentCenterWorldPos());
    }

    // 取消线条/连接
    private void CancelLine()
    {
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }
        isDrawingLine = false;
    }

    // 辅助方法：获取组件中心的世界坐标（适配UI）
    private bool IsClickOnButton(PointerEventData eventData)
    {
        if (leftBtn != null && RectTransformUtility.RectangleContainsScreenPoint(leftBtn.GetComponent<RectTransform>(), eventData.position))
        {
            return true;
        }
        if (rightBtn != null && RectTransformUtility.RectangleContainsScreenPoint(rightBtn.GetComponent<RectTransform>(), eventData.position))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 核心辅助方法：获取面板**中心**的世界坐标（适配UGUI任意锚点）
    /// </summary>
    public Vector3 GetComponentCenterWorldPos()
    {
        Vector3 centerLocalPos = new Vector3(rectTransform.rect.center.x, rectTransform.rect.center.y, 0);
        Vector3 centerWorldPos = rectTransform.TransformPoint(centerLocalPos);
        centerWorldPos.z = 0; // 2D UI，z轴强制置0，避免线条偏移
        return centerWorldPos;
    }
    #endregion
    public void OnEndDrag(PointerEventData eventData)
    {
        // 这里可以留空，或者添加你需要的逻辑
    }
}