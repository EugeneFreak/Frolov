using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Settings")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private Vector2Int[] itemShape;
    [SerializeField] private Image itemImage;
    
    [Header("Drag Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    
    // State
    private Vector3 originalPosition;
    private Transform originalParent;
    private bool isInInventory = false;
    private Vector2Int gridPosition;
    
    // References
    private InventoryGrid inventoryGrid;
    private ItemPool itemPool;
    
    public ItemType ItemType => itemType;
    public Vector2Int[] ItemShape => itemShape;
    public bool IsInInventory => isInInventory;
    public Vector2Int GridPosition => gridPosition;
    
    private void Awake()
    {
        if (itemImage == null)
            itemImage = GetComponent<Image>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }
    
    private void Start()
    {
        inventoryGrid = FindObjectOfType<InventoryGrid>();
        itemPool = FindObjectOfType<ItemPool>();
        
        SetupItemShape();
    }
    
    private void SetupItemShape()
    {
        switch (itemType)
        {
            case ItemType.Small1x1:
                itemShape = new Vector2Int[] { new Vector2Int(0, 0) };
                break;
                
            case ItemType.Medium2x2:
                itemShape = new Vector2Int[] 
                { 
                    new Vector2Int(0, 0), new Vector2Int(1, 0),
                    new Vector2Int(0, 1), new Vector2Int(1, 1)
                };
                break;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"OnBeginDrag started for {gameObject.name}");
        
        originalPosition = transform.position;
        originalParent = transform.parent;
        
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
        
        if (isInInventory && inventoryGrid != null)
        {
            inventoryGrid.RemoveItem(this);
            isInInventory = false;
        }
        
        transform.SetParent(canvas.transform);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        
        if (inventoryGrid != null)
        {
            Vector2Int gridPos = inventoryGrid.GetGridPosition(transform.position);
            ClearAllHighlights();
            
            if (inventoryGrid.CanPlaceItem(gridPos.x, gridPos.y, itemShape, this))
            {
                inventoryGrid.HighlightCells(gridPos.x, gridPos.y, itemShape, true);
            }
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("NEW OnEndDrag started!");
        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        ClearAllHighlights();
        
        bool placed = false;
        
        if (inventoryGrid != null)
        {
            Vector2Int gridPos = inventoryGrid.GetGridPosition(eventData.position);
            Debug.Log($"Position: {gridPos}");
            
            if (gridPos.x >= 0 && gridPos.x < inventoryGrid.GridWidth && 
                gridPos.y >= 0 && gridPos.y < inventoryGrid.GridHeight)
            {
                Debug.Log("Valid position, clearing occupied items...");
                
                // Clear occupied items
                foreach (Vector2Int offset in itemShape)
                {
                    int checkX = gridPos.x + offset.x;
                    int checkY = gridPos.y + offset.y;
                    
                    if (checkX >= 0 && checkX < inventoryGrid.GridWidth && 
                        checkY >= 0 && checkY < inventoryGrid.GridHeight)
                    {
                        GridCell cell = inventoryGrid.GetCell(checkX, checkY);
                        if (cell != null && cell.occupiedItem != null && cell.occupiedItem != this)
                        {
                            Debug.Log($"Removing item at ({checkX}, {checkY})");
                            InventoryItem oldItem = cell.occupiedItem;
                            inventoryGrid.RemoveItem(oldItem);
                            oldItem.ReturnToPool();
                        }
                    }
                }
                
                if (inventoryGrid.CanPlaceItem(gridPos.x, gridPos.y, itemShape))
                {
                    Debug.Log("Placing item!");
                    inventoryGrid.PlaceItem(gridPos.x, gridPos.y, itemShape, this);
                    PlaceInInventory(gridPos, inventoryGrid.GetCell(gridPos.x, gridPos.y));
                    placed = true;
                }
            }
        }
        
        if (!placed)
        {
            Debug.Log("Returning to pool");
            ReturnToPool();
        }
    }
    
    private void PlaceInInventory(Vector2Int gridPos, GridCell targetCell)
    {
        transform.SetParent(targetCell.transform, false);
        
        RectTransform itemRect = GetComponent<RectTransform>();
        RectTransform cellRect = targetCell.GetComponent<RectTransform>();
        
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.localScale = Vector3.one;
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        
        if (itemType == ItemType.Small1x1)
        {
            itemRect.sizeDelta = new Vector2(cellRect.sizeDelta.x * 0.8f, cellRect.sizeDelta.y * 0.8f);
        }
        else if (itemType == ItemType.Medium2x2)
        {
            float cellSize = cellRect.sizeDelta.x;
            float spacing = inventoryGrid.transform.GetComponent<GridLayoutGroup>().spacing.x;
            float totalSize = (cellSize * 2) + spacing;
            
            itemRect.sizeDelta = new Vector2(totalSize, totalSize);
            float offset = (cellSize + spacing) * 0.5f;
            itemRect.anchoredPosition = new Vector2(offset, -offset);
        }
        
        transform.SetAsLastSibling();
        isInInventory = true;
        gridPosition = gridPos;
    }
    
    private void ClearAllHighlights()
    {
        if (inventoryGrid != null)
        {
            for (int x = 0; x < inventoryGrid.GridWidth; x++)
            {
                for (int y = 0; y < inventoryGrid.GridHeight; y++)
                {
                    GridCell cell = inventoryGrid.GetCell(x, y);
                    if (cell != null)
                        cell.HighlightCell(false);
                }
            }
        }
    }
    
    public void ReturnToPool()
    {
        if (itemPool != null)
        {
            itemPool.ReturnItem(this);
        }
        else
        {
            transform.position = originalPosition;
            transform.SetParent(originalParent);
        }
        
        isInInventory = false;
    }
}

public enum ItemType
{
    Small1x1,
    Medium2x2
}