using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI representation of a single inventory slot.
/// Displays icon, stack count and handles drag & drop interaction.
/// </summary>
public class InventorySlotView : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject countObject;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button dropButton;


    private int index;
    private InventoryController controller;
    private InventorySlot _slot;
    private DragGhost ghost;
    private TooltipView tooltip;

    private float lastClickTime;
    private const float doubleClickDelay = 0.25f;

    #region public 
    public void Init(int index, InventoryController controller, DragGhost ghost, TooltipView tooltip)
    {
        this.index = index;
        this.controller = controller;
        this.ghost = ghost;
        this.tooltip = tooltip;
        if (dropButton && controller)
            dropButton.onClick.AddListener(() => controller.RequestDrop(index));
    }

    public void SetData(InventorySlot slot)
    {
        _slot = slot;
        if (_slot.IsEmpty)
        {
            iconImage.enabled = false;
            countObject.SetActive(false);
            dropButton.gameObject.SetActive(false);
        }
        else
        {
            InventoryItemSO item = controller.GetItemById(slot.ItemId);
            iconImage.enabled = true;
            iconImage.sprite = item.Icon;
            bool showCount = item != null && item.Stackable && slot.Count > 1;
            countObject.SetActive(showCount);
            dropButton.gameObject.SetActive(true);
            if (showCount) countText.text = slot.Count.ToString();
        }
    }
    #endregion

    #region draggable
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_slot.IsEmpty) return;
        InventoryItemSO item = controller.GetItemById(_slot.ItemId);
        ghost.Begin(index, iconImage.sprite, eventData.position);
        //HideTooltip();

    }

    public void OnDrag(PointerEventData eventData)
    {
        ghost.Move(eventData.position);
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
        float t = Time.unscaledTime;
        if (t - lastClickTime < doubleClickDelay)
        {
            controller.RequestUse(index);
        }
        lastClickTime = t;
    }

    #endregion


    #region hoverable

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_slot.IsEmpty) return;
        if (DragGhost.CurrentSourceIndex != -1) return;
        InventoryItemSO item = controller.GetItemById(_slot.ItemId);
        tooltip.Show(item, _slot.Count, transform as RectTransform);
        DragGhost.CurrentHoverIndex = index;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }



    #endregion

    private void HideTooltip()
    {

        tooltip.Hide();
        if (DragGhost.CurrentHoverIndex == index) DragGhost.CurrentHoverIndex = -1;
    }
}
