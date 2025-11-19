using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry of all available items in the game.
/// Provides fast lookup by ID and ensures items are consistent across scenes.
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Inventory Database", fileName = "Inventory Database")]
public class InventoryDatabaseSO : ScriptableObject
{
    [SerializeField] private List<InventoryItemSO> items = new();

    private Dictionary<string, InventoryItemSO> mapById;

    #region Properties
    public List<InventoryItemSO> Items { get => items; set => items = value; }

    #endregion

    private void Init()
    {
        if (mapById != null) return;
        mapById = new Dictionary<string, InventoryItemSO>();
        foreach (InventoryItemSO item in items)
        {
            if (item != null && !mapById.ContainsKey(item.Id))
            {
                mapById.Add(item.Id, item);
            }
        }
    }

    public InventoryItemSO GetItemById(string id)
    {
        Init();
        return mapById.TryGetValue(id, out var res) ? res : null;
    }
}
