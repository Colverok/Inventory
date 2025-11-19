using UnityEngine;

public class InventoryDebugItemsAdder : MonoBehaviour
{
    [SerializeField] InventoryController inventoryController;

    public void AddOne(InventoryItemSO item)
    {
        inventoryController.RequestAdd(item);
    }
    public void AddFive(InventoryItemSO item)
    {
        inventoryController.RequestAdd(item, 5);
    }
        
}
