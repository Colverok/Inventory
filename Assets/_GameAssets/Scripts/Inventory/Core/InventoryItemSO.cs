using UnityEngine;

public enum InventoryItemType
{
    Weapon,
    Potion,
    Quest,
    Material
}
/// <summary>
/// Describes an item used in the inventory system
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Item", fileName = "Item")]
public class InventoryItemSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [TextArea] [SerializeField] private string description;
    [SerializeField] private InventoryItemType type;
    [SerializeField] private bool stackable = true;
    [SerializeField] private int maxStack = 99;

    #region Properties

    public string Id { get => id; set => id = value; }
    public string DisplayName { get => displayName; set => displayName = value; }
    public Sprite Icon { get => icon; set => icon = value; }
    public string Description { get => description; set => description = value; }
    public InventoryItemType Type { get => type; set => type = value; }
    public bool Stackable { get => stackable; set => stackable = value; }
    public int MaxStack { get => maxStack; set => maxStack = value; }

    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            Id = System.Guid.NewGuid().ToString("N");
    }
#endif
}
