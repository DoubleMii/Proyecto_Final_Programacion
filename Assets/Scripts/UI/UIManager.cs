using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject victoryMenu;
    public GameObject hudPanel;
    public Button btnContinuar;
    public TextMeshProUGUI healthText;
    public Slider healthSlider;

    private PlayerPersistence _player;
    private RectTransform _healthRoot;
    private float _lastHealth = -1f;
    private float _lastMaxHealth = -1f;

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
        FindHealthUI();

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (victoryMenu != null) victoryMenu.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);

        if (btnContinuar != null)
        {
            btnContinuar.onClick.AddListener(ResumeGame);
        }

        AutoWireButtons();
        UpdateHealthUI(true);
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

        SetHealthVisible(shouldShowHud);
        UpdateHealthUI();
    }

    private void ShowVictoryMenu()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        SetHealthVisible(false);
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

    private void FindHealthUI()
    {
        if (healthText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in texts)
            {
                string textName = text.name.ToLowerInvariant();
                if (textName.Contains("vida") || textName.Contains("health"))
                {
                    healthText = text;
                    break;
                }
            }
        }

        if (healthSlider == null)
        {
            Slider[] sliders = GetComponentsInChildren<Slider>(true);
            foreach (Slider slider in sliders)
            {
                string sliderName = slider.name.ToLowerInvariant();
                if (sliderName.Contains("vida") || sliderName.Contains("health"))
                {
                    healthSlider = slider;
                    break;
                }
            }
        }

        _healthRoot = FindHealthRoot();

        if (healthText == null)
            healthText = CreateHealthText();

        if (healthSlider == null)
            healthSlider = CreateHealthSlider();
    }

    private RectTransform FindHealthRoot()
    {
        if (healthText != null)
            return healthText.transform.parent as RectTransform;

        if (healthSlider != null)
            return healthSlider.transform.parent as RectTransform;

        Transform existingRoot = transform.Find("VidaHUDCanvas");
        if (existingRoot != null && existingRoot.TryGetComponent(out RectTransform existingRect))
            return existingRect;

        GameObject canvasObject = new GameObject("VidaHUDCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        RectTransform rect = canvasObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return rect;
    }

    private TextMeshProUGUI CreateHealthText()
    {
        GameObject textObject = new GameObject("VidaText", typeof(RectTransform));
        textObject.transform.SetParent(_healthRoot, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(24f, -24f);
        rect.sizeDelta = new Vector2(220f, 44f);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 28f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        text.raycastTarget = false;
        return text;
    }

    private Slider CreateHealthSlider()
    {
        GameObject sliderObject = new GameObject("VidaSlider", typeof(RectTransform));
        sliderObject.transform.SetParent(_healthRoot, false);

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 1f);
        sliderRect.anchorMax = new Vector2(0f, 1f);
        sliderRect.pivot = new Vector2(0f, 1f);
        sliderRect.anchoredPosition = new Vector2(24f, -68f);
        sliderRect.sizeDelta = new Vector2(220f, 18f);

        Image background = CreateBarImage("Fondo", sliderObject.transform, new Color(0.12f, 0.12f, 0.12f, 0.85f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2f, 2f);
        fillAreaRect.offsetMax = new Vector2(-2f, -2f);

        Image fill = CreateBarImage("Fill", fillArea.transform, new Color(0.82f, 0.08f, 0.08f, 1f));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;
        slider.targetGraphic = background;
        slider.fillRect = fillRect;

        return slider;
    }

    private Image CreateBarImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private void UpdateHealthUI(bool force = false)
    {
        if (_player == null)
            _player = FindAnyObjectByType<PlayerPersistence>();

        float health = _player != null ? _player.health : PersistenceManager.Instance?.CurrentData?.player.health ?? 100f;
        float maxHealth = _player != null ? _player.maxHealth : PersistenceManager.Instance?.CurrentData?.player.maxHealth ?? 100f;

        if (!force && Mathf.Approximately(health, _lastHealth) && Mathf.Approximately(maxHealth, _lastMaxHealth))
            return;

        _lastHealth = health;
        _lastMaxHealth = maxHealth;

        if (healthText != null)
            healthText.text = $"Vida: {Mathf.CeilToInt(health)} / {Mathf.CeilToInt(maxHealth)}";

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
    }

    private void SetHealthVisible(bool visible)
    {
        if (_healthRoot != null && _healthRoot.gameObject.activeSelf != visible)
        {
            _healthRoot.gameObject.SetActive(visible);

            if (visible)
                UpdateHealthUI(true);
        }
    }
}
