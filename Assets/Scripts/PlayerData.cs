// PlayerData.cs
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // 1. Положение игрока
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;

    // 2. Здоровье игрока
    public int currentPlayerHealth;
    public int maxPlayerHealth;

    // 3. Положение девочки
    public float ghostGirlPositionX;
    public float ghostGirlPositionY;
    public float ghostGirlPositionZ;

    // 4. Следующая контрольная точка девочки
    public int ghostGirlCurrentWaypointIndex;

    // Флаги, указывающие, были ли данные для определенных сущностей вообще сохранены
    public bool hasSavedPlayerState;
    public bool hasSavedGhostGirlState;

    // 5. Список идентификаторов побежденных волков
    public System.Collections.Generic.List<string> defeatedWolfIdentifiers;

    // Конструктор для создания данных для "Новой игры" (значения по умолчанию)
    public PlayerData(int playerMaxHp, Vector3 initialPlayerPos, Vector3 initialGirlPos, int initialGirlWaypoint)
    {
        Debug.Log($"[PlayerData] Конструктор вызван с initialPlayerPos: {initialPlayerPos}, initialGirlPos: {initialGirlPos}");
        maxPlayerHealth = playerMaxHp;
        currentPlayerHealth = maxPlayerHealth;
        SetPlayerPosition(initialPlayerPos);
        hasSavedPlayerState = true;

        SetGhostGirlPosition(initialGirlPos);
        ghostGirlCurrentWaypointIndex = initialGirlWaypoint;
        hasSavedGhostGirlState = true; // Предполагаем, что девочка есть на старте новой игры
        defeatedWolfIdentifiers = new System.Collections.Generic.List<string>(); // Инициализируем пустой список
        Debug.Log($"[PlayerData] PlayerData создан. PlayerPos: {GetPlayerPosition()}, GirlPos: {GetGhostGirlPosition()}");
    }

    public PlayerData() { 
        defeatedWolfIdentifiers = new System.Collections.Generic.List<string>(); // Инициализируем и для пустого конструктора
    } // Пустой конструктор для JsonUtility

    public void SetPlayerPosition(Vector3 pos)
    {
        playerPositionX = pos.x;
        playerPositionY = pos.y;
        playerPositionZ = pos.z;
    }

    public Vector3 GetPlayerPosition()
    {
        return new Vector3(playerPositionX, playerPositionY, playerPositionZ);
    }

    public void SetGhostGirlPosition(Vector3 pos)
    {
        ghostGirlPositionX = pos.x;
        ghostGirlPositionY = pos.y;
        ghostGirlPositionZ = pos.z;
    }

    public Vector3 GetGhostGirlPosition()
    {
        return new Vector3(ghostGirlPositionX, ghostGirlPositionY, ghostGirlPositionZ);
    }
}