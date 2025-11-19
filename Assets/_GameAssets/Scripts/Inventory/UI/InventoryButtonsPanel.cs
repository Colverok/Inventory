using UnityEngine;
using UnityEngine.UI;

public class InventoryButtonsPanel : MonoBehaviour
{
    [SerializeField] private Button sortByNameButton;
    [SerializeField] private Button sortByTypeButton;
    [SerializeField] private Button sortByCountButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private Button useButton;

    private bool nameAsc = false;
    private bool typeAsc = false;
    private bool countAsc = false;

    private InventoryController _controller;
    public void Init(InventoryController controller)
    {
        _controller = controller;
        if (_controller)
        {
            if (sortByNameButton) sortByNameButton.onClick.AddListener(() => SortByName());
            if (sortByTypeButton) sortByTypeButton.onClick.AddListener(() => SortByType());
            if (sortByCountButton) sortByCountButton.onClick.AddListener(() => SortByCount());
            if (dropButton) dropButton.onClick.AddListener(() => OnDrop());
            if (useButton) useButton.onClick.AddListener(() => OnUse());
        }
    }

    private void SortByName()
    {
        nameAsc = !nameAsc;
        _controller.SortByName(nameAsc);
    }
    private void SortByType()
    {
        typeAsc = !typeAsc;
        _controller.SortByType(typeAsc);
    }

    private void SortByCount()
    {
        countAsc = !countAsc;
        _controller.SortByCount(countAsc);
    }

    private void OnDrop()
    {
        if (!_controller.InventorySelection.HasSelection) return;
        _controller.RequestDrop(_controller.InventorySelection.SelectedIndex);
        _controller.InventorySelection.Clear();
    }

    private void OnUse()
    {
        if (!_controller.InventorySelection.HasSelection) return;
        _controller.RequestUse(_controller.InventorySelection.SelectedIndex);
    }

}
