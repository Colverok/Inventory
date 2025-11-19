using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Connects InventoryModel with the user interface.
/// Processes UI requests (use, move, drop, sort) and updates views accordingly.
/// </summary>
public class InventoryController : MonoBehaviour
{
    [SerializeField] private int rows = 4;
    [SerializeField] private int cols = 5;
    [SerializeField] private InventoryDatabaseSO db;
    [SerializeField] private InventoryView view;
    [SerializeField] private InventoryDefaultUseHandler useHandler;

    private InventorySelection inventorySelection;
    private InventoryModel inventoryModel;

    public event Action<int> SelectionChanged;

    #region Properties
    public InventoryModel InventoryModel { get => inventoryModel; private set => inventoryModel = value; }
    public InventorySelection InventorySelection { get => inventorySelection; set => inventorySelection = value; }
    #endregion

    // monobeh

    private void Awake()
    {
        inventoryModel = new InventoryModel(db, rows, cols);
        inventorySelection = new InventorySelection();
        inventoryModel.OnSlotChanged += OnSlotChanged;

        view.Init(this);

        InventorySaveService.TryLoad(InventoryModel);

    }
    private void OnDestroy()
    {
        if (inventoryModel != null) inventoryModel.OnSlotChanged -= OnSlotChanged;
    }

    #region public methods

    public InventoryItemSO GetItemById(string id) => db.GetItemById(id);

    // UI

    public void RequestUse(int index)
    {
        InventoryModel.Use(index, useHandler);
    }
    
    public void RequestMove(int from, int to, bool merge)
    {
        InventoryModel.Move(from, to, merge);
    }
    public void RequestDrop(int index, int amount = int.MaxValue)
    {
        InventoryModel.Drop(index, amount);
    }
    public void RequestAdd(InventoryItemSO item)
    {
        InventoryModel.Add(new InventorySlot(item.Id, 1));
    }
    public void RequestAdd(InventoryItemSO item, int count)
    {
        InventoryModel.Add(new InventorySlot(item.Id, count));
    }

    public void RequestSelect(int index)
    {
        InventorySelection.Select(index);
        SelectionChanged?.Invoke(index);
    }
    public void RequestClearSelection()
    {
        InventorySelection.Clear();
        SelectionChanged?.Invoke(-1);
    }


    // Sorting
    public void SortByName()
    {
        inventoryModel.SortBy(x => x.DisplayName);
    }
    public void SortByType()
    {
        inventoryModel.SortBy(x => x.Type);
    }
    public void SortByCount()
    {
        inventoryModel.SortBy(x => x.Count);
    }

    // Save/Load

    public void Save() => InventorySaveService.Save(inventoryModel);
    public void Load() => InventorySaveService.TryLoad(inventoryModel);
    public void ClearSaves() => InventorySaveService.Clear();
    #endregion


    private void OnSlotChanged(int index, InventorySlot slot)
    {
        view.UpdateSlot(index, slot);
    }
}
