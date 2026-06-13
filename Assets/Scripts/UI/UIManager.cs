using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject victoryMenu;
    public GameObject hudPanel;
    public Button btnContinuar;
    public Button btnReiniciar;

    private void OnEnable()
    {
        EventManager.OnVictory += ShowVictoryMenu;
    }

    private void OnDisable()
    {
        EventManager.OnVictory -= ShowVictoryMenu;
    }

    private GameObject FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child.gameObject;
            GameObject result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private void Start()
    {
        if (pauseMenu == null) pauseMenu = FindChildRecursive(transform, "MenuPausa");
        if (victoryMenu == null) victoryMenu = FindChildRecursive(transform, "MenuVictoria");
        if (hudPanel == null) hudPanel = FindChildRecursive(transform, "HudJugador");

        if (btnReiniciar == null) 
        {
            GameObject restartObj = FindChildRecursive(transform, "BtnReiniciar");
            if (restartObj != null) btnReiniciar = restartObj.GetComponent<Button>();
        }

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (victoryMenu != null) victoryMenu.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);

        if (btnContinuar != null)
        {
            btnContinuar.onClick.AddListener(ResumeGame);
        }

        if (btnReiniciar != null)
        {
            btnReiniciar.onClick.AddListener(RestartGame);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.currentState == GameManager.GameState.Victory) return;

        bool isPaused = GameManager.Instance.currentState == GameManager.GameState.Paused;
        
        if (pauseMenu != null && pauseMenu.activeSelf != isPaused)
        {
            pauseMenu.SetActive(isPaused);
        }

        if (hudPanel != null && hudPanel.activeSelf == isPaused)
        {
            hudPanel.SetActive(!isPaused);
        }
    }

    private void ShowVictoryMenu()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (victoryMenu != null) victoryMenu.SetActive(true);
        if (pauseMenu != null) pauseMenu.SetActive(false); 
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
