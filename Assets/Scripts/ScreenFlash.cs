using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFlash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image flashImage; // a full-screen stretched Image, starts at alpha 0

    [Header("Flash Settings")]
    [SerializeField] private float flashAlpha = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private Color lossColor = new Color(0.8f, 0.05f, 0.05f); // deep red

    void Awake()
    {
        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
    }

    public void PlayWhite()
    {
        Play(hitColor);
    }

    public void PlayRed()
    {
        Play(lossColor);
    }

    private void Play(Color color)
    {
        if (flashImage == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(color));
    }

    private IEnumerator FlashRoutine(Color color)
    {
        SetColor(color, flashAlpha);

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            SetColor(color, Mathf.Lerp(flashAlpha, 0f, t));
            yield return null;
        }

        SetColor(color, 0f);
    }

    private void SetColor(Color color, float alpha)
    {
        color.a = alpha;
        flashImage.color = color;
    }
}