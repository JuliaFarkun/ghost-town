// SaveLoadManager.cs
using UnityEngine;
using System.IO;

public static class SaveLoadManager
{
    private const string SAVE_FILE_NAME = "playerGameData.json";
    private static string saveFilePath;

    static SaveLoadManager()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        Debug.Log($"[SaveLoadManager] Путь для сохранения игры: {saveFilePath}");
    }

    public static void SaveGame(PlayerData dataToSave)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(dataToSave, true);
            File.WriteAllText(saveFilePath, jsonData);
            Debug.Log($"[SaveLoadManager] Игра успешно сохранена в: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Ошибка при сохранении игры: {e.Message}\n{e.StackTrace}");
        }
    }

    public static PlayerData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                PlayerData loadedData = JsonUtility.FromJson<PlayerData>(jsonData);
                Debug.Log($"[SaveLoadManager] Игра успешно загружена из: {saveFilePath}");
                return loadedData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveLoadManager] Ошибка при загрузке игры: {e.Message}\n{e.StackTrace}. Возвращаем null.");
                return null;
            }
        }
        else
        {
            Debug.LogWarning($"[SaveLoadManager] Файл сохранения не найден ({saveFilePath}). Возвращаем null.");
            return null;
        }
    }

    public static bool DoesSaveFileExist()
    {
        return File.Exists(saveFilePath);
    }

    public static void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                File.Delete(saveFilePath);
                Debug.Log($"[SaveLoadManager] Файл сохранения удален: {saveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveLoadManager] Ошибка при удалении файла сохранения: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}