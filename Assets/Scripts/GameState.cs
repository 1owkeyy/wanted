using UnityEngine;

// Persistent singleton that tracks cross-scene game state.
// Attach to an empty GameObject named "GameState" in your Main scene.
// DontDestroyOnLoad keeps it alive through scene reloads (level restarts),
// but since WANTED is a single-scene prototype, it's mainly used as a
// clean global flag store that any script can read/write without tight coupling.

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    // Set to true when the Wanted guy is defeated during a run.
    // Resets on scene reload (level restart) since the scene re-instantiates everything.
    public static bool WantedGuyDefeated { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public static void SetWantedGuyDefeated()
    {
        WantedGuyDefeated = true;
    }

    // Called on scene reload to reset flags for a fresh run
    public static void ResetForNewRun()
    {
        WantedGuyDefeated = false;
    }
}