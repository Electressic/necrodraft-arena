using UnityEngine;

[RequireComponent(typeof(GridSpawnManager))]
public class GridVisualizer : MonoBehaviour
{
    [Header("Visual Configuration")]
    [Range(0f, 1f)]
    public float cellAlpha = 0.1f;
    public Color gridLineColor = Color.black;
    public float gridLineWidth = 0.05f;

    private GameObject gridVisualsContainer;
    private GridSpawnManager gridManager;
    private Sprite generatedCellSprite;

    void Start()
    {
        gridManager = GetComponent<GridSpawnManager>();
        if (gridManager == null)
        {
            Debug.LogError("[GridVisualizer] GridSpawnManager component not found!");
            return;
        }

        generatedCellSprite = CreateDefaultSprite();
        CreateGridVisuals();
        CreateGridLines();
    }

    private Sprite CreateDefaultSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void CreateGridVisuals()
    {
        if (gridVisualsContainer != null)
        {
            if (Application.isPlaying)
            {
                Destroy(gridVisualsContainer);
            }
            else
            {
                DestroyImmediate(gridVisualsContainer);
            }
        }

        gridVisualsContainer = new GameObject("GridVisuals");
        gridVisualsContainer.transform.SetParent(transform);
        gridVisualsContainer.transform.localPosition = Vector3.zero;

        if (gridManager.grid == null)
        {
            Debug.LogWarning("[GridVisualizer] Grid not initialized in GridSpawnManager. Can't create visuals.");
            return;
        }

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                var cellInfo = gridManager.grid[x, y];
                Vector3 position = cellInfo.worldPosition;
                GridSpawnManager.GridZone zone = cellInfo.zone;

                GameObject cellObject = new GameObject($"GridCell_{x}_{y}");
                cellObject.transform.position = position;
                cellObject.transform.SetParent(gridVisualsContainer.transform);
                cellObject.transform.localScale = new Vector3(gridManager.cellSize, gridManager.cellSize, 1f);

                SpriteRenderer sr = cellObject.AddComponent<SpriteRenderer>();
                sr.sprite = generatedCellSprite;
                sr.sortingOrder = -10;

                Color zoneColor = GetZoneColor(zone);
                zoneColor.a = cellAlpha;
                sr.color = zoneColor;
            }
        }
    }

    private void CreateGridLines()
    {
        GameObject gridLinesContainer = new GameObject("GridLines");
        gridLinesContainer.transform.SetParent(transform);
        gridLinesContainer.transform.localPosition = Vector3.zero;

        // Create a default material for the LineRenderer
        var lineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        // Horizontal lines
        for (int y = 0; y <= gridManager.gridHeight; y++)
        {
            GameObject lineObj = new GameObject($"H_Line_{y}");
            lineObj.transform.SetParent(gridLinesContainer.transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            
            lr.material = lineMaterial;
            lr.startColor = lr.endColor = gridLineColor;
            lr.startWidth = lr.endWidth = gridLineWidth;
            lr.positionCount = 2;
            lr.sortingOrder = -9; // Render on top of cells

            float yPos = y * gridManager.cellSize + gridManager.gridOffset.y - (gridManager.cellSize / 2f);
            float xStart = gridManager.gridOffset.x - (gridManager.cellSize / 2f);
            float xEnd = (gridManager.gridWidth * gridManager.cellSize) + gridManager.gridOffset.x - (gridManager.cellSize / 2f);

            lr.SetPosition(0, new Vector3(xStart, yPos, 0));
            lr.SetPosition(1, new Vector3(xEnd, yPos, 0));
        }

        // Vertical lines
        for (int x = 0; x <= gridManager.gridWidth; x++)
        {
            GameObject lineObj = new GameObject($"V_Line_{x}");
            lineObj.transform.SetParent(gridLinesContainer.transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            lr.material = lineMaterial;
            lr.startColor = lr.endColor = gridLineColor;
            lr.startWidth = lr.endWidth = gridLineWidth;
            lr.positionCount = 2;
            lr.sortingOrder = -9;

            float xPos = x * gridManager.cellSize + gridManager.gridOffset.x - (gridManager.cellSize / 2f);
            float yStart = gridManager.gridOffset.y - (gridManager.cellSize / 2f);
            float yEnd = (gridManager.gridHeight * gridManager.cellSize) + gridManager.gridOffset.y - (gridManager.cellSize / 2f);

            lr.SetPosition(0, new Vector3(xPos, yStart, 0));
            lr.SetPosition(1, new Vector3(xPos, yEnd, 0));
        }
    }

    private Color GetZoneColor(GridSpawnManager.GridZone zone)
    {
        switch (zone)
        {
            case GridSpawnManager.GridZone.PlayerZone:
                return gridManager.playerZoneColor;
            case GridSpawnManager.GridZone.EnemyZone:
                return gridManager.enemyZoneColor;
            case GridSpawnManager.GridZone.NeutralZone:
                return gridManager.neutralZoneColor;
            default:
                return gridManager.gridColor;
        }
    }
} 