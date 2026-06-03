using System;
using System.Collections.Generic;
using UnityEngine;


/// Contenedor principal de todos los datos persistentes del juego.
/// Esta clase se serializa/deserializa a JSON mediante SaveSystem.
/// No hereda de MonoBehaviour — es un objeto de datos puro.

[Serializable]
public class GameData
{

    // JUGADOR
    
    public PlayerData player;

    
    // ESTADÍSTICAS
    
    public StatsData stats;

    
    // CONFIGURACIÓN
    
    public SettingsData settings;

    
    // METADATOS DEL GUARDADO
    
    public string saveTimestamp;   // Fecha y hora del guardado
    public string gameVersion;     // Versión del juego al guardar
    public int    saveSlot;        // Slot de guardado (0, 1, 2...)

    // Constructor: inicializa todos los sub-objetos con valores por defecto.
    // Se llama cuando se crea una partida nueva.
    public GameData()
    {
        player   = new PlayerData();
        stats    = new StatsData();
        settings = new SettingsData();

        saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        gameVersion   = "1.0.0";
        saveSlot      = 0;
    }
}


// DATOS DEL JUGADOR

[Serializable]
public class PlayerData
{
    // Posición en el mundo
    public float posX;
    public float posY;
    public float posZ;

    // Rotación (solo Y es suficiente para la mayoría de juegos)
    public float rotY;

    // Propiedad auxiliar para simplificar coordenadas en C#
    public Vector3 Position
    {
        get => new Vector3(posX, posY, posZ);
        set
        {
            posX = value.x;
            posY = value.y;
            posZ = value.z;
        }
    }

    // Estado vital
    public float health;        // Vida actual
    public float maxHealth;     // Vida máxima
    public float stamina;       // Stamina actual
    public float maxStamina;    // Stamina máxima

    // Progreso
    public int   level;         // Nivel del jugador
    public int   experience;    // Experiencia acumulada
    public int   gold;          // Moneda / recursos

    // Nombre del escenario donde estaba el jugador
    public string currentScene;

    // Inventario: lista de nombres de objetos que lleva
    public List<string> inventory;

    // Objetos equipados (ranura -> nombre del objeto)
    public string equippedWeapon;
    public string equippedArmor;

    public PlayerData()
    {
        posX          = 0f;
        posY          = 0f;
        posZ          = 0f;
        rotY          = 0f;
        health        = 100f;
        maxHealth     = 100f;
        stamina       = 100f;
        maxStamina    = 100f;
        level         = 1;
        experience    = 0;
        gold          = 0;
        currentScene  = "Scene_Main";
        inventory     = new List<string>();
        equippedWeapon = "";
        equippedArmor  = "";
    }
}


// ESTADÍSTICAS DE PARTIDA

[Serializable]
public class StatsData
{
    public float  totalPlayTimeSeconds; // Tiempo total jugado en segundos
    public int    enemiesKilled;        // Enemigos eliminados
    public int    deaths;               // Veces que ha muerto el jugador
    public int    itemsCollected;       // Objetos recogidos en total
    public int    saveCount;            // Cuántas veces se ha guardado

    public StatsData()
    {
        totalPlayTimeSeconds = 0f;
        enemiesKilled        = 0;
        deaths               = 0;
        itemsCollected       = 0;
        saveCount            = 0;
    }
}


// CONFIGURACIÓN DEL JUGADOR

[Serializable]
public class SettingsData
{
    // Audio
    public float masterVolume;  // 0.0 a 1.0
    public float musicVolume;
    public float sfxVolume;

    // Gráficos
    public int   qualityLevel;  // 0 = Low, 1 = Medium, 2 = High, 3 = Ultra
    public bool  fullscreen;
    public int   resolutionIndex;

    // Control
    public float mouseSensitivity;
    public bool  invertYAxis;

    public SettingsData()
    {
        masterVolume      = 1.0f;
        musicVolume       = 0.8f;
        sfxVolume         = 1.0f;
        qualityLevel      = 2;
        fullscreen        = true;
        resolutionIndex   = 0;
        mouseSensitivity  = 2.0f;
        invertYAxis       = false;
    }
}
