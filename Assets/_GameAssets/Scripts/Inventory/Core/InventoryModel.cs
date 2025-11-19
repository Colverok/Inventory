using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct InventorySlot
{
    [SerializeField] private string itemId;
    [SerializeField] private int count;

    #region Properties
    public string ItemId { get => itemId; set => itemId = value; }
    public int Count { get => count; set => count = value; }
    #endregion

    public InventorySlot(string itemId, int count)
    {
        this.itemId = itemId;
        this.count = count;
    }

    public bool IsEmpty => string.IsNullOrEmpty(itemId) || count == 0;

}

/// <summary>
/// Data and business logic for the inventory.
/// Manages item stacks, movement, merge/swap operations and sorting
/// </summary>
public class InventoryModel
{
    private readonly InventoryDatabaseSO db;
    private readonly InventorySlot[] slots;
    private int rows;
    private int cols;

    #region Properties
    public int SlotsCount => slots.Length;
    public int Rows { get => rows; set => rows = value; }
    public int Cols { get => cols; set => cols = value; }
    public InventorySlot[] Slots => slots;
    #endregion

    #region Events
    public event Action<int, InventorySlot> OnSlotChanged;
    #endregion

    public InventoryModel(InventoryDatabaseSO db, int rows, int cols)
    {
        this.db = db;
        this.rows = rows;
        this.cols = cols;
        slots = new InventorySlot[rows * cols];
    }

    #region public methods

    public InventorySlot GetSlot(int index) => slots[index];

    /// <summary>
    /// Add one slot to inventory
    /// </summary>
    /// <param name="addingSlot"></param>
    /// <returns>Returns true, if operation is valid</returns>
    public bool Add(InventorySlot addingSlot)
    {
        if (addingSlot.IsEmpty) return false;

        // Try add to existing slots
        for (int i = 0; i < slots.Length && addingSlot.Count > 0; i++)
        {
            InventorySlot existingSlot = slots[i];

            // check if can stack slots
            if (!CanStack(addingSlot, existingSlot)) continue;

            // check if slot has place
            int maxStack = GetMaxStack(existingSlot.ItemId);
            int freeValue = maxStack - existingSlot.Count;
            if (freeValue <= 0) continue;

            int movingValue = Math.Min(freeValue, addingSlot.Count);
            existingSlot.Count += movingValue;
            addingSlot.Count -= movingValue;
            SetSlot(i, existingSlot);
        }

        // If needed, add to empty slots

        for (int i = 0; i < slots.Length && addingSlot.Count > 0; i++)
        {
            InventorySlot existingSlot = slots[i];
            if (!existingSlot.IsEmpty) continue;
            int maxStack = GetMaxStack(addingSlot.ItemId);
            int movingValue = Math.Min(maxStack, addingSlot.Count);
            SetSlot(i, new InventorySlot(addingSlot.ItemId, movingValue));
            addingSlot.Count -= movingValue;
        }

        return addingSlot.Count == 0;
    }

    public bool Move(int fromIndex, int toIndex, bool mergeStacks = true)
    {
        if (fromIndex == toIndex) return false;
        InventorySlot fromSlot = slots[fromIndex];
        InventorySlot toSlot = slots[toIndex];
        if (fromSlot.IsEmpty) return false;

        if (mergeStacks && CanStack(fromSlot, toSlot))
        {
            // if can, stack
            int maxStack = GetMaxStack(toSlot.ItemId);
            int freeValue = maxStack - toSlot.Count;
            if (freeValue > 0)
            {
                int movingValue = Math.Min(freeValue, fromSlot.Count);
                toSlot.Count += movingValue;
                fromSlot.Count -= movingValue;
                SetSlot(toIndex, toSlot);
                SetSlot(fromIndex, fromSlot.Count > 0 ? fromSlot : default);
                return true;
            }
        }  

        // else swap
        SetSlot(toIndex, fromSlot);
        SetSlot(fromIndex, toSlot);
        return true;
    }
    public bool Drop(int index, int amount = int.MaxValue)
    {
        InventorySlot slot = slots[index];
        if (slot.IsEmpty) return false;
        if (amount >= slot.Count)
        {
            SetSlot(index, default);
            return true;
        }
        else
        {
            slot.Count -= amount;
            SetSlot(index, slot);
            return true;
        }
    }

    public bool Split(int from, int to, int amount)
    {
        return false;
    }

    public bool Use(int index, IInventoryItemUseHandler handler)
    {
        InventorySlot slot = slots[index];
        if (slot.IsEmpty) return false;
        InventoryItemSO item = db.GetItemById(slot.ItemId);
        if (item == null) return false;

        bool used = handler.HandleUse(item);
        if (used)
        {
            if (item.Stackable)
            {
                slot.Count -= 1;
                SetSlot(index, slot.Count > 0 ? slot : default);
            }
            else
            {
                SetSlot(index, default);
            }
        }

        return used;
    }
    /// <summary>
    /// Sort inventory by some key
    /// </summary>
    /// <param name="selector">Example: x => x.itemId </param>
    /// <param name="ascending"></param>
    public void SortBy(Func<InventoryItemSO, object> selector, bool ascending = true)
    {
        List<InventorySlot> sorted = new List<InventorySlot>(slots);

        sorted.Sort((x, y) =>
        {
            InventoryItemSO itemX = x.IsEmpty ? null : db.GetItemById(x.ItemId);
            InventoryItemSO itemY = y.IsEmpty ? null : db.GetItemById(y.ItemId);
            if (itemX == null && itemY == null) return 0;
            if (itemX == null) return 1;
            if (itemY == null) return -1;

            int compare = Comparer.Default.Compare(selector(itemX), selector(itemY));
            return ascending ? compare : -compare;
        });

        for (int i = 0; i < slots.Length; i++)
        {
            SetSlot(i, sorted[i]);
        }
    }
    public void SortBy(Func<InventorySlot, object> selector, bool ascending = true)
    {
        List<InventorySlot> sorted = new List<InventorySlot>(slots);

        sorted.Sort((x, y) =>
        {
            InventoryItemSO itemX = x.IsEmpty ? null : db.GetItemById(x.ItemId);
            InventoryItemSO itemY = y.IsEmpty ? null : db.GetItemById(y.ItemId);
            if (itemX == null && itemY == null) return 0;
            if (itemX == null) return 1;
            if (itemY == null) return -1;

            int compare = Comparer.Default.Compare(selector(x), selector(y));
            return ascending ? compare : -compare;
        });

        for (int i = 0; i < slots.Length; i++)
        {
            SetSlot(i, sorted[i]);
        }
    }



    public void Restore(InventorySlot[] saved)
    {
        for (int i = 0; i < saved.Length; i++)
        {
            SetSlot(i, saved[i]);
        }
    }


    #endregion

    #region private 

    private void SetSlot(int index, InventorySlot slot)
    {
        slots[index] = slot;
        OnSlotChanged?.Invoke(index, slot);
    }

    private bool CanStack(InventorySlot a, InventorySlot b)
    {
        if (a.IsEmpty || b.IsEmpty || a.ItemId != b.ItemId) return false;
        InventoryItemSO item = db.GetItemById(a.ItemId);
        return item != null && item.Stackable; 
    }
    
    private int GetMaxStack(string itemId)
    {
        InventoryItemSO item = db.GetItemById(itemId);
        return item != null && item.Stackable ? item.MaxStack : 1;
    }

    #endregion

}
