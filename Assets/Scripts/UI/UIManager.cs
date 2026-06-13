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
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        
        // Si el juego está en el menú de victoria, no alteramos el HUD desde aquí
        if (GameManager.Instance.currentState == GameManager.GameState.Victory) return;

        bool isPaused = GameManager.Instance.currentState == GameManager.GameState.Paused;
        
        // Control del Menú de Pausa
        if (pauseMenu != null && pauseMenu.activeSelf != isPaused)
        {
            pauseMenu.SetActive(isPaused);
        }

        // MODIFICACIÓN: Control del HUD basándose en la pausa
        // Si está pausado (!isPaused será false), el HUD se apaga. Si se despausa, se enciende.
        if (hudPanel != null && hudPanel.activeSelf == isPaused)
        {
            hudPanel.SetActive(!isPaused);
        }
    }

    private void ShowVictoryMenu()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (victoryMenu != null) victoryMenu.SetActive(true);
        if (pauseMenu != null) pauseMenu.SetActive(false); // Por si acaso estaba abierto
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }
}