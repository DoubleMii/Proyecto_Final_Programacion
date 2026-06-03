using UnityEngine;

public class UIManager : MonoBehaviour
{
    //Referencias a los diferentes paneles del Canvas (Menú Principal, HUD, Pausa, etc.)

    private void Start()
    {
        // Ocultar/Mostrar paneles según el estado inicial
    }

    public void ShowPauseMenu()
    {
        // Lógica para mostrar menú de pausa
    }

    public void UpdateHUD(float health, int gold)
    {
        // Actualizar elementos visuales del HUD (barras de vida, textos)
    }

    //Conectar botones del Canvas a las funciones del PersistenceManager
}
