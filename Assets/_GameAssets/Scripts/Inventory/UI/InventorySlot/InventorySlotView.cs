using UnityEngine;

/// <summary>
/// Composite view of a single inventory slot.
/// Delegates rendering to InventorySlotVisual and input handling to InventorySlotInteractionHandler.
/// </summary>
public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private InventorySlotVisual visuals;
    [SerializeField] private InventorySlotInteractionHandler interactionHandler;
    public void Init(int index, InventoryController controller, DragGhost ghost, TooltipView tooltip)
    {
        visuals.Init(controller, index);
        interactionHandler.Init(index, controller, ghost, tooltip);
    }

    public void SetData(InventorySlot slot)
    {
        visuals.UpdateVisuals(slot);
        interactionHandler.SetData(slot);
    }
}
