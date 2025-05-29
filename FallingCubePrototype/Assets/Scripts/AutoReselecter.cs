using UnityEngine;
using UnityEngine.EventSystems;

// https://discussions.unity.com/t/635261
// fixes issue where clicking away from a menu will cause future keyboard navigation to not work

public class AutoReselecter : MonoBehaviour
{

    [SerializeField] private EventSystem eventSystem;
    private GameObject lastSelectedObject;

    void Awake()
    {
        if (eventSystem == null)
            eventSystem = gameObject.GetComponent<EventSystem>();
    }

    void Update()
    {
        if (eventSystem.currentSelectedGameObject == null)
            eventSystem.SetSelectedGameObject(lastSelectedObject); // no current selection, go back to last selected
        else
            lastSelectedObject = eventSystem.currentSelectedGameObject; // keep setting current selected object
    }
}