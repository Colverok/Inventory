using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and updates the visual representation of the inventory grid.
/// Instantiates slot views, binds them to controller actions and updates UI states. 
/// </summary>
public class InventoryView : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private InventorySlotView slotPrefab;
    [SerializeField] private DragGhost dragGhost;
    [SerializeField] private TooltipView tooltip;
    [SerializeField] private Button sortByNameButton;
    [SerializeField] private Button sortByTypeButton;
    [SerializeField] private Button sortByCountButton;

    private InventoryController _controller;
    private readonly List<InventorySlotView> slots = new List<InventorySlotView>();

    public void Init(InventoryController controller)
    {
        _controller = controller;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = controller.InventoryModel.Cols;
        for (int i = 0; i < controller.InventoryModel.Slots.Length; i++)
        {
            InventorySlotView slot = Instantiate(slotPrefab, grid.transform);
            slot.Init(i, _controller, dragGhost, tooltip);
            slots.Add(slot);
            UpdateSlot(i, controller.InventoryModel.GetSlot(i));
        }
        if (_controller)
        {
            if (sortByNameButton) sortByNameButton.onClick.AddListener(() => _controller.SortByName());
            if (sortByTypeButton) sortByTypeButton.onClick.AddListener(() => _controller.SortByType());
            if (sortByCountButton) sortByCountButton.onClick.AddListener(() => _controller.SortByCount());
        }
    }

    public void UpdateSlot(int index, InventorySlot slot)
    {
        slots[index].SetData(slot);
    }
}
