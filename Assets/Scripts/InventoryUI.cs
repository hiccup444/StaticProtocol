using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 0f, 0.8f); // Yellow highlight
    [SerializeField] private Color unselectedColor = new Color(0.3f, 0.3f, 0.3f, 0.6f); // Gray
    [SerializeField] private Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.4f); // Dark gray
    
    [Header("UI Settings")]
    [SerializeField] private float slotSpacing = 15f;
    [SerializeField] private float slotSize = 100f;
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float outlineWidth = 3f;
    
    [Header("Debug - Slot Contents")]
    [SerializeField] private string slot1 = "Empty";
    [SerializeField] private string slot2 = "Empty";
    [SerializeField] private string slot3 = "Empty";
    [SerializeField] private string slot4 = "Empty";
    
    // UI Components
    private InventorySlotUI[] slots;
    private InventorySystem inventorySystem;
    
    // Public properties for InventorySlotUI access
    public Color SelectedColor => selectedColor;
    public Color UnselectedColor => unselectedColor;
    public Color EmptySlotColor => emptySlotColor;
    public float SelectedScale => selectedScale;
    public float NormalScale => normalScale;
    public InventorySystem InventorySystem => inventorySystem;
    
    private void Awake()
    {
        // Find inventory system
        inventorySystem = FindObjectOfType<InventorySystem>();
        if (inventorySystem == null)
        {
            Debug.LogError("InventoryUI: No InventorySystem found in scene!");
            return;
        }
        
        // Find or create canvas
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
            }
        }
        
        // Subscribe to inventory events
        inventorySystem.OnSlotChanged.AddListener(UpdateSelectedSlot);
        inventorySystem.OnItemAdded.AddListener(UpdateSlot);
        inventorySystem.OnItemRemoved.AddListener(UpdateSlot);
        
        // Create hotbar UI
        CreateHotbarUI();
    }
    
    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("InventoryCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // Ensure it's on top
        
        // Add CanvasScaler for UI scaling
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster for UI interaction
        canvasObj.AddComponent<GraphicRaycaster>();
    }
    
    private void Start()
    {
        // Initialize UI with current inventory state
        RefreshAllSlots();
    }
    
    private void CreateHotbarUI()
    {
        if (canvas == null)
        {
            Debug.LogError("InventoryUI: Canvas not found!");
            return;
        }
        
        // Create hotbar parent
        GameObject hotbarParent = new GameObject("Hotbar");
        hotbarParent.transform.SetParent(canvas.transform, false);
        
        // Setup hotbar parent rect
        RectTransform hotbarRect = hotbarParent.AddComponent<RectTransform>();
        hotbarRect.anchorMin = new Vector2(0.5f, 0f);
        hotbarRect.anchorMax = new Vector2(0.5f, 0f);
        hotbarRect.pivot = new Vector2(0.5f, 0f);
        hotbarRect.anchoredPosition = new Vector2(0, 20); // 20 pixels from bottom
        hotbarRect.sizeDelta = new Vector2(inventorySystem.InventorySize * (slotSize + slotSpacing) - slotSpacing, slotSize + 20);
        
        // Create slots
        slots = new InventorySlotUI[inventorySystem.InventorySize];
        
        for (int i = 0; i < inventorySystem.InventorySize; i++)
        {
            GameObject slotObj = CreateSlotUI(i, hotbarParent.transform);
            slots[i] = slotObj.GetComponent<InventorySlotUI>();
        }
    }
    
    private GameObject CreateSlotUI(int slotIndex, Transform parent)
    {
        // Create slot background
        GameObject slotObj = new GameObject($"Slot_{slotIndex}");
        slotObj.transform.SetParent(parent, false);
        
        // Add RectTransform
        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(slotSize, slotSize);
        slotRect.anchoredPosition = new Vector2(slotIndex * (slotSize + slotSpacing) - (inventorySystem.InventorySize - 1) * (slotSize + slotSpacing) * 0.5f, 0);
        
        // Add Image component for background
        Image backgroundImage = slotObj.AddComponent<Image>();
        backgroundImage.color = emptySlotColor;
        
        // Add Button component for clicking
        Button button = slotObj.AddComponent<Button>();
        button.targetGraphic = backgroundImage;
        
        // Create item icon child
        GameObject itemIconObj = new GameObject("ItemIcon");
        itemIconObj.transform.SetParent(slotObj.transform, false);
        
        RectTransform iconRect = itemIconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(5, 5);
        iconRect.offsetMax = new Vector2(-5, -5);
        
        Image itemImage = itemIconObj.AddComponent<Image>();
        itemImage.color = new Color(1, 1, 1, 0); // Transparent initially
        
        // Create count text child
        GameObject countTextObj = new GameObject("CountText");
        countTextObj.transform.SetParent(slotObj.transform, false);
        
        RectTransform countRect = countTextObj.AddComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0.7f, 0);
        countRect.anchorMax = new Vector2(1, 0.3f);
        countRect.offsetMin = Vector2.zero;
        countRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI countText = countTextObj.AddComponent<TextMeshProUGUI>();
        countText.text = "";
        countText.fontSize = 14;
        countText.color = Color.white;
        countText.alignment = TextAlignmentOptions.BottomRight;
        countText.fontStyle = FontStyles.Bold;
        
        // Create selected indicator container
        GameObject selectedIndicatorObj = new GameObject("SelectedIndicator");
        selectedIndicatorObj.transform.SetParent(slotObj.transform, false);
        selectedIndicatorObj.SetActive(false);
        
        // Create outline using 4 border images
        CreateOutlineBorders(selectedIndicatorObj);
        
        // Add InventorySlotUI component
        InventorySlotUI slotUI = slotObj.AddComponent<InventorySlotUI>();
        slotUI.Initialize(slotIndex, this, backgroundImage, itemImage, countText, selectedIndicatorObj);
        
        // Setup button click
        button.onClick.AddListener(() => OnSlotClicked(slotIndex));
        
        return slotObj;
    }
    
    private void CreateOutlineBorders(GameObject parent)
    {
        // Create 4 border images for outline effect
        Color outlineColor = new Color(selectedColor.r, selectedColor.g, selectedColor.b, 1f);
        
        // Top border - positioned above the slot
        CreateBorderImage(parent, "TopBorder", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 
            new Vector2(-39, 39), new Vector2(39, 42));
        
        // Bottom border - positioned below the slot
        CreateBorderImage(parent, "BottomBorder", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 
            new Vector2(-39, -42), new Vector2(39, -39));
        
        // Left border - positioned to the left of the slot
        CreateBorderImage(parent, "LeftBorder", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 
            new Vector2(-42, -39), new Vector2(-39, 39));
        
        // Right border - positioned to the right of the slot
        CreateBorderImage(parent, "RightBorder", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 
            new Vector2(39, -39), new Vector2(42, 39));
    }
    
    private void CreateBorderImage(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject borderObj = new GameObject(name);
        borderObj.transform.SetParent(parent.transform, false);
        
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = anchorMin;
        borderRect.anchorMax = anchorMax;
        borderRect.offsetMin = offsetMin;
        borderRect.offsetMax = offsetMax;
        
        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = new Color(selectedColor.r, selectedColor.g, selectedColor.b, 1f);
    }
    
    private void UpdateSelectedSlot(int selectedSlot)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].SetSelected(i == selectedSlot);
            }
        }
    }
    
    private void UpdateSlot(int slotIndex, Item item)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length && slots[slotIndex] != null)
        {
            slots[slotIndex].UpdateSlot(item, inventorySystem.GetItemCount(slotIndex));
            UpdateDebugSlot(slotIndex, item);
        }
    }
    
    private void UpdateDebugSlot(int slotIndex, Item item)
    {
        string itemName = item.IsEmpty() ? "Empty" : $"{item.itemName} (x{inventorySystem.GetItemCount(slotIndex)})";
        
        switch (slotIndex)
        {
            case 0:
                slot1 = itemName;
                break;
            case 1:
                slot2 = itemName;
                break;
            case 2:
                slot3 = itemName;
                break;
            case 3:
                slot4 = itemName;
                break;
        }
    }
    
    private void RefreshAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                Item item = inventorySystem.GetItem(i);
                int count = inventorySystem.GetItemCount(i);
                slots[i].UpdateSlot(item, count);
                slots[i].SetSelected(i == inventorySystem.SelectedSlot);
                UpdateDebugSlot(i, item);
            }
        }
    }
    
    public void OnSlotClicked(int slotIndex)
    {
        inventorySystem.SelectSlot(slotIndex);
    }
    
    public void OnSlotRightClicked(int slotIndex)
    {
        // Use item on right click
        inventorySystem.UseItem(slotIndex);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inventorySystem != null)
        {
            inventorySystem.OnSlotChanged.RemoveListener(UpdateSelectedSlot);
            inventorySystem.OnItemAdded.RemoveListener(UpdateSlot);
            inventorySystem.OnItemRemoved.RemoveListener(UpdateSlot);
        }
    }
}

