using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver, Victory }
    public GameState currentState { get; private set; }

    private bool _initialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("GameManager creado: " + gameObject.name);

        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        EventManager.OnPlayerDeath += HandleGameOver;
        EventManager.OnVictory += HandleVictory;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDeath -= HandleGameOver;
        EventManager.OnVictory -= HandleVictory;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        SetState(GameState.Playing);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing) PauseGame();
            else if (currentState == GameState.Paused) ResumeGame();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_initialized)
            _initialized = true;

        Time.timeScale = 1f;
        SetState(GameState.Playing);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EventManager.ResetPlayerDetection();

        Debug.Log("Scene cargada: " + scene.name);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log("Estado: " + currentState);
    }

    public void PauseGame()
    {
        SetState(GameState.Paused);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        SetState(GameState.Playing);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        ResetEnemiesToSpawn();

        if (PersistenceManager.Instance != null)
            PersistenceManager.Instance.NewGame();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetEnemiesToSpawn()
    {
        NPCController[] enemies = FindObjectsByType<NPCController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (NPCController enemy in enemies)
        {
            if (enemy != null)
                enemy.ResetToSpawnPosition();
        }
    }

    public void QuitGame()
    {
        if (PersistenceManager.Instance != null)
            PersistenceManager.Instance.TrySave(false);

        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HandleGameOver()
    {
        if (currentState == GameState.GameOver) return;

        SetState(GameState.GameOver);
        RestartCurrentScene();
    }

    private void HandleVictory()
    {
        if (currentState == GameState.Victory) return;

        SetState(GameState.Victory);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}
