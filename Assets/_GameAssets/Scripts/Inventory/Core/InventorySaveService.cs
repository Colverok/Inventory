using UnityEngine;

/// <summary>
/// Serializes and deserializes inventory data.
/// Current implementation uses JSON + PlayerPrefs.
/// </summary>
public static class InventorySaveService
{
    private const string Key = "INVENTORY";

    [System.Serializable]
    private struct SaveData
    {
        public int rows;
        public int cols;
        public InventorySlot[] slots;

        public SaveData(int rows, int cols, InventorySlot[] slots)
        {
            this.rows = rows;
            this.cols = cols;
            this.slots = slots;
        }
    }

    public static void Save(InventoryModel model)
    {
        SaveData data = new SaveData(model.Rows, model.Cols, model.Slots);
        var jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(Key, jsonData);
        PlayerPrefs.Save();
    }

    public static bool TryLoad(InventoryModel model)
    {
        if (!PlayerPrefs.HasKey(Key)) return false;
        var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key));
        if (data.slots == null) return false;
        model.Restore(data.slots);
        return true;
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(Key);
    }
}
