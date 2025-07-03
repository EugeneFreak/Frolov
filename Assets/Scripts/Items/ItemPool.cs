using UnityEngine;
using System.Collections.Generic;

public class ItemPool : MonoBehaviour
{
    [Header("Item Prefabs")]
    [SerializeField] private List<GameObject> itemPrefabs = new List<GameObject>();
    
    [Header("Pool Settings")]
    [SerializeField] private int itemsPerType = 1;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private int itemsPerRow = 3;
    
    // Pool data
    private List<InventoryItem> pooledItems = new List<InventoryItem>();
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    private void Start()
    {
        CreateItemPool();
    }
    
    private void CreateItemPool()
    {
        // Clear existing items
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        pooledItems.Clear();
        
        // Create items
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs[i] != null)
            {
                for (int j = 0; j < itemsPerType; j++)
                {
                    GameObject itemObj = Instantiate(itemPrefabs[i], transform);
                    InventoryItem item = itemObj.GetComponent<InventoryItem>();
                    
                    if (item != null)
                    {
                        pooledItems.Add(item);
                        PositionItemInPool(pooledItems.Count - 1, item);
                    }
                }
            }
        }
    }
    
    private void PositionItemInPool(int index, InventoryItem item)
    {
        int row = index / itemsPerRow;
        int col = index % itemsPerRow;
        
        float startX = -(itemsPerRow - 1) * itemSpacing * 0.5f;
        float startY = -row * itemSpacing;
        
        Vector3 position = new Vector3(startX + col * itemSpacing, startY, 0);
        item.transform.localPosition = position;
        
        Debug.Log($"Positioning item {index}: row={row}, col={col}, pos={position}");
    }
    
    public void ReturnItem(InventoryItem item)
    {
        if (item == null) return;
        
        // Reset item state
        item.transform.SetParent(transform);
        
        // Find item index and reposition
        int index = pooledItems.IndexOf(item);
        if (index >= 0)
        {
            PositionItemInPool(index, item);
        }
        else
        {
            // If not found, add to pool and position
            pooledItems.Add(item);
            PositionItemInPool(pooledItems.Count - 1, item);
        }
    }
    
    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(pooledItems);
    }
}
