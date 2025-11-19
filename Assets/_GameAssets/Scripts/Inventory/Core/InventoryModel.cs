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
    /// Adds items to the inventory, first trying to merge them into existing
    /// compatible stacks, and then placing any remaining items into empty slots.
    /// </summary>
    /// <param name="addingSlot">
    /// Slot containing the item ID and amount to add.  
    /// </param>
    /// <returns>
    /// True if all items were successfully placed into the inventory;  
    /// false if there was not enough free space or stack capacity.
    /// </returns>
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

    /// <summary>
    /// Moves items from one slot to another.
    /// Can merge stacks if the items are compatible. 
    /// </summary>
    /// <param name="fromIndex">Index of the source slot to move items from.</param>
    /// <param name="toIndex">Index of the target slot to move items to.</param>
    /// <param name="mergeStacks">If true, tries to merge item stacks when they have the same item and
    /// stacking is allowed; otherwise the slots are simply swapped.</param>
    /// <returns>True if the inventory state was changed; otherwise, false.</returns>
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

    /// <summary>
    /// Removes items from a slot.
    /// </summary>
    /// <param name="index">Index of the slot to remove items from.</param>
    /// <param name="amount">Maximum number of items to remove. If greater than or equal to the
    /// current stack size, the entire stack is cleared.</param>
    /// <returns>True if any items were removed; otherwise, false.</returns>
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

    /// <summary>
    /// Attempts to use the item in the given slot via the provided use handler.
    /// If the handler reports success, the item stack is decreased or the slot
    /// is cleared for non-stackable items.
    /// </summary>
    /// <param name="index">Index of the slot whose item should be used.</param>
    /// <param name="handler">Object responsible for handling item usage and reporting whether the
    /// item was successfully consumed or activated.</param>
    /// <returns>True if the item was successfully used by the handler; otherwise, false.</returns>
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
    /// Sorts slots in the inventory using a key selector 
    /// keeping empty slots at the end.
    /// </summary>
    /// <param name="selector">
    /// Function that selects a sort key from a slot
    /// (for example: <c>x => x.Count</c>).
    /// </param>
    /// <param name="ascending">
    /// If true, sorts in ascending order; if false, sorts in descending order.
    /// </param>
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
    
    /// <summary>
    /// Restores the inventory state from a previously saved array of slots.
    /// </summary>
    /// <param name="saved">
    /// Array of previously saved slots.
    /// </param>
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

    private bool CanStack(InventorySlot slotA, InventorySlot slotB)
    {
        if (slotA.IsEmpty || slotB.IsEmpty || slotA.ItemId != slotB.ItemId) return false;
        InventoryItemSO item = db.GetItemById(slotA.ItemId);
        return item != null && item.Stackable; 
    }
    
    private int GetMaxStack(string itemId)
    {
        InventoryItemSO item = db.GetItemById(itemId);
        return item != null && item.Stackable ? item.MaxStack : 1;
    }

    #endregion

}
