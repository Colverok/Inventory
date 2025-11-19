using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryDefaultUseHandler : MonoBehaviour, IInventoryItemUseHandler
{
    public bool HandleUse(InventoryItemSO item)
    {
        switch (item.Type)
        {
            case InventoryItemType.Weapon:
                Debug.Log($"Equipped weapon: {item.DisplayName}.");
                return false;
            case InventoryItemType.Potion:
                Debug.Log($"Used potion: {item.DisplayName}. Restored HP.");
                return true;
            default:
                Debug.Log($"Used item: {item.DisplayName}.");
                return true;

        }
    }

}
