///  <summary>
/// Defines how items are consumed or activated.
/// Encapsulates “use” behavior outside the model, making item usage extensible.
/// </summary>
public interface IInventoryItemUseHandler
{
    bool HandleUse(InventoryItemSO item);
}
