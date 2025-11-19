using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotInteractionHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private int index;
    private InventoryController controller;
    private InventorySlot slot;

    private DragGhost ghost;
    private TooltipView tooltip;
    private float lastClickTime;
    private const float doubleClickDelay = 0.25f;
    public void Init(int index, InventoryController controller, DragGhost ghost,
        TooltipView tooltip)
    {
        this.index = index;
        this.controller = controller;
        this.ghost = ghost;
        this.tooltip = tooltip;
    }

    public void SetData(InventorySlot slot) => this.slot = slot;

    #region draggable
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot.IsEmpty) return;

        InventoryItemSO item = controller.GetItemById(slot.ItemId);
        ghost.Begin(index, item.Icon, eventData.position);

    }

    public void OnDrag(PointerEventData eventData)
    {
        ghost.Move(eventData.position);
        HideTooltip();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ghost.End();
    }

    #endregion


    #region droppable
    public void OnDrop(PointerEventData eventData)
    {
        int dragSource = DragGhost.CurrentSourceIndex;
        if (dragSource < 0) return;
        bool merge = !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift);

        controller.RequestMove(dragSource, index, merge);
    }
    #endregion


    #region clickable

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slot.IsEmpty)
        {
            return;
        }
        float t = Time.unscaledTime;
        if (t - lastClickTime < doubleClickDelay)
        {
            controller.RequestUse(index);
        }
        else
        {
            controller.RequestSelect(index);
        }
        lastClickTime = t;
    }

    #endregion


    #region hoverable

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slot.IsEmpty) return;
        if (DragGhost.CurrentSourceIndex != -1) return;
        InventoryItemSO item = controller.GetItemById(slot.ItemId);
        tooltip.Show(item, slot.Count, transform as RectTransform);
        DragGhost.CurrentHoverIndex = index;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }
    private void HideTooltip()
    {
        tooltip.Hide();
        if (DragGhost.CurrentHoverIndex == index) DragGhost.CurrentHoverIndex = -1;
    }

    #endregion

}
