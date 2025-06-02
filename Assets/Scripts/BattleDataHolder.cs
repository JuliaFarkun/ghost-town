// BattleDataHolder.cs
using UnityEngine;

public static class BattleDataHolder
{
    public static string CurrentWolfIdentifier { get; set; }
    public static string ActiveWolfGameObjectName { get; set; } 

    public static string BattleOutcome { get; set; } 
    public static int DamageToPlayer { get; set; }

    public static Vector3 PlayerPositionBeforeBattle { get; set; }
    public static Vector3 GhostGirlPositionBeforeBattle { get; set; } 
    public static bool PlayerPositionSaved { get; set; } = false;
    public static bool GhostGirlPositionSaved { get; set; } = false;

    // ДОБАВЛЕНО для девочки-призрака
    public static int GhostGirlCurrentWaypointIndex { get; set; } 
    public static bool GhostGirlWaypointSaved { get; set; } = false;

    public static void ResetSessionData()
    {
        CurrentWolfIdentifier = null;
        ActiveWolfGameObjectName = null;
        BattleOutcome = null;
        DamageToPlayer = 0;
        PlayerPositionSaved = false;
        GhostGirlPositionSaved = false;
        GhostGirlCurrentWaypointIndex = 0; // Сброс на дефолтное значение
        GhostGirlWaypointSaved = false;    // Сброс флага
    }
}