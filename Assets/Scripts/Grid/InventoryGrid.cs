using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 3;
    [SerializeField] private int gridHeight = 3;
    [SerializeField] private float cellSpacing = 5f;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject cellPrefab;
    
    [Header("Grid Container")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    
    // Grid data
    private GridCell[,] gridCells;
    private RectTransform rectTransform;
    private Canvas canvas;
    
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (gridLayoutGroup == null)
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
    }
    
    private void Start()
    {
        CreateGrid();
        SetupGridLayout();
    }
    
    private void CreateGrid()
    {
        gridCells = new GridCell[gridWidth, gridHeight];
        
        // Clear existing cells
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        
        // Create new cells
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, transform);
                GridCell cell = cellObj.GetComponent<GridCell>();
                
                if (cell == null)
                    cell = cellObj.AddComponent<GridCell>();
                
                cell.SetPosition(x, y);
                gridCells[x, y] = cell;
            }
        }
    }
    
    private void SetupGridLayout()
    {
        if (gridLayoutGroup == null) return;
        
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = gridWidth;
        gridLayoutGroup.spacing = new Vector2(cellSpacing, cellSpacing);
        
        // Calculate cell size based on available space
        float availableWidth = rectTransform.rect.width - (cellSpacing * (gridWidth - 1));
        float availableHeight = rectTransform.rect.height - (cellSpacing * (gridHeight - 1));
        
        float cellSize = Mathf.Min(availableWidth / gridWidth, availableHeight / gridHeight);
        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
    }
    
    public GridCell GetCell(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return gridCells[x, y];
        return null;
    }
    
    public bool CanPlaceItem(int startX, int startY, Vector2Int[] itemShape)
    {
        return CanPlaceItem(startX, startY, itemShape, null);
    }
    
    public bool CanPlaceItem(int startX, int startY, Vector2Int[] itemShape, InventoryItem excludeItem = null)
    {
        foreach (Vector2Int offset in itemShape)
        {
            int cellX = startX + offset.x;
            int cellY = startY + offset.y;
            
            GridCell cell = GetCell(cellX, cellY);
            if (cell == null || (cell.isOccupied && cell.occupiedItem != excludeItem))
                return false;
        }
        return true;
    }
    
    public void PlaceItem(int startX, int startY, Vector2Int[] itemShape, InventoryItem item)
    {
        foreach (Vector2Int offset in itemShape)
        {
            int cellX = startX + offset.x;
            int cellY = startY + offset.y;
            
            GridCell cell = GetCell(cellX, cellY);
            if (cell != null)
                cell.SetOccupied(true, item);
        }
    }
    
    public void RemoveItem(InventoryItem item)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridCell cell = gridCells[x, y];
                if (cell.occupiedItem == item)
                {
                    cell.SetOccupied(false, null);
                }
            }
        }
    }
    
    public void HighlightCells(int startX, int startY, Vector2Int[] itemShape, bool highlight)
    {
        foreach (Vector2Int offset in itemShape)
        {
            int cellX = startX + offset.x;
            int cellY = startY + offset.y;
            
            GridCell cell = GetCell(cellX, cellY);
            if (cell != null)
                cell.HighlightCell(highlight);
        }
    }
    
    public Vector2Int GetGridPosition(Vector3 screenPosition)
    {
        // Ищем ближайшую ячейку по реальным мировым позициям
        float minDistance = float.MaxValue;
        Vector2Int closestGridPos = new Vector2Int(0, 0);
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridCell cell = GetCell(x, y);
                if (cell != null)
                {
                    // Получаем реальную мировую позицию ячейки
                    Vector3 cellWorldPos = cell.transform.position;
                    
                    // Вычисляем расстояние до курсора в мировых координатах
                    float distance = Vector2.Distance(screenPosition, cellWorldPos);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestGridPos = new Vector2Int(x, y);
                    }
                }
            }
        }
        
        Debug.Log($"Mouse screen: {screenPosition}, Closest cell: {closestGridPos} at distance: {minDistance:F1}");
        
        return closestGridPos;
    }
}