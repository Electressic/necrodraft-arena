using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GridSpawnManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    public int gridWidth = 8;
    public int gridHeight = 6;
    public float cellSize = 1.0f;
    public Vector2 gridOffset = Vector2.zero;
    
    [Header("Zone Configuration")]
    [Range(0, 8)]
    public int playerZoneWidth = 3; // Left side for player minions
    [Range(0, 8)]
    public int enemyZoneWidth = 3;  // Right side for enemies
    // Middle zone will be neutral/battle area
    
    [Header("Visual Debug")]
    public bool showGrid = true;
    public Color gridColor = Color.white;
    public Color playerZoneColor = Color.blue;
    public Color enemyZoneColor = Color.red;
    public Color neutralZoneColor = Color.yellow;
    public Color occupiedCellColor = Color.gray;
    
    // Grid data
    public GridCell[,] grid { get; private set; }
    private List<Transform> playerSpawnPoints = new List<Transform>();
    private List<Transform> enemySpawnPoints = new List<Transform>();
    private List<Transform> allSpawnPoints = new List<Transform>();
    
    // Spawn point parent objects for organization
    private Transform playerSpawnParent;
    private Transform enemySpawnParent;
    private Transform neutralSpawnParent;
    
    public struct GridCell
    {
        public Vector3 worldPosition;
        public bool isOccupied;
        public GameObject occupant;
        public GridZone zone;
    }
    
    public enum GridZone
    {
        PlayerZone,
        NeutralZone,
        EnemyZone
    }
    
    void Awake()
    {
        InitializeGrid();
        CreateSpawnPointHierarchy();
        CreateSpawnPoints();
    }
    
    void InitializeGrid()
    {
        grid = new GridCell[gridWidth, gridHeight];
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = new Vector3(
                    x * cellSize + gridOffset.x,
                    y * cellSize + gridOffset.y,
                    0
                );
                
                GridZone zone = DetermineZone(x);
                
                grid[x, y] = new GridCell
                {
                    worldPosition = worldPos,
                    isOccupied = false,
                    occupant = null,
                    zone = zone
                };
            }
        }
        
        Debug.Log($"[GridSpawnManager] Initialized {gridWidth}x{gridHeight} grid with cell size {cellSize}");
    }
    
    GridZone DetermineZone(int x)
    {
        if (x < playerZoneWidth)
            return GridZone.PlayerZone;
        else if (x >= gridWidth - enemyZoneWidth)
            return GridZone.EnemyZone;
        else
            return GridZone.NeutralZone;
    }
    
    void CreateSpawnPointHierarchy()
    {
        // Create parent objects for organization
        GameObject spawnParent = new GameObject("SpawnPoints");
        spawnParent.transform.SetParent(transform);
        
        playerSpawnParent = new GameObject("PlayerSpawnPoints").transform;
        playerSpawnParent.SetParent(spawnParent.transform);
        
        enemySpawnParent = new GameObject("EnemySpawnPoints").transform;
        enemySpawnParent.SetParent(spawnParent.transform);
        
        neutralSpawnParent = new GameObject("NeutralSpawnPoints").transform;
        neutralSpawnParent.SetParent(spawnParent.transform);
    }
    
    void CreateSpawnPoints()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = grid[x, y].worldPosition;
                GridZone zone = grid[x, y].zone;
                
                // Create spawn point GameObject
                GameObject spawnPoint = new GameObject($"SpawnPoint_{x}_{y}");
                spawnPoint.transform.position = position;
                
                // Set parent based on zone
                Transform parent = zone switch
                {
                    GridZone.PlayerZone => playerSpawnParent,
                    GridZone.EnemyZone => enemySpawnParent,
                    _ => neutralSpawnParent
                };
                spawnPoint.transform.SetParent(parent);
                
                // Add to appropriate lists
                if (zone == GridZone.PlayerZone)
                    playerSpawnPoints.Add(spawnPoint.transform);
                else if (zone == GridZone.EnemyZone)
                    enemySpawnPoints.Add(spawnPoint.transform);
                
                allSpawnPoints.Add(spawnPoint.transform);
            }
        }
        
        Debug.Log($"[GridSpawnManager] Created {playerSpawnPoints.Count} player spawn points, {enemySpawnPoints.Count} enemy spawn points");
    }
    
    // Public interface for CombatManager
    public Transform[] GetPlayerSpawnPoints()
    {
        return playerSpawnPoints.ToArray();
    }
    
    public Transform[] GetEnemySpawnPoints()
    {
        return enemySpawnPoints.ToArray();
    }
    
    public Vector3 GetBestPlayerSpawnPosition()
    {
        // Find the closest available player spawn point to the front
        Transform bestSpawn = null;
        float closestToFront = float.MaxValue;
        
        foreach (Transform spawn in playerSpawnPoints)
        {
            Vector2Int gridPos = WorldToGridPosition(spawn.position);
            if (!IsOccupied(gridPos.x, gridPos.y))
            {
                // Prefer positions closer to the enemy zone (higher X values in player zone)
                float distanceToFront = gridWidth - spawn.position.x;
                if (distanceToFront < closestToFront)
                {
                    closestToFront = distanceToFront;
                    bestSpawn = spawn;
                }
            }
        }
        
        return bestSpawn != null ? bestSpawn.position : GetClosestAvailablePosition(GridZone.PlayerZone);
    }
    
    public Vector3 GetBestEnemySpawnPosition()
    {
        // Find available enemy spawn point, prefer front positions
        Transform bestSpawn = null;
        float closestToFront = float.MaxValue;
        
        foreach (Transform spawn in enemySpawnPoints)
        {
            Vector2Int gridPos = WorldToGridPosition(spawn.position);
            if (!IsOccupied(gridPos.x, gridPos.y))
            {
                // Prefer positions closer to player zone (lower X values in enemy zone)
                float distanceToFront = spawn.position.x;
                if (distanceToFront < closestToFront)
                {
                    closestToFront = distanceToFront;
                    bestSpawn = spawn;
                }
            }
        }
        
        return bestSpawn != null ? bestSpawn.position : GetClosestAvailablePosition(GridZone.EnemyZone);
    }
    
    Vector3 GetClosestAvailablePosition(GridZone preferredZone)
    {
        // Fallback: find any available position in the preferred zone
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y].zone == preferredZone && !grid[x, y].isOccupied)
                {
                    return grid[x, y].worldPosition;
                }
            }
        }
        
        // Last resort: any available position
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y].isOccupied)
                {
                    return grid[x, y].worldPosition;
                }
            }
        }
        
        return Vector3.zero; // Grid is full
    }
    
    public bool SetOccupied(Vector3 worldPosition, GameObject occupant)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        if (IsValidGridPosition(gridPos.x, gridPos.y))
        {
            grid[gridPos.x, gridPos.y].isOccupied = true;
            grid[gridPos.x, gridPos.y].occupant = occupant;
            return true;
        }
        return false;
    }
    
    public void SetFree(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        if (IsValidGridPosition(gridPos.x, gridPos.y))
        {
            grid[gridPos.x, gridPos.y].isOccupied = false;
            grid[gridPos.x, gridPos.y].occupant = null;
        }
    }
    
    public bool IsOccupied(int x, int y)
    {
        if (IsValidGridPosition(x, y))
            return grid[x, y].isOccupied;
        return true; // Consider invalid positions as occupied
    }
    
    Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - gridOffset.x) / cellSize);
        int y = Mathf.RoundToInt((worldPosition.y - gridOffset.y) / cellSize);
        return new Vector2Int(x, y);
    }
    
    bool IsValidGridPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }
    
    // Utility methods for advanced placement
    public List<Vector3> GetAvailablePositionsInZone(GridZone zone, int maxCount = -1)
    {
        List<Vector3> positions = new List<Vector3>();
        int count = 0;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y].zone == zone && !grid[x, y].isOccupied)
                {
                    positions.Add(grid[x, y].worldPosition);
                    count++;
                    
                    if (maxCount > 0 && count >= maxCount)
                        return positions;
                }
            }
        }
        
        return positions;
    }
    
    // Formation helpers
    public List<Vector3> GetFormationPositions(GridZone zone, int unitCount, FormationType formation = FormationType.Line)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> available = GetAvailablePositionsInZone(zone);
        
        if (available.Count == 0) return positions;
        
        switch (formation)
        {
            case FormationType.Line:
                return GetLineFormation(available, unitCount, zone);
            case FormationType.Triangle:
                return GetTriangleFormation(available, unitCount);
            case FormationType.Spread:
                return GetSpreadFormation(available, unitCount);
            default:
                return available.Take(unitCount);
        }
    }
    
    List<Vector3> GetLineFormation(List<Vector3> available, int count, GridZone zone)
    {
        // Sort by Y position for horizontal line, prefer middle rows
        available.Sort((a, b) => Mathf.Abs(a.y - (gridHeight * cellSize / 2)).CompareTo(Mathf.Abs(b.y - (gridHeight * cellSize / 2))));
        return available.Take(count);
    }
    
    List<Vector3> GetTriangleFormation(List<Vector3> available, int count)
    {
        // Implement triangle formation logic
        return available.Take(count);
    }
    
    List<Vector3> GetSpreadFormation(List<Vector3> available, int count)
    {
        // Spread units evenly across available positions
        List<Vector3> positions = new List<Vector3>();
        if (available.Count <= count)
            return available;
        
        float step = (float)available.Count / count;
        for (int i = 0; i < count; i++)
        {
            int index = Mathf.RoundToInt(i * step);
            if (index < available.Count)
                positions.Add(available[index]);
        }
        
        return positions;
    }
    
    public enum FormationType
    {
        Line,
        Triangle,
        Spread
    }
    
    // Visual debugging - works in both edit and play mode
    void OnDrawGizmos()
    {
        if (!showGrid || grid == null)
            return;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = new Vector3(
                    x * cellSize + gridOffset.x,
                    y * cellSize + gridOffset.y,
                    0
                );
                
                // Determine zone (works even if grid array is not initialized)
                GridZone zone = DetermineZone(x);
                bool occupied = grid != null && IsValidGridPosition(x, y) && grid[x, y].isOccupied;
                
                // Set color based on zone and occupation
                Color cellColor = zone switch
                {
                    GridZone.PlayerZone => playerZoneColor,
                    GridZone.EnemyZone => enemyZoneColor,
                    _ => neutralZoneColor
                };
                
                if (occupied)
                    cellColor = occupiedCellColor;
                
                // Draw filled cell with zone color
                Gizmos.color = new Color(cellColor.r, cellColor.g, cellColor.b, 0.3f); // Semi-transparent
                Gizmos.DrawCube(position, Vector3.one * cellSize * 0.8f);
                
                // Draw grid lines
                Gizmos.color = gridColor;
                Gizmos.DrawWireCube(position, Vector3.one * cellSize);
            }
        }
        
        // Draw occupied status
        if (Application.isPlaying)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y].isOccupied)
                    {
                        Gizmos.color = occupiedCellColor;
                        Gizmos.DrawCube(grid[x, y].worldPosition, new Vector3(cellSize, cellSize, 0.1f) * 0.5f);
                    }
                }
            }
        }
    }
}

// Extension method for List.Take functionality (Unity recompile trigger)
public static class ListExtensions
{
    public static List<T> Take<T>(this List<T> source, int count)
    {
        List<T> result = new List<T>();
        for (int i = 0; i < Mathf.Min(count, source.Count); i++)
        {
            result.Add(source[i]);
        }
        return result;
    }
} 