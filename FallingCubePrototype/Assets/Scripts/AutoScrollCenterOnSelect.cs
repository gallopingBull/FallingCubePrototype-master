using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScrollCenterOnSelect : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollSpeed = 8f;
    [SerializeField] private bool instantAtStart = true;

    private RectTransform content;
    private RectTransform viewport;
    private GameObject lastSelected;

    void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        content = scrollRect.content;
        viewport = scrollRect.viewport;
    }

    void LateUpdate()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(content))
            return;

        if (selected == lastSelected)
            return;

        lastSelected = selected;
        CenterOnSelected(selected.GetComponent<RectTransform>());
    }

    void CenterOnSelected(RectTransform target)
    {
        if (target == null) return;

        Canvas.ForceUpdateCanvases();

        float contentHeight = content.rect.height;
        float viewHeight = viewport.rect.height;

        // Convert both rects into viewport space
        Vector3[] contentCorners = new Vector3[4];
        Vector3[] itemCorners = new Vector3[4];
        content.GetWorldCorners(contentCorners);
        target.GetWorldCorners(itemCorners);

        float itemCenterY = (itemCorners[0].y + itemCorners[1].y);

        // Midpoint of the visible area
        float viewCenterY = (viewport.position.y);

        // Determine how far to move content to bring the button’s center to viewport center
        float worldOffset = itemCenterY - viewCenterY;

        float normalizedDelta = worldOffset / (contentHeight - viewHeight);
        float targetPos = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + normalizedDelta);

        if (instantAtStart && !Application.isPlaying)
        {
            scrollRect.verticalNormalizedPosition = targetPos;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(SmoothScroll(targetPos));
    }

   IEnumerator SmoothScroll(float target)
    {
        while (!Mathf.Approximately(scrollRect.verticalNormalizedPosition, target))
        {
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(
                scrollRect.verticalNormalizedPosition,
                target,
                Time.deltaTime * scrollSpeed
            );
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = target;
    }
}
