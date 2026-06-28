using UnityEngine;

/// <summary>
/// Añade color visual al enemigo según su tipo en el Start,
/// para distinguirlos a primera vista sin necesidad de materiales separados.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class EnemyVisual : MonoBehaviour
{
    // Colores identificativos por tipo de enemigo
    private static readonly Color COLOR_GUARD    = new Color(0.20f, 0.45f, 0.90f); // Azul — centinela tranquilo
    private static readonly Color COLOR_PURSUER  = new Color(0.90f, 0.20f, 0.20f); // Rojo — perseguidor agresivo
    private static readonly Color COLOR_ROAMER   = new Color(0.20f, 0.75f, 0.30f); // Verde — rondador curioso
    private static readonly Color COLOR_WATCHER  = new Color(0.80f, 0.55f, 0.05f); // Naranja — observador agudo

    [Header("Tipo visual (debe coincidir con NPCController)")]
    [SerializeField] private eEnemyType _visualType = eEnemyType.Guard;

    private Renderer _renderer;
    private MaterialPropertyBlock _block;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _block    = new MaterialPropertyBlock();
    }

    private void Start()
    {
        ApplyColor();
    }

    private void ApplyColor()
    {
        Color col = _visualType switch
        {
            eEnemyType.Guard    => COLOR_GUARD,
            eEnemyType.Pursuer  => COLOR_PURSUER,
            eEnemyType.Roamer   => COLOR_ROAMER,
            eEnemyType.Watcher  => COLOR_WATCHER,
            _                   => Color.white
        };

        _renderer.GetPropertyBlock(_block);
        _block.SetColor("_Color", col);
        _renderer.SetPropertyBlock(_block);
        // Usamos MaterialPropertyBlock para no modificar el material compartido
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Actualiza el color en el editor al cambiar el tipo
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_block == null)    _block    = new MaterialPropertyBlock();
        ApplyColor();
    }
#endif
}
