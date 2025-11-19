using UnityEngine;

public class InventorySelection
{
    private int selectedIndex;

    public int SelectedIndex { get => selectedIndex; set => selectedIndex = value; }
    public bool HasSelection => SelectedIndex >= 0;

    public void Select(int index)
    {
        SelectedIndex = index;
    }
    public void Clear()
    {
        SelectedIndex = -1;
    }
}
