using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InventorySlotVisual : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject countObject;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image selectionFrame;

    private InventoryController controller;
    private int index;
    private bool isSelected;

    public void Init(InventoryController controller, int index)
    {
        this.controller = controller;
        this.index = index;

        controller.SelectionChanged += OnSelectionChanged;
    }

    private void OnDestroy()
    {
        if (controller != null)
            controller.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(int selectedIndex)
    {
        isSelected = index == selectedIndex;
        selectionFrame.gameObject.SetActive(isSelected);
    }

    public void UpdateVisuals(InventorySlot slot)
    {
        if (slot.IsEmpty)
        {
            iconImage.enabled = false;
            countObject.SetActive(false);
            selectionFrame.gameObject.SetActive(false);
        }
        else
        {
            InventoryItemSO item = controller.GetItemById(slot.ItemId);
            iconImage.enabled = true;
            iconImage.sprite = item.Icon;

            bool showCount = item.Stackable && slot.Count > 1;
            countObject.SetActive(showCount);
            if (showCount) countText.text = slot.Count.ToString();
        }
    }

}
