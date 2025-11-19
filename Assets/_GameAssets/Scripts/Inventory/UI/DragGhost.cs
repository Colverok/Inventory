using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual indicator shown during drag 
/// Follows cursor in the canvas space and displays the dragged item icon.
/// Keeps temporary drag state (source index, payload) until drop is completed.
/// </summary>
public class DragGhost : MonoBehaviour
{
    [SerializeField] private Image ghostImage;
    [SerializeField] private Canvas canvas;

    public static int CurrentSourceIndex = -1;
    public static int CurrentHoverIndex = -1;

    public void Begin(int index, Sprite sprite, Vector2 screenPos)
    {
        ghostImage.sprite = sprite;
        ghostImage.enabled = sprite != null;
        CurrentSourceIndex = index;

        Move(screenPos);
        gameObject.SetActive(true);
    }

    public void Move(Vector2 screenPos)
    {
        RectTransform rect = transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, 
            canvas.worldCamera, out var local);
        (transform as RectTransform).anchoredPosition = local;

    }

    public void End()
    {
        ghostImage.enabled = false;
        gameObject.SetActive(false);
        CurrentSourceIndex = -1;
    }
}
