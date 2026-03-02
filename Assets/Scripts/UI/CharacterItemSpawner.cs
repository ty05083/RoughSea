using UnityEngine;
using UnityEngine.UI;

public class CharacterItemSpawner : MonoBehaviour
{
    [Header("生成设置")]
    public Button spawnButton;               // 点击生成的按钮
    public GameObject characterItemPrefab;   // 角色组件预制体
    public RectTransform spawnArea;          // 拖拽你的生成区域面板（SpawnArea）到这里

    [Header("连接线预制体")]
    public GameObject linePrefab;            // 连接线预制体

    [Header("角色数据源")]
    public Sprite fixedCharacterSprite;      // 固定显示的角色图片
    public string[] characterNames = new string[] { "mate", "boss", "bro" };

    // 生成区域的实际边界（由代码自动计算）
    private float areaLeft;
    private float areaRight;
    private float areaBottom;
    private float areaTop;

    void Start()
    {
        spawnButton.onClick.AddListener(SpawnCharacterItem);

        // 初始化时计算生成区域的边界
        CalculateSpawnAreaBounds();
    }

    /// <summary>
    /// 核心方法：计算SpawnArea的可生成范围（适配UI锚点）
    /// </summary>
    private void CalculateSpawnAreaBounds()
    {
        if (spawnArea == null)
        {
            Debug.LogError("请指定生成区域（SpawnArea）！");
            return;
        }

        // 获取生成区域的矩形信息
        Rect rect = spawnArea.rect;

        // 计算生成区域的四个边界（世界坐标/锚点坐标）
        // 左边界 = 区域中心X - 区域宽度的一半
        areaLeft = spawnArea.anchoredPosition.x - rect.width / 2;
        // 右边界 = 区域中心X + 区域宽度的一半
        areaRight = spawnArea.anchoredPosition.x + rect.width / 2;
        // 下边界 = 区域中心Y - 区域高度的一半
        areaBottom = spawnArea.anchoredPosition.y - rect.height / 2;
        // 上边界 = 区域中心Y + 区域高度的一半
        areaTop = spawnArea.anchoredPosition.y + rect.height / 2;

        Debug.Log($"生成范围计算完成：X({areaLeft}~{areaRight}), Y({areaBottom}~{areaTop})");
    }

    private void SpawnCharacterItem()
    {
        if (characterItemPrefab == null || spawnArea == null) return;

        // 1. 实例化新面板
        GameObject newItem = Instantiate(characterItemPrefab, spawnArea.parent);
        RectTransform itemRect = newItem.GetComponent<RectTransform>();
        if (itemRect == null) return;

        // 2. 获取生成的面板自身尺寸（防止生成在边缘被裁切）
        float itemHalfWidth = itemRect.rect.width / 2;
        float itemHalfHeight = itemRect.rect.height / 2;

        // 3. 在范围内随机生成位置（扣除面板自身尺寸，确保完全在区域内）
        float randomX = Random.Range(areaLeft + itemHalfWidth, areaRight - itemHalfWidth);
        float randomY = Random.Range(areaBottom + itemHalfHeight, areaTop - itemHalfHeight);

        // 4. 设置面板位置
        itemRect.anchoredPosition = new Vector2(randomX, randomY);
        itemRect.anchorMin = spawnArea.anchorMin; // 保持和区域相同的锚点
        itemRect.anchorMax = spawnArea.anchorMax;
        itemRect.sizeDelta = characterItemPrefab.GetComponent<RectTransform>().sizeDelta;

        // 5. 初始化数据和层级
        DraggableCharacterItem itemScript = newItem.GetComponent<DraggableCharacterItem>();
        if (itemScript != null)
        {
            itemScript.Init(fixedCharacterSprite, this.characterNames);
            itemScript.linePrefab = this.linePrefab;
            newItem.layer = LayerMask.NameToLayer("CharacterItem");
        }
    }
}