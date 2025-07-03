using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    [Header("Cell Settings")]
    [SerializeField] private Image cellImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.green;
    
    [Header("Cell Data")]
    public int x;
    public int y;
    public bool isOccupied = false;
    public InventoryItem occupiedItem = null;
    
    private void Awake()
    {
        if (cellImage == null)
            cellImage = GetComponent<Image>();
    }
    
    public void SetPosition(int posX, int posY)
    {
        x = posX;
        y = posY;
        gameObject.name = $"Cell_{x}_{y}";
    }
    
    public void SetOccupied(bool occupied, InventoryItem item = null)
    {
        isOccupied = occupied;
        occupiedItem = item;
    }
    
    public void HighlightCell(bool highlight)
    {
        if (cellImage != null)
        {
            cellImage.color = highlight ? highlightColor : normalColor;
        }
    }
    
    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }
    
    public Vector3 GetCenterPosition()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        return rectTransform.position;
    }
}
