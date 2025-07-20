using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardSlotManager : MonoBehaviour
{
    [Header("Row Configuration")]
    public int frontRowSlots = 3;
    public int backRowSlots = 3;
    public float slotSpacing = 2.0f;
    public float rowSeparation = 4.0f;
    public float rowDepth = 2.5f;
    
    [Header("Visual References")]
    public Transform playerSlotParent;
    public Transform enemySlotParent;
    public GameObject slotIndicatorPrefab;
    
    [Header("Resolution Scaling")]
    public bool useAdaptiveSpacing = true;
    [Range(0.5f, 2.0f)]
    public float spacingScale = 1.0f;
    
    [Header("Debug")]
    public bool showSlotGizmos = true;
    public Color frontRowColor = Color.blue;
    public Color backRowColor = Color.cyan;
    public Color enemyFrontRowColor = Color.red;
    public Color enemyBackRowColor = Color.magenta;
    public Color occupiedSlotColor = Color.gray;
    
    public CardSlot[] playerFrontRow { get; private set; }
    public CardSlot[] playerBackRow { get; private set; }
    public CardSlot[] enemyFrontRow { get; private set; }
    public CardSlot[] enemyBackRow { get; private set; }
    
    [System.Serializable]
    public class CardSlot
    {
        public Transform slotTransform;
        public Vector3 worldPosition;
        public bool isOccupied;
        public GameObject occupant;
        public int slotIndex;
        public SlotType type;
        public RowType row;
        
        public bool IsEmpty => !isOccupied && occupant == null;
        public bool CanPlace => IsEmpty;
        public bool IsFrontRow => row == RowType.Front;
        public bool IsBackRow => row == RowType.Back;
    }
    
    public enum SlotType
    {
        Player,
        Enemy
    }
    
    public enum RowType
    {
        Front,
        Back
    }

    public CardSlot[] AllPlayerSlots => playerFrontRow.Concat(playerBackRow).ToArray();
    public CardSlot[] AllEnemySlots => enemyFrontRow.Concat(enemyBackRow).ToArray();
    public CardSlot[] AllSlots => AllPlayerSlots.Concat(AllEnemySlots).ToArray();

    void Awake()
    {
        InitializeSlotSystem();
    }
    
    void Start()
    {
        if (useAdaptiveSpacing)
        {
            AdaptToScreenSize();
        }
        
        SetupSlotPositions();
    }
    
    void InitializeSlotSystem()
    {
        playerFrontRow = new CardSlot[frontRowSlots];
        playerBackRow = new CardSlot[backRowSlots];
        enemyFrontRow = new CardSlot[frontRowSlots];
        enemyBackRow = new CardSlot[backRowSlots];
        
        CreateSlotParents();
        CreateSlotObjects();
    }
    
    void CreateSlotParents()
    {
        if (playerSlotParent == null)
        {
            GameObject playerParent = new GameObject("PlayerSlots");
            playerParent.transform.SetParent(transform);
            playerSlotParent = playerParent.transform;
        }
        
        if (enemySlotParent == null)
        {
            GameObject enemyParent = new GameObject("EnemySlots");
            enemyParent.transform.SetParent(transform);
            enemySlotParent = enemyParent.transform;
        }
    }
    
    void CreateSlotObjects()
    {
        for (int i = 0; i < frontRowSlots; i++)
        {
            GameObject slotObj = new GameObject($"PlayerFrontSlot_{i + 1}");
            slotObj.transform.SetParent(playerSlotParent);
            
            playerFrontRow[i] = new CardSlot
            {
                slotTransform = slotObj.transform,
                slotIndex = i,
                type = SlotType.Player,
                row = RowType.Front,
                isOccupied = false,
                occupant = null
            };
        }
        
        for (int i = 0; i < backRowSlots; i++)
        {
            GameObject slotObj = new GameObject($"PlayerBackSlot_{i + 1}");
            slotObj.transform.SetParent(playerSlotParent);
            
            playerBackRow[i] = new CardSlot
            {
                slotTransform = slotObj.transform,
                slotIndex = i,
                type = SlotType.Player,
                row = RowType.Back,
                isOccupied = false,
                occupant = null
            };
        }
        
        for (int i = 0; i < frontRowSlots; i++)
        {
            GameObject slotObj = new GameObject($"EnemyFrontSlot_{i + 1}");
            slotObj.transform.SetParent(enemySlotParent);
            
            enemyFrontRow[i] = new CardSlot
            {
                slotTransform = slotObj.transform,
                slotIndex = i,
                type = SlotType.Enemy,
                row = RowType.Front,
                isOccupied = false,
                occupant = null
            };
        }
        
        for (int i = 0; i < backRowSlots; i++)
        {
            GameObject slotObj = new GameObject($"EnemyBackSlot_{i + 1}");
            slotObj.transform.SetParent(enemySlotParent);
            
            enemyBackRow[i] = new CardSlot
            {
                slotTransform = slotObj.transform,
                slotIndex = i,
                type = SlotType.Enemy,
                row = RowType.Back,
                isOccupied = false,
                occupant = null
            };
        }
    }
    
    void AdaptToScreenSize()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.orthographic)
        {
            float screenWidth = mainCam.orthographicSize * 2.0f * mainCam.aspect;
            float availableWidth = screenWidth * 0.8f;
            
            float maxTotalWidth = (frontRowSlots - 1) * slotSpacing;
            if (maxTotalWidth > availableWidth)
            {
                spacingScale = availableWidth / maxTotalWidth;
            }
        }
    }
    
    void SetupSlotPositions()
    {
        float adjustedSpacing = slotSpacing * spacingScale;
        
        SetupRowPositions(playerFrontRow, frontRowSlots, adjustedSpacing, -rowSeparation / 2f, 0f);
        SetupRowPositions(playerBackRow, backRowSlots, adjustedSpacing, -rowSeparation / 2f - rowDepth, 0f);
        SetupRowPositions(enemyFrontRow, frontRowSlots, adjustedSpacing, rowSeparation / 2f, 0f);
        SetupRowPositions(enemyBackRow, backRowSlots, adjustedSpacing, rowSeparation / 2f + rowDepth, 0f);
    }
    
    void SetupRowPositions(CardSlot[] row, int slotCount, float spacing, float baseY, float offsetZ)
    {
        float totalWidth = (slotCount - 1) * spacing;
        float startX = -totalWidth / 2f;
        
        for (int i = 0; i < slotCount; i++)
        {
            Vector3 position = new Vector3(
                startX + (i * spacing),
                baseY,
                offsetZ
            );
            
            row[i].slotTransform.position = position;
            row[i].worldPosition = position;
        }
    }

    public CardSlot[] GetRow(SlotType slotType, RowType rowType)
    {
        if (slotType == SlotType.Player)
        {
            return rowType == RowType.Front ? playerFrontRow : playerBackRow;
        }
        else
        {
            return rowType == RowType.Front ? enemyFrontRow : enemyBackRow;
        }
    }
    
    public CardSlot GetNearestEmptySlot(SlotType slotType, RowType preferredRow, Vector3 preferredPosition)
    {
        CardSlot[] primaryRow = GetRow(slotType, preferredRow);
        CardSlot[] fallbackRow = GetRow(slotType, preferredRow == RowType.Front ? RowType.Back : RowType.Front);
        
        var availableSlot = primaryRow
            .Where(slot => slot.CanPlace)
            .OrderBy(slot => Vector3.Distance(slot.worldPosition, preferredPosition))
            .FirstOrDefault();
            
        if (availableSlot == null)
        {
            availableSlot = fallbackRow
                .Where(slot => slot.CanPlace)
                .OrderBy(slot => Vector3.Distance(slot.worldPosition, preferredPosition))
                .FirstOrDefault();
        }
        
        return availableSlot;
    }
    
    public CardSlot GetSlotByIndex(SlotType slotType, RowType rowType, int index)
    {
        CardSlot[] targetRow = GetRow(slotType, rowType);
        if (index < 0 || index >= targetRow.Length) return null;
        return targetRow[index];
    }
    
    public bool PlaceUnitInSlot(GameObject unit, SlotType slotType, RowType preferredRow = RowType.Front, int slotIndex = -1)
    {
        CardSlot targetSlot;
        
        if (slotIndex >= 0)
        {
            targetSlot = GetSlotByIndex(slotType, preferredRow, slotIndex);
        }
        else
        {
            targetSlot = GetNearestEmptySlot(slotType, preferredRow, unit.transform.position);
        }
        
        if (targetSlot != null && targetSlot.CanPlace)
        {
            unit.transform.position = targetSlot.worldPosition;
            targetSlot.occupant = unit;
            targetSlot.isOccupied = true;
            return true;
        }
        
        return false;
    }
    
    public void RemoveUnitFromSlot(GameObject unit)
    {
        var occupiedSlot = AllSlots.FirstOrDefault(slot => slot.occupant == unit);
        
        if (occupiedSlot != null)
        {
            occupiedSlot.occupant = null;
            occupiedSlot.isOccupied = false;
        }
    }
    
    public List<CardSlot> GetOccupiedSlots(SlotType slotType, RowType? rowType = null)
    {
        var allSlots = slotType == SlotType.Player ? AllPlayerSlots : AllEnemySlots;
        var query = allSlots.Where(slot => slot.isOccupied);
        
        if (rowType.HasValue)
        {
            query = query.Where(slot => slot.row == rowType.Value);
        }
        
        return query.ToList();
    }
} 