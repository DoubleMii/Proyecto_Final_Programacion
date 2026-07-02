using UnityEngine;

public class DiegeticUI : MonoBehaviour
{
    public Color safeColor = Color.green;
    public Color alertColor = Color.red;

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        EventManager.OnPlayerDetected += UpdateIndicator;
        UpdateIndicator(EventManager.IsPlayerDetected);
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDetected -= UpdateIndicator;
    }

    private void UpdateIndicator(bool isDetected)
    {
        if (rend != null)
        {
            rend.material.color = isDetected ? alertColor : safeColor;
        }
    }
}
