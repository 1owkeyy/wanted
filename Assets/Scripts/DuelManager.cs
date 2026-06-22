using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class DuelManager : MonoBehaviour
{
    public static DuelManager Instance { get; private set; }

    public enum DuelState { Inactive, Aiming, Resolving }
    public DuelState CurrentState { get; private set; } = DuelState.Inactive;

    [Header("Slow-Mo Settings")]
    [SerializeField] private float duelTimeScale = 0.15f;
    [SerializeField] private float normalTimeScale = 1f;

    [Header("Hit-Stop")]
    [SerializeField] private float hitStopDuration = 0.06f;

    [Header("Group Duel - Refund on Partial Kill")]
    [SerializeField] private float refundPercentOnPartialKill = 0.35f;

    private const float baseFixedDeltaTime = 0.02f;

    [Header("References")]
    [SerializeField] private ArcAimSystem arcAimSystem;
    [SerializeField] private DuelCameraEffects cameraEffects;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private PlayerRecoil playerRecoil;
    [SerializeField] private FallReaction playerFallReaction;
    [SerializeField] private ScreenFlash screenFlash;
    [SerializeField] private Transform playerTransform;

    [Header("Retry UI")]
    [SerializeField] private GameObject retryPromptUI;
    [SerializeField] private float retryPromptDelay = 0.5f;
    [SerializeField] private float enemyFireDelay = 0.35f;

    private List<EnemyTrigger> currentEnemies = new List<EnemyTrigger>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (retryPromptUI != null)
            retryPromptUI.SetActive(false);

        // Note: Time.timeScale is intentionally NOT set here anymore.
        // UIManager.Start() pauses time for the main menu and resumes it when gameplay begins.
        // DuelManager manages timeScale only during active duels.

        if (arcAimSystem != null)
            arcAimSystem.OnShotResolved += HandleShotResolved;
    }

    void OnDestroy()
    {
        if (arcAimSystem != null)
            arcAimSystem.OnShotResolved -= HandleShotResolved;
    }

    void Update()
    {
        if (retryPromptUI != null && retryPromptUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartLevel();
            }
        }
    }

    // ---------- Starting duels ----------

    public void StartDuel(EnemyTrigger enemy)
    {
        if (CurrentState != DuelState.Inactive) return;

        currentEnemies = new List<EnemyTrigger> { enemy };
        BeginCommonDuelSetup(enemy.TimerDuration, playApproach: true);

        arcAimSystem.BeginDuel(enemy, enemy.TimerDuration);
    }

    public void StartGroupDuel(List<EnemyTrigger> enemies, float sharedDuration)
    {
        if (CurrentState != DuelState.Inactive) return;

        currentEnemies = new List<EnemyTrigger>(enemies);
        // playApproach: false - GroupEncounterTrigger already moved enemies to coordinated
        // side-by-side positions before calling this; running independent approach here would
        // override that with each enemy's own uncoordinated circle-point calculation.
        BeginCommonDuelSetup(sharedDuration, playApproach: false);

        arcAimSystem.BeginGroupDuel(enemies, sharedDuration);
    }

    private void BeginCommonDuelSetup(float duration, bool playApproach)
    {
        CurrentState = DuelState.Aiming;
        SetTimeScale(duelTimeScale);

        if (cameraEffects != null)
            cameraEffects.OnDuelEnter();

        // Frame on all active enemies for group duels, or the single enemy for solo duels
        if (cameraFollow != null && currentEnemies.Count > 0)
        {
            if (currentEnemies.Count > 1)
            {
                List<Transform> enemyTransforms = new List<Transform>();
                foreach (var enemy in currentEnemies)
                    enemyTransforms.Add(enemy.transform);

                cameraFollow.BeginGroupDuelFraming(enemyTransforms);
            }
            else
            {
                cameraFollow.BeginDuelFraming(currentEnemies[0].transform);
            }
        }

        if (playApproach)
        {
            foreach (var enemy in currentEnemies)
            {
                EnemyApproach approach = enemy.Pivot != null ? enemy.Pivot.GetComponent<EnemyApproach>() : null;
                if (approach != null && playerTransform != null)
                {
                    approach.PlayApproach(playerTransform);
                }
            }
        }
    }

    // ---------- Handling shot results ----------

    private void HandleShotResolved(bool wasHit, EnemyTrigger hitEnemy, bool duelComplete)
    {
        if (CurrentState != DuelState.Aiming) return;
        CurrentState = DuelState.Resolving;

        if (wasHit)
        {
            currentEnemies.Remove(hitEnemy);
            hitEnemy.OnDefeated(); // notify GameState if this was the wanted guy

            if (duelComplete)
            {
                // Final enemy down - full win sequence
                StartCoroutine(HandleWinSequence(hitEnemy));
            }
            else
            {
                // One down, others remain - partial kill: play the fall, refund time, resume aiming
                StartCoroutine(HandlePartialKillSequence(hitEnemy));
            }
        }
        else
        {
            // Miss or timeout - loss sequence
            StartCoroutine(HandleLossSequence());
        }
    }

    private IEnumerator HandlePartialKillSequence(EnemyTrigger killedEnemy)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);

        if (cameraEffects != null)
            cameraEffects.OnDuelHit();

        if (screenFlash != null)
            screenFlash.PlayWhite();

        // Player's shot lands
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGunshot();

        if (playerRecoil != null && playerTransform != null)
        {
            Vector3 fireDir = (killedEnemy.transform.position - playerTransform.position);
            fireDir.y = 0f;
            playerRecoil.PlayRecoil(fireDir);
        }

        SetTimeScale(duelTimeScale); // resume into the still-active duel, not full normal speed

        FallReaction enemyFall = killedEnemy.Pivot != null ? killedEnemy.Pivot.GetComponent<FallReaction>() : null;
        if (enemyFall != null && playerTransform != null)
        {
            enemyFall.PlayFall(playerTransform, null);
        }
        else
        {
            killedEnemy.gameObject.SetActive(false);
        }

        // The "sigh of relief" - refund a chunk of the shared timer
        arcAimSystem.RefundTime(refundPercentOnPartialKill);

        CurrentState = DuelState.Aiming; // hand control back, duel continues against remaining enemies
    }

    private IEnumerator HandleWinSequence(EnemyTrigger finalEnemy)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);

        if (cameraEffects != null)
            cameraEffects.OnDuelHit();

        if (screenFlash != null)
            screenFlash.PlayWhite();

        // Player's shot lands
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGunshot();

        if (playerRecoil != null && playerTransform != null)
        {
            Vector3 fireDir = (finalEnemy.transform.position - playerTransform.position);
            fireDir.y = 0f;
            playerRecoil.PlayRecoil(fireDir);
        }

        SetTimeScale(normalTimeScale);

        FallReaction enemyFall = finalEnemy.Pivot != null ? finalEnemy.Pivot.GetComponent<FallReaction>() : null;
        if (enemyFall != null && playerTransform != null)
        {
            enemyFall.PlayFall(playerTransform, null);
        }
        else
        {
            finalEnemy.gameObject.SetActive(false);
        }

        if (cameraEffects != null)
            cameraEffects.OnDuelExit();

        if (cameraFollow != null)
            cameraFollow.EndDuelFraming();

        CurrentState = DuelState.Inactive;
        currentEnemies.Clear();
    }

    private IEnumerator HandleLossSequence()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);

        SetTimeScale(normalTimeScale);

        yield return new WaitForSecondsRealtime(enemyFireDelay);

        if (cameraEffects != null)
            cameraEffects.OnDuelHit();

        if (screenFlash != null)
            screenFlash.PlayRed();

        // Enemy's shot fires back
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGunshot();

        // Any remaining enemy can be the one that fires - pick the first still-active one
        EnemyTrigger shooter = currentEnemies.Count > 0 ? currentEnemies[0] : null;

        if (shooter != null && shooter.Pivot != null && playerTransform != null)
        {
            PlayerRecoil enemyRecoil = shooter.Pivot.GetComponent<PlayerRecoil>();
            if (enemyRecoil != null)
            {
                Vector3 fireDir = (playerTransform.position - shooter.transform.position);
                fireDir.y = 0f;
                enemyRecoil.PlayRecoil(fireDir);
            }
        }

        if (playerFallReaction != null && shooter != null)
        {
            playerFallReaction.PlayFall(shooter.transform, null);
        }

        yield return new WaitForSecondsRealtime(retryPromptDelay);

        SetTimeScale(0f);

        if (retryPromptUI != null)
            retryPromptUI.SetActive(true);

        // Note: CurrentState stays Resolving here intentionally - RestartLevel() via R key
        // is handled by checking retryPromptUI.activeSelf in Update(), not CurrentState,
        // since the duel is over but state cleanup happens via scene reload anyway.
    }

    private void RestartLevel()
    {
        GameState.ResetForNewRun();
        SetTimeScale(normalTimeScale);
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = baseFixedDeltaTime * Mathf.Max(scale, 0.0001f);
    }
}