using UnityEngine;

public class DiegeticUI : MonoBehaviour
{
    public Color safeColor = Color.green;
    public Color alertColor = Color.red;

    private int enemiesChasing = 0;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        EventManager.OnPlayerDetected += UpdateIndicator;
        if (rend != null) rend.material.color = safeColor;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDetected -= UpdateIndicator;
    }

    private void UpdateIndicator(bool isDetected)
    {
        if (isDetected) enemiesChasing++;
        else enemiesChasing = Mathf.Max(0, enemiesChasing - 1);

        if (rend != null)
        {
            rend.material.color = (enemiesChasing > 0) ? alertColor : safeColor;
        }
    }
}
