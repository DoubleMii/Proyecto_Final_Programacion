using System;
using System.Collections.Generic;
using UnityEngine;

/// Contenedor principal de los datos persistentes del juego.
/// Esta clase se serializa y deserializa a JSON mediante SaveSystem.
[Serializable]
public class GameData
{
    public PlayerData player;
    public StatsData stats;

    public string saveTimestamp;
    public string gameVersion;
    public int saveSlot;
    public bool isManualSave;

    public GameData()
    {
        player = new PlayerData();
        stats = new StatsData();

        saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        gameVersion = "1.0.0";
        saveSlot = 0;
        isManualSave = false;
    }
}

[Serializable]
public class PlayerData
{
    public float posX;
    public float posY;
    public float posZ;

    public float rotY;
    public bool hasSavedPosition;

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

    public float health;
    public float maxHealth;

    public List<string> inventory;

    public PlayerData()
    {
        posX = 0f;
        posY = 0f;
        posZ = 0f;
        rotY = 0f;
        hasSavedPosition = false;
        health = 100f;
        maxHealth = 100f;
        inventory = new List<string>();
    }
}

[Serializable]
public class StatsData
{
    public float totalPlayTimeSeconds;
    public int enemiesKilled;
    public int deaths;
    public int itemsCollected;
    public int saveCount;

    public StatsData()
    {
        totalPlayTimeSeconds = 0f;
        enemiesKilled = 0;
        deaths = 0;
        itemsCollected = 0;
        saveCount = 0;
    }
}
