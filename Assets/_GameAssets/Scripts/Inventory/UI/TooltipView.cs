using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        Hide();
        canvasGroup.blocksRaycasts = false;
    }
    public void Show(InventoryItemSO item, int count, RectTransform anchor)
    {
        title.text = count > 1 ? $"{item.DisplayName}x{count}" : item.DisplayName;
        description.text = item.Description;
        type.text = item.Type.ToString();


        RectTransform rect = transform as RectTransform;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, anchor.position);
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, 
            screenPos, canvas.worldCamera, out var localPoint);
        rect.SetParent(canvasRect, false);
        rect.anchoredPosition = localPoint + new Vector2(rect.rect.width * 0.5f, -rect.rect.height * 0.5f);
        //rect.position = anchor.position + new Vector3(rect.rect.width * 0.5f, -rect.rect.height * 0.5f, 0f);
        canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }
}
