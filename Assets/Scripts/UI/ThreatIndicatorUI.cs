using UnityEngine;
using UnityEngine.UI;

public class ThreatIndicatorUI : MonoBehaviour
{
    private CanvasGroup _group;
    private Image _topBar;
    private Image _bottomBar;
    private Text _label;
    private bool _detected;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindAnyObjectByType<ThreatIndicatorUI>() != null)
            return;

        GameObject root = new GameObject("ThreatIndicatorUI");
        DontDestroyOnLoad(root);
        root.AddComponent<ThreatIndicatorUI>().BuildUI();
    }

    private void OnEnable()
    {
        EventManager.OnPlayerDetected += HandlePlayerDetected;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDetected -= HandlePlayerDetected;
    }

    private void Update()
    {
        if (_group == null) return;

        float target = _detected ? 1f : 0f;
        _group.alpha = Mathf.MoveTowards(_group.alpha, target, Time.unscaledDeltaTime * 5f);

        if (!_detected) return;

        float pulse = 0.35f + Mathf.Sin(Time.unscaledTime * 7f) * 0.18f;
        Color red = new Color(1f, 0f, 0f, pulse);
        _topBar.color = red;
        _bottomBar.color = red;

        if (_label != null)
        {
            _label.color = new Color(1f, 0.92f, 0.84f, 0.75f + Mathf.Sin(Time.unscaledTime * 8f) * 0.2f);
        }
    }

    private void HandlePlayerDetected(bool detected)
    {
        _detected = detected;

        if (detected)
            RuntimeAudioFeedback.PlayAlert();
    }

    private void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;

        _topBar = CreateBar("TopThreatBar", new Vector2(0.5f, 1f), new Vector2(1f, 0f), -18f);
        _bottomBar = CreateBar("BottomThreatBar", new Vector2(0.5f, 0f), new Vector2(1f, 0f), 18f);
        _label = CreateLabel();
    }

    private Image CreateBar(string name, Vector2 anchor, Vector2 size, float y)
    {
        GameObject bar = new GameObject(name);
        bar.transform.SetParent(transform, false);

        RectTransform rect = bar.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(0f, 18f);

        if (anchor.y > 0.5f)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
        }
        else
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
        }

        Image image = bar.AddComponent<Image>();
        image.raycastTarget = false;
        image.color = new Color(1f, 0f, 0f, 0f);
        return image;
    }

    private Text CreateLabel()
    {
        GameObject label = new GameObject("ThreatText");
        label.transform.SetParent(transform, false);

        RectTransform rect = label.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -42f);
        rect.sizeDelta = new Vector2(420f, 42f);

        Text text = label.AddComponent<Text>();
        text.text = "TE HAN VISTO";
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;

        return text;
    }
}