[System.Serializable]
public class InventorySlotUI : MonoBehaviour
{
    private Image backgroundImage;
    private Image itemImage;
    private TextMeshProUGUI countText;
    private GameObject selectedIndicator;
    
    private int slotIndex;
    private InventoryUI inventoryUI;
    private bool isSelected = false;
    
    public void Initialize(int index, InventoryUI ui, Image bgImage, Image img, TextMeshProUGUI text, GameObject indicator)
    {
        slotIndex = index;
        inventoryUI = ui;
        backgroundImage = bgImage;
        itemImage = img;
        countText = text;
        selectedIndicator = indicator;
    }
    
    public void UpdateSlot(Item item, int count)
    {
        if (item.IsEmpty())
        {
            itemImage.sprite = null;
            itemImage.color = new Color(1, 1, 1, 0);
            countText.text = "";
            backgroundImage.color = inventoryUI.EmptySlotColor;
        }
        else
        {
            itemImage.sprite = item.icon;
            itemImage.color = Color.white;
            countText.text = count > 1 ? count.ToString() : "";
            backgroundImage.color = isSelected ? inventoryUI.SelectedColor : inventoryUI.UnselectedColor;
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        selectedIndicator.SetActive(selected);
        
        // Scale effect
        float scale = selected ? inventoryUI.SelectedScale : inventoryUI.NormalScale;
        transform.localScale = Vector3.one * scale;
        
        // Update background color
        if (inventoryUI != null && !inventoryUI.InventorySystem.GetItem(slotIndex).IsEmpty())
        {
            backgroundImage.color = selected ? inventoryUI.SelectedColor : inventoryUI.UnselectedColor;
        }
    }
    
    private void OnSlotClicked()
    {
        if (inventoryUI != null)
        {
            inventoryUI.OnSlotClicked(slotIndex);
        }
    }
    
    private InventorySystem inventorySystem
    {
        get
        {
            if (inventoryUI != null)
                return inventoryUI.InventorySystem;
            return null;
        }
    }
}
