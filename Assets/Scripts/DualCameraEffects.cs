using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class DuelCameraEffects : MonoBehaviour
{
    [Header("Zoom")]
    [SerializeField] private float normalOrthoSize = 5f;
    [SerializeField] private float duelOrthoSize = 3f;
    [SerializeField] private float zoomSpeed = 8f; // higher = faster transition

    [Header("Vignette")]
    [SerializeField] private VolumeProfile volumeProfile;
    [SerializeField] private float normalVignetteIntensity = 0f;
    [SerializeField] private float duelVignetteIntensity = 0.4f;
    [SerializeField] private float vignetteSpeed = 8f;

    [Header("Shake")]
    [SerializeField] private float entryShakeStrength = 0.15f;
    [SerializeField] private float entryShakeDuration = 0.15f;
    [SerializeField] private float hitShakeStrength = 0.3f;
    [SerializeField] private float hitShakeDuration = 0.2f;

    [Header("Aiming Sway")]
    [SerializeField] private float swayAmount = 0.008f;
    [SerializeField] private float swaySpeed = 1.2f;

    private Camera cam;
    private Vignette vignette;

    private float targetOrthoSize;
    private float targetVignetteIntensity;

    private float shakeTimer;
    private float shakeStrength;

    private bool isAiming;

    void Awake()
    {
        cam = GetComponent<Camera>();
        targetOrthoSize = normalOrthoSize;
        targetVignetteIntensity = normalVignetteIntensity;

        if (volumeProfile != null)
        {
            volumeProfile.TryGet(out vignette);
        }
    }

    // Runs after CameraFollow's LateUpdate has positioned the camera,
    // so shake/sway offsets apply on top of the followed position, not a static one.
    // Ensure this script executes after CameraFollow in Script Execution Order if jitter appears.
    void LateUpdate()
    {
        // Smooth zoom toward target
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, Time.unscaledDeltaTime * zoomSpeed);

        // Smooth vignette toward target
        if (vignette != null)
        {
            float current = vignette.intensity.value;
            float next = Mathf.Lerp(current, targetVignetteIntensity, Time.unscaledDeltaTime * vignetteSpeed);
            vignette.intensity.Override(next);
        }

        ApplyPositionOffset();
    }

    private void ApplyPositionOffset()
    {
        Vector3 offset = Vector3.zero;

        // One-off shake (entry punch or hit punch), decays over its duration
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            float falloff = Mathf.Clamp01(shakeTimer / Mathf.Max(entryShakeDuration, 0.0001f));
            offset += new Vector3(
                (Random.value * 2f - 1f) * shakeStrength * falloff,
                0f,
                (Random.value * 2f - 1f) * shakeStrength * falloff
            );
        }
        // Continuous subtle sway while actively aiming
        else if (isAiming)
        {
            float swayX = (Mathf.PerlinNoise(Time.unscaledTime * swaySpeed, 0f) - 0.5f) * 2f * swayAmount;
            float swayZ = (Mathf.PerlinNoise(0f, Time.unscaledTime * swaySpeed) - 0.5f) * 2f * swayAmount;
            offset += new Vector3(swayX, 0f, swayZ);
        }

        // Apply as an additive offset on top of wherever CameraFollow positioned us this frame
        transform.position += offset;
    }

    // ----- Public calls, wired into DuelManager -----

    public void OnDuelEnter()
    {
        targetOrthoSize = duelOrthoSize;
        targetVignetteIntensity = duelVignetteIntensity;
        isAiming = true;
        TriggerShake(entryShakeStrength, entryShakeDuration);
    }

    public void OnDuelHit()
    {
        isAiming = false;
        TriggerShake(hitShakeStrength, hitShakeDuration);
    }

    public void OnDuelExit()
    {
        // Called when returning to normal gameplay (after win, after restart)
        targetOrthoSize = normalOrthoSize;
        targetVignetteIntensity = normalVignetteIntensity;
        isAiming = false;
    }

    private void TriggerShake(float strength, float duration)
    {
        shakeStrength = strength;
        shakeTimer = duration;
    }
}