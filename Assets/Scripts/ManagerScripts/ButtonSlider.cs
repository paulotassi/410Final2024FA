using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonSlider : MonoBehaviour
{
    public RectTransform panel; 
    private CanvasGroup canvasGroup;
    public float slideSpeed = 5f;

    public Vector2 hiddenPosition;  
    public Vector2 visiblePosition; 

    private bool isVisible = false;

    void Start()
    {
        // Get or add a CanvasGroup for fading
        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        // Set panel to start hidden
        panel.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        isVisible = false; 
    }

    private void OnEnable()
    {
        // Get or add a CanvasGroup for fading
        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        // Set panel to start hidden
        panel.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        isVisible = false; 
    }
    public void TogglePanel()
    {
        if (panel == null)
        {
            Debug.LogError("Panel reference is null!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(SlidePanel(isVisible ? hiddenPosition : visiblePosition, isVisible ? 0f : 1f));
        isVisible = !isVisible;
    }

    IEnumerator SlidePanel(Vector2 targetPos, float targetAlpha)
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (Vector2.Distance(panel.anchoredPosition, targetPos) > 0.1f || Mathf.Abs(canvasGroup.alpha - targetAlpha) > 0.05f)
        {
            panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPos, Time.deltaTime * slideSpeed);
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * slideSpeed);
            yield return null;
        }

        // Snap to final position and alpha
        panel.anchoredPosition = targetPos;
        canvasGroup.alpha = targetAlpha;

        // Fully disable interactions when hidden
        if (targetAlpha == 0f)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
