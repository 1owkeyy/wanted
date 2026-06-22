using UnityEngine;
using UnityEngine.UI;

public class TimerBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArcAimSystem arcAimSystem;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private GameObject timerBarRoot; // parent object to show/hide (assign the TimerBar GameObject itself)

    void Start()
    {
        if (timerBarRoot != null)
            timerBarRoot.SetActive(false);
    }

    void Update()
    {
        if (arcAimSystem == null || timerSlider == null) return;

        bool shouldShow = arcAimSystem.IsActive;

        if (timerBarRoot != null && timerBarRoot.activeSelf != shouldShow)
            timerBarRoot.SetActive(shouldShow);

        if (shouldShow)
        {
            timerSlider.value = arcAimSystem.TimerProgress01;
        }
    }
}