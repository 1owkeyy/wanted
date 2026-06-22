using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Central UI controller for all panels.
// Attach to an empty GameObject in the scene alongside your Canvas.
// Wire up all panel references in the Inspector.

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject ftuePanel;
    [SerializeField] private GameObject winPanel;

    [Header("Win Panel Elements")]
    [SerializeField] private TextMeshProUGUI wantedStatusText;
    [SerializeField] private TextMeshProUGUI wantedCheckText; // the ☑ or ☒ symbol

    [Header("FTUE")]
    [SerializeField] private bool showFTUEOnFirstPlay = true;
    private static bool ftueShownThisSession = false;

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
        // Always start with all panels hidden except main menu on first load
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (ftuePanel != null) ftuePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // Always unlock cursor on scene load — main menu needs it free.
        // Gets locked again in StartGameplay() after FTUE is dismissed.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pause gameplay while main menu is showing
        Time.timeScale = 0f;
    }

    // Called by Play button on main menu
    public void OnPlayPressed()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        // Revolver cock + BGM chain — the "game is starting" audio moment
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDrawSequence();

        if (showFTUEOnFirstPlay && !ftueShownThisSession)
        {
            if (ftuePanel != null) ftuePanel.SetActive(true);
        }
        else
        {
            StartGameplay();
        }
    }

    // Called by clicking anywhere on the FTUE panel
    public void OnFTUEDismissed()
    {
        ftueShownThisSession = true;
        if (ftuePanel != null) ftuePanel.SetActive(false);
        StartGameplay();
    }

    private void StartGameplay()
    {
        Time.timeScale = 1f;
        // Unlock cursor for gameplay (it gets locked by ArcAimSystem on duel, but needs to be free for exploration)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowWinScreen()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Update wanted guy status based on GameState flag
        bool wantedDefeated = GameState.WantedGuyDefeated;

        if (wantedCheckText != null)
            wantedCheckText.text = wantedDefeated ? "☑" : "☒";

        if (wantedStatusText != null)
        {
            wantedStatusText.text = wantedDefeated
                ? "Billy \"Quick Hands\" Mercer — Eliminated"
                : "Billy \"Quick Hands\" Mercer — Still at Large";

            // Green tint if eliminated, red/faded if escaped
            wantedStatusText.color = wantedDefeated
                ? new Color(0.6f, 0.9f, 0.6f)
                : new Color(0.9f, 0.5f, 0.5f);
        }

        if (winPanel != null) winPanel.SetActive(true);
    }

    // Called by Play Again button on win screen
    public void OnPlayAgainPressed()
    {
        GameState.ResetForNewRun();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Called by Main Menu button on win screen
    public void OnMainMenuPressed()
    {
        GameState.ResetForNewRun();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Scene reloads, Start() will show main menu again automatically
    }
}