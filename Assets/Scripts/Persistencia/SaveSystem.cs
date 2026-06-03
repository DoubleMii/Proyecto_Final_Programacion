using System;
using System.IO;
using UnityEngine;


// Sistema de guardado y carga de datos mediante JSON.
// Clase estática — se usa directamente sin instanciar.
// Guarda los archivos en Application.persistentDataPath (funciona en build y editor).

public static class SaveSystem
{
    private const string FILE_PREFIX  = "save_slot_";
    private const string FILE_EXT     = ".json";
    private const int    MAX_SLOTS    = 3;

    
    // RUTA DEL ARCHIVO
    

    //Devuelve la ruta completa del archivo de guardado para un slot.
    public static string GetSavePath(int slot = 0)
    {
        slot = Mathf.Clamp(slot, 0, MAX_SLOTS - 1);
        return Path.Combine(Application.persistentDataPath, FILE_PREFIX + slot + FILE_EXT);
    }

   
    // GUARDAR
    

    
    // Serializa GameData a JSON y lo escribe en disco.
    // Devuelve true si el guardado fue exitoso.
    
    public static bool Save(GameData data, int slot = 0)
    {
        try
        {
            // Actualizar metadatos antes de guardar
            data.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.saveSlot      = slot;
            data.stats.saveCount++;

            string json = JsonUtility.ToJson(data, prettyPrint: true);
            string path = GetSavePath(slot);

            File.WriteAllText(path, json);

            Debug.Log($"[SaveSystem] Guardado exitoso en slot {slot}:\n{path}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Error al guardar en slot {slot}: {e.Message}");
            return false;
        }
    }

    
    // CARGAR
    

    
    //Lee el JSON del disco y devuelve un GameData deserializado.
    // Devuelve null si no existe el archivo o hay error.
    
    public static GameData Load(int slot = 0)
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] No existe guardado en slot {slot}: {path}");
            return null;
        }

        try
        {
            string   json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);

            Debug.Log($"[SaveSystem] Carga exitosa del slot {slot} — guardado el {data.saveTimestamp}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Error al cargar slot {slot}: {e.Message}");
            return null;
        }
    }

    
    // UTILIDADES
    

    //Comprueba si existe un archivo de guardado en el slot indicado.
    public static bool SaveExists(int slot = 0)
    {
        return File.Exists(GetSavePath(slot));
    }

    //Elimina el archivo de guardado de un slot.
    public static bool DeleteSave(int slot = 0)
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] No hay guardado que eliminar en slot {slot}");
            return false;
        }

        try
        {
            File.Delete(path);
            Debug.Log($"[SaveSystem] Slot {slot} eliminado.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Error al eliminar slot {slot}: {e.Message}");
            return false;
        }
    }

    //Devuelve cuántos slots tienen guardado activo.
    public static int GetActiveSaveCount()
    {
        int count = 0;
        for (int i = 0; i < MAX_SLOTS; i++)
            if (SaveExists(i)) count++;
        return count;
    }
}
