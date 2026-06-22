using UnityEngine;
using System.Collections;

// Attach this to PlayerPivot (the empty parent at ground/feet level), not the capsule.
// Attach to PlayerPivot or EnemyPivot - a small "kick" lean used for both player recoil and enemy firing.
public class PlayerRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float recoilAngle = 18f;
    [SerializeField] private float recoilOutDuration = 0.07f;
    [SerializeField] private float recoilReturnDuration = 0.22f;

    private Coroutine activeRoutine;
    private PlayerController playerController; // optional - only present on the actual player

    void Awake()
    {
        playerController = GetComponent<PlayerController>(); // null on enemy, that's fine
    }

    // fireDirection = direction the shot was fired (player to enemy) - player leans away from this.
    public void PlayRecoil(Vector3 fireDirection)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(RecoilRoutine(fireDirection));
    }

    private IEnumerator RecoilRoutine(Vector3 fireDirection)
    {
        // Take rotation control away from PlayerController for the duration of the recoil,
        // otherwise its movement-facing logic overwrites our rotation every FixedUpdate.
        // (No-op on enemies, since they have no PlayerController.)
        if (playerController != null)
            playerController.RotationLocked = true;

        fireDirection.y = 0f;
        fireDirection.Normalize();

        Vector3 rotationAxis = Vector3.Cross(Vector3.up, fireDirection);

        Quaternion restRotation = transform.rotation;
        Quaternion leanRotation = Quaternion.AngleAxis(-recoilAngle, rotationAxis) * restRotation;

        float elapsed = 0f;
        while (elapsed < recoilOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / recoilOutDuration);
            transform.rotation = Quaternion.Slerp(restRotation, leanRotation, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < recoilReturnDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / recoilReturnDuration);
            transform.rotation = Quaternion.Slerp(leanRotation, restRotation, t);
            yield return null;
        }

        transform.rotation = restRotation;
        if (playerController != null)
            playerController.RotationLocked = false;
        activeRoutine = null;
    }
}