using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach this to a Canvas (or any parent object with UI Selectables).
/// It ensures that when the mouse hovers over a UI element (Button, Toggle, etc.),
/// the EventSystem selection updates to that element.
/// 
/// This prevents situations where a gamepad-selected element remains "active"
/// while the mouse visually highlights a different one, leading to two buttons
/// looking active at once.
/// </summary>
public class UIHoverSelectionHandler : MonoBehaviour
{
    private void Awake()
    {
        // Attach MouseHoverSelectable to all child Selectables
        var selectables = GetComponentsInChildren<Selectable>(true);
        foreach (var sel in selectables)
        {
            if (sel.GetComponent<MouseHoverSelectable>() == null)
            {
                sel.gameObject.AddComponent<MouseHoverSelectable>();
            }
        }
    }
}

/// <summary>
/// Component added to each Selectable.
/// Handles pointer enter events and updates the EventSystem selection
/// so the hovered object becomes the currently selected one.
/// </summary>
public class MouseHoverSelectable : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current == null) return;

        var sel = GetComponent<Selectable>();
        if (sel != null)
        {
            // Only update if it's not already the selected object
            if (EventSystem.current.currentSelectedGameObject != sel.gameObject)
            {
                // Clear selection first to ensure Unity refreshes properly
                EventSystem.current.SetSelectedGameObject(null);
                sel.Select();
            }
        }
    }
}
