using UnityEngine;
using TMPro;

/// <summary>
/// Displays contextual information about an item when hovered.
/// </summary>
public class TooltipView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI type;
    [SerializeField] private Canvas canvas;
    private float offset = 0.5f;

    private void Awake()
    {
        Hide();
        canvasGroup.blocksRaycasts = false;
    }
    public void Show(InventoryItemSO item, int count, RectTransform anchor)
    {
        title.text = item.DisplayName;
        description.text = item.Description;
        type.text = item.Type.ToString();

        SetPosition(anchor);
    }

    private void SetPosition(RectTransform anchor)
    {
        RectTransform rect = transform as RectTransform;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, anchor.position);
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,
            screenPos, canvas.worldCamera, out var localPoint);
        rect.SetParent(canvasRect, false);
        rect.anchoredPosition = localPoint + new Vector2(rect.rect.width * offset, -rect.rect.height * offset);
        canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }
}
