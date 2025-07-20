using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MinionMovementManager : MonoBehaviour
{
    [Header("References")]
    public CardSlotManager cardSlotManager;
    public Camera mainCamera;
    
    [Header("Movement Settings")]
    public float dragSensitivity = 1.0f;
    public bool enableDragAndDrop = true;
    public bool enableClickToMove = true;
    
    [Header("Visual Feedback")]
    public Color highlightColor = new Color(1f, 1f, 0f, 0.3f);
    public Color validDropColor = new Color(0f, 1f, 0f, 0.3f);
    public Color invalidDropColor = new Color(1f, 0f, 0f, 0.3f);
    public Color selectionColor = new Color(1f, 1f, 0f, 0.8f);  
    public GameObject slotIndicatorPrefab;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    private MinionController selectedMinion;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private CardSlotManager.CardSlot hoveredSlot;
    private List<MinionController> playerMinions = new List<MinionController>();
    private List<GameObject> slotIndicators = new List<GameObject>();
    
    private Dictionary<MinionController, Color> originalMinionColors = new Dictionary<MinionController, Color>();
    private GameObject selectionIndicator;
    
    private PlayerInput playerInput;
    private InputAction mouseClickAction;
    private InputAction mousePositionAction;
    
    void Start()
    {
        InitializeMovementSystem();
        SetupInputSystem();
        
        StartCoroutine(CreateIndicatorsAfterSetup());
    }
    
    System.Collections.IEnumerator CreateIndicatorsAfterSetup()
    {
        yield return null;
        
        CreateSlotIndicators();
    }
    
    void InitializeMovementSystem()
    {
        if (cardSlotManager == null)
            cardSlotManager = FindFirstObjectByType<CardSlotManager>();
            
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        FindPlayerMinions();
        
        Debug.Log($"[MinionMovementManager] Initialized with {playerMinions.Count} minions");
    }
    
    void SetupInputSystem()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }
        
        mouseClickAction = new InputAction("MouseClick", InputActionType.Button, "<Mouse>/leftButton");
        mousePositionAction = new InputAction("MousePosition", InputActionType.Value, "<Mouse>/position");
        
        mouseClickAction.Enable();
        mousePositionAction.Enable();
        
        Debug.Log("[MinionMovementManager] Input System setup complete");
    }
    
    void CreateSlotIndicators()
    {
        foreach (var indicator in slotIndicators)
        {
            if (indicator != null)
                DestroyImmediate(indicator);
        }
        slotIndicators.Clear();
        
        CardSlotManager.CardSlot[] allPlayerSlots = cardSlotManager.AllPlayerSlots;
        
        Debug.Log($"[MinionMovementManager] Creating {allPlayerSlots.Length} slot indicators");
        
        foreach (var slot in allPlayerSlots)
        {
            GameObject indicator = CreateSlotIndicator(slot);
            slotIndicators.Add(indicator);
            
            SpriteRenderer sr = indicator.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Debug.Log($"[MinionMovementManager] Created slot indicator at {slot.worldPosition} with sprite: {sr.sprite.name}");
            }
            else
            {
                Debug.LogError($"[MinionMovementManager] Failed to create sprite for slot indicator at {slot.worldPosition}");
            }
        }
    }
    
    GameObject CreateSlotIndicator(CardSlotManager.CardSlot slot)
    {
        GameObject indicator;
        
        if (slotIndicatorPrefab != null)
        {
            indicator = Instantiate(slotIndicatorPrefab, slot.worldPosition, Quaternion.identity);
        }
        else
        {
            indicator = new GameObject($"SlotIndicator_{slot.type}_{slot.row}_{slot.slotIndex}");
            
            SpriteRenderer spriteRenderer = indicator.AddComponent<SpriteRenderer>();
            
            Sprite sprite = CreateSimpleWhiteSquareSprite();
            spriteRenderer.sprite = sprite;
            
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
            
            spriteRenderer.sortingOrder = 0;
            
            indicator.transform.position = slot.worldPosition;
            indicator.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            
            Debug.Log($"[MinionMovementManager] Created slot indicator at position {slot.worldPosition} for {slot.type} {slot.row} slot {slot.slotIndex}");
        }
        
        return indicator;
    }
    
    Sprite CreateSimpleWhiteSquareSprite()
    {
        int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color[] pixels = new Color[size * size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isEdge = x == 0 || x == size - 1 || y == 0 || y == size - 1;
                bool isInnerEdge = x == 1 || x == size - 2 || y == 1 || y == size - 2;
                
                if (isEdge)
                {
                    pixels[y * size + x] = Color.white;
                }
                else if (isInnerEdge)
                {
                    pixels[y * size + x] = new Color(1f, 1f, 1f, 0.3f);
                }
                else
                {
                    pixels[y * size + x] = new Color(1f, 1f, 1f, 0.1f);
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        sprite.name = "SlotIndicatorSprite";
        
        return sprite;
    }
    
    void FindPlayerMinions()
    {
        playerMinions.Clear();
        MinionController[] allMinions = FindObjectsByType<MinionController>(FindObjectsSortMode.None);
        
        foreach (MinionController minion in allMinions)
        {
            if (minion != null && IsPlayerMinion(minion))
            {
                playerMinions.Add(minion);
                SetupMinionForMovement(minion);
            }
        }
    }
    
    bool IsPlayerMinion(MinionController minion)
    {
        CardSlotManager.CardSlot[] playerSlots = cardSlotManager.AllPlayerSlots;    
        foreach (var slot in playerSlots)
        {
            if (slot.occupant == minion.gameObject)
                return true;
        }
        return false;
    }
    
    void SetupMinionForMovement(MinionController minion)
    {
        Collider2D collider = minion.GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D boxCollider = minion.gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(1f, 1f);
            boxCollider.isTrigger = true;
        }
        else if (collider is CircleCollider2D)
        {
            DestroyImmediate(collider);
            BoxCollider2D boxCollider = minion.gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(1f, 1f);
            boxCollider.isTrigger = true;
        }
        
        EventTrigger eventTrigger = minion.GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = minion.gameObject.AddComponent<EventTrigger>();
        
        eventTrigger.triggers.Clear();
        
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnMinionPointerDown(minion, (PointerEventData)data); });
        eventTrigger.triggers.Add(pointerDown);
        
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { OnMinionPointerUp(minion, (PointerEventData)data); });
        eventTrigger.triggers.Add(pointerUp);
        
        SpriteRenderer sr = minion.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalMinionColors[minion] = sr.color;
        }
        
        Debug.Log($"[MinionMovementManager] Set up movement for minion: {minion.minionData?.minionName ?? "Unknown"}");
    }
    
    public void OnMinionCreated(MinionController newMinion)
    {
        if (newMinion != null && IsPlayerMinion(newMinion))
        {
            playerMinions.Add(newMinion);
            SetupMinionForMovement(newMinion);
            Debug.Log($"[MinionMovementManager] Added new minion to movement system: {newMinion.minionData?.minionName ?? "Unknown"}");
        }
    }
    
    void Update()
    {
        if (enableDragAndDrop && isDragging && selectedMinion != null)
        {
            HandleDragMovement();
        }
        
        if (enableClickToMove)
        {
            HandleClickToMove();
        }
        
        UpdateVisualFeedback();
    }
    
    void HandleDragMovement()
    {
        Vector2 mousePosition = mousePositionAction.ReadValue<Vector2>();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
        worldPosition.z = selectedMinion.transform.position.z;
        
        selectedMinion.transform.position = worldPosition + dragOffset;
        
        CheckDropZone();
    }
    
    void HandleClickToMove()
    {
        if (mouseClickAction.WasPressedThisFrame() && !isDragging)
        {
            Vector2 mousePosition = mousePositionAction.ReadValue<Vector2>();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
            
            MinionController clickedMinion = GetMinionAtPosition(worldPosition);
            if (clickedMinion != null && IsPlayerMinion(clickedMinion))
            {
                SelectMinion(clickedMinion);
            }
            else if (selectedMinion != null)
            {
                CardSlotManager.CardSlot targetSlot = GetSlotAtPosition(worldPosition);
                if (targetSlot != null && targetSlot.CanPlace)
                {
                    MoveMinionToSlot(selectedMinion, targetSlot);
                    DeselectMinion();
                }
            }
        }
    }
    
    void OnMinionPointerDown(MinionController minion, PointerEventData eventData)
    {
        if (enableDragAndDrop)
        {
            SelectMinion(minion);
            isDragging = true;
            
            Vector2 mousePosition = mousePositionAction.ReadValue<Vector2>();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
            dragOffset = minion.transform.position - worldPosition;
            
            Debug.Log($"[MinionMovementManager] Started dragging: {minion.minionData?.minionName ?? "Unknown"}");
        }
    }
    
    void OnMinionPointerUp(MinionController minion, PointerEventData eventData)
    {
        if (isDragging && selectedMinion == minion)
        {
            isDragging = false;
            
            if (hoveredSlot != null && hoveredSlot.CanPlace)
            {
                MoveMinionToSlot(minion, hoveredSlot);
            }
            else
            {
                ReturnMinionToOriginalPosition(minion);
            }
            
            DeselectMinion();
            hoveredSlot = null;
            
            Debug.Log($"[MinionMovementManager] Stopped dragging: {minion.minionData?.minionName ?? "Unknown"}");
        }
    }
    
    void SelectMinion(MinionController minion)
    {
        if (selectedMinion != null && selectedMinion != minion)
        {
            DeselectMinion();
        }
        
        selectedMinion = minion;
        
        CreateSelectionIndicator(minion);
        
        Debug.Log($"[MinionMovementManager] Selected minion: {minion.minionData?.minionName ?? "Unknown"}");
    }
    
    void DeselectMinion()
    {
        if (selectedMinion != null)
        {
            selectedMinion = null;
        }
        
        if (selectionIndicator != null)
        {
            DestroyImmediate(selectionIndicator);
            selectionIndicator = null;
        }
    }
    
    void CreateSelectionIndicator(MinionController minion)
    {
        if (selectionIndicator != null)
        {
            DestroyImmediate(selectionIndicator);
        }

        selectionIndicator = new GameObject("SelectionIndicator");
        selectionIndicator.transform.position = minion.transform.position;
        selectionIndicator.transform.SetParent(minion.transform);
        
        SpriteRenderer indicatorSr = selectionIndicator.AddComponent<SpriteRenderer>();
        indicatorSr.sprite = CreateSelectionOutlineSprite();
        indicatorSr.color = new Color(1f, 1f, 0f, 0.8f); // Semi-transparent yellow
        indicatorSr.sortingOrder = minion.GetComponent<SpriteRenderer>().sortingOrder + 1; // Above minion
        
        selectionIndicator.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
    }
    
    Sprite CreateSelectionOutlineSprite()
    {
        int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color[] pixels = new Color[size * size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isEdge = x == 0 || x == size - 1 || y == 0 || y == size - 1;
                bool isInnerEdge = x == 1 || x == size - 2 || y == 1 || y == size - 2;
                
                if (isEdge)
                {
                    pixels[y * size + x] = Color.white;
                }
                else if (isInnerEdge)
                {
                    pixels[y * size + x] = new Color(1f, 1f, 1f, 0.5f);
                }
                else
                {
                    pixels[y * size + x] = new Color(0f, 0f, 0f, 0f);
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        sprite.name = "SelectionOutlineSprite";
        
        return sprite;
    }
    
    void CheckDropZone()
    {
        Vector2 mousePosition = mousePositionAction.ReadValue<Vector2>();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
        
        CardSlotManager.CardSlot nearestSlot = GetSlotAtPosition(worldPosition);
        
        if (nearestSlot != hoveredSlot)
        {
            hoveredSlot = nearestSlot;
            
            if (hoveredSlot != null)
            {
                Debug.Log($"[MinionMovementManager] Hovering over slot: {hoveredSlot.type} {hoveredSlot.row} {hoveredSlot.slotIndex}");
            }
        }
    }
    
    CardSlotManager.CardSlot GetSlotAtPosition(Vector3 worldPosition)
    {
        CardSlotManager.CardSlot[] allSlots = cardSlotManager.AllPlayerSlots;
        float closestDistance = float.MaxValue;
        CardSlotManager.CardSlot closestSlot = null;
        
        foreach (var slot in allSlots)
        {
            float distance = Vector3.Distance(worldPosition, slot.worldPosition);
            if (distance < closestDistance && distance < 2f)
            {
                closestDistance = distance;
                closestSlot = slot;
            }
        }
        
        return closestSlot;
    }
    
    MinionController GetMinionAtPosition(Vector3 worldPosition)
    {
        foreach (var minion in playerMinions)
        {
            if (minion != null)
            {
                float distance = Vector3.Distance(worldPosition, minion.transform.position);
                if (distance < 1f)
                {
                    return minion;
                }
            }
        }
        return null;
    }
    
    void MoveMinionToSlot(MinionController minion, CardSlotManager.CardSlot targetSlot)
    {
        cardSlotManager.RemoveUnitFromSlot(minion.gameObject);
        
        if (cardSlotManager.PlaceUnitInSlot(minion.gameObject, targetSlot.type, targetSlot.row, targetSlot.slotIndex))
        {
            minion.transform.position = targetSlot.worldPosition;
            Debug.Log($"[MinionMovementManager] Moved {minion.minionData?.minionName ?? "Unknown"} to {targetSlot.type} {targetSlot.row} slot {targetSlot.slotIndex}");
        }
        else
        {
            Debug.LogError($"[MinionMovementManager] Failed to place minion in slot!");
            ReturnMinionToOriginalPosition(minion);
        }
    }
    
    void ReturnMinionToOriginalPosition(MinionController minion)
    {
        CardSlotManager.CardSlot[] allSlots = cardSlotManager.AllPlayerSlots;
        foreach (var slot in allSlots)
        {
            if (slot.occupant == minion.gameObject)
            {
                minion.transform.position = slot.worldPosition;
                break;
            }
        }
    }
    
    void UpdateVisualFeedback()
    {
        CardSlotManager.CardSlot[] allSlots = cardSlotManager.AllPlayerSlots;
        
        for (int i = 0; i < allSlots.Length && i < slotIndicators.Count; i++)
        {
            var slot = allSlots[i];
            var indicator = slotIndicators[i];
            
            if (indicator != null)
            {
                SpriteRenderer spriteRenderer = indicator.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Color indicatorColor;
                    
                    if (slot == hoveredSlot)
                    {
                        indicatorColor = slot.CanPlace ? validDropColor : invalidDropColor;
                    }
                    else if (slot.isOccupied)
                    {
                        indicatorColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                    }
                    else
                    {
                        indicatorColor = new Color(1f, 1f, 1f, 0.6f);
                    }
                    
                    spriteRenderer.color = indicatorColor;
                }
            }
        }
    }
    
    public void RefreshMinionList()
    {
        FindPlayerMinions();
    }
    
    public void SetDragAndDropEnabled(bool enabled)
    {
        enableDragAndDrop = enabled;
    }
    
    public void SetClickToMoveEnabled(bool enabled)
    {
        enableClickToMove = enabled;
    }
    
    void OnDestroy()
    {
        if (mouseClickAction != null)
            mouseClickAction.Disable();
        if (mousePositionAction != null)
            mousePositionAction.Disable();
    }
}