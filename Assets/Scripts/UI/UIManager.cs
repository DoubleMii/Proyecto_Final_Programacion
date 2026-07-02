using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject victoryMenu;
    public GameObject hudPanel;
    public Button btnContinuar;

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
        // Búsqueda recursiva que encuentra objetos aunque estén apagados
        if (pauseMenu == null) pauseMenu = FindChildRecursive(transform, "MenuPausa");
        if (victoryMenu == null) victoryMenu = FindChildRecursive(transform, "MenuVictoria");
        if (hudPanel == null) hudPanel = FindChildRecursive(transform, "HudJugador");

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (victoryMenu != null) victoryMenu.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);

        if (btnContinuar != null)
        {
            btnContinuar.onClick.AddListener(ResumeGame);
        }

        AutoWireButtons();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        
        bool isPaused = GameManager.Instance.currentState == GameManager.GameState.Paused;
        bool shouldShowHud = GameManager.Instance.currentState == GameManager.GameState.Playing;

        if (pauseMenu != null && pauseMenu.activeSelf != isPaused)
        {
            pauseMenu.SetActive(isPaused);

            if (isPaused)
                RuntimeAudioFeedback.PlayMenuOpen();
            else
                RuntimeAudioFeedback.PlayMenuClose();
        }

        if (hudPanel != null && hudPanel.activeSelf != shouldShowHud)
            hudPanel.SetActive(shouldShowHud);
    }

    private void ShowVictoryMenu()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (victoryMenu != null) victoryMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    public void RestartGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartCurrentScene();
    }

    public void QuitGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.QuitGame();
    }

    private void AutoWireButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.GetComponentInParent<SaveMenuController>(true) != null)
                continue;

            string buttonName = button.name.ToLowerInvariant();

            if (buttonName.Contains("continuar") || buttonName.Contains("resume"))
                button.onClick.AddListener(ResumeGame);
            else if (buttonName.Contains("reiniciar") || buttonName.Contains("restart") || buttonName.Contains("reset"))
                button.onClick.AddListener(RestartGame);
            else if (buttonName.Contains("salir") || buttonName.Contains("quit") || buttonName.Contains("exit"))
                button.onClick.AddListener(QuitGame);
        }
    }
}
