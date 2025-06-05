using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestPanelAnimator : MonoBehaviour
{
    public RectTransform topBorder;
    public RectTransform bottomBorder;
    public CanvasGroup topGroup;
    public CanvasGroup bottomGroup;
    public TextMeshProUGUI questText;
    public float animationDuration = 0.5f;
    public float borderOpenDistance = 100f;
    public float fadeDuration = 0.5f;

    private Vector2 topStartPos, bottomStartPos;
    private Vector2 topOpenPos, bottomOpenPos;

    void Awake()
    {
        topStartPos = topBorder.anchoredPosition;
        bottomStartPos = bottomBorder.anchoredPosition;
        topOpenPos = topStartPos + Vector2.up * borderOpenDistance;
        bottomOpenPos = bottomStartPos + Vector2.down * borderOpenDistance;

        if (questText != null)
            questText.gameObject.SetActive(false);

        // Start with borders fully transparent
        if (topGroup != null) topGroup.alpha = 0f;
        if (bottomGroup != null) bottomGroup.alpha = 0f;
    }

    public void PlayOpenAnimation(string text)
    {
        if (questText != null)
            questText.gameObject.SetActive(false);
        if (questText != null)
            questText.text = text;
        StartCoroutine(FadeInAndOpenBorders());
    }

    private IEnumerator FadeInAndOpenBorders()
    {
        // Fade in both borders
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            if (topGroup != null) topGroup.alpha = t;
            if (bottomGroup != null) bottomGroup.alpha = t;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        if (topGroup != null) topGroup.alpha = 1f;
        if (bottomGroup != null) bottomGroup.alpha = 1f;

        // Animate borders opening
        elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            topBorder.anchoredPosition = Vector2.Lerp(topStartPos, topOpenPos, t);
            bottomBorder.anchoredPosition = Vector2.Lerp(bottomStartPos, bottomOpenPos, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        topBorder.anchoredPosition = topOpenPos;
        bottomBorder.anchoredPosition = bottomOpenPos;

        if (questText != null)
            questText.gameObject.SetActive(true);
    }

    public IEnumerator PlayShrinkAnimation()
    {
        if (questText != null)
            questText.gameObject.SetActive(false);

        float elapsed = 0f;
        // Shrink borders back to center
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            topBorder.anchoredPosition = Vector2.Lerp(topOpenPos, topStartPos, t);
            bottomBorder.anchoredPosition = Vector2.Lerp(bottomOpenPos, bottomStartPos, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        topBorder.anchoredPosition = topStartPos;
        bottomBorder.anchoredPosition = bottomStartPos;

        // Fade out both borders
        elapsed = 0f;
        float startAlpha = 1f;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            float alpha = Mathf.Lerp(startAlpha, 0f, t);
            if (topGroup != null) topGroup.alpha = alpha;
            if (bottomGroup != null) bottomGroup.alpha = alpha;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        if (topGroup != null) topGroup.alpha = 0f;
        if (bottomGroup != null) bottomGroup.alpha = 0f;
    }

    public void ResetPanel()
    {
        topBorder.anchoredPosition = topStartPos;
        bottomBorder.anchoredPosition = bottomStartPos;
        if (questText != null)
            questText.gameObject.SetActive(false);
        if (topGroup != null) topGroup.alpha = 0f;
        if (bottomGroup != null) bottomGroup.alpha = 0f;
    }
}
