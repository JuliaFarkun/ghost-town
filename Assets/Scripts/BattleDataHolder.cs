// BattleDataHolder.cs
using UnityEngine; // Нужен для Vector3

public static class BattleDataHolder
{
    // --- Данные, передаваемые ИЗ GameScene В BattleScene ---
    public static string CurrentWolfIdentifier { get; set; }    // Тип волка, с которым будет бой
    public static string ActiveWolfGameObjectName { get; set; } // Имя GameObject'а волка в GameScene

    // --- Данные, передаваемые ИЗ BattleScene ОБРАТНО В GameScene ---
    public static string BattleOutcome { get; set; } // Результат боя ("Victory", "Defeat", "ErrorNoWolf", и т.д.)
    public static int DamageToPlayer { get; set; }   // Урон, полученный игроком в бою

    // --- Данные о позициях, сохраняемые ПЕРЕД боем ---
    public static Vector3 PlayerPositionBeforeBattle { get; set; }
    public static Vector3 GhostGirlPositionBeforeBattle { get; set; } 
    public static int GhostGirlCurrentWaypointIndex { get; set; } 

    // --- Флаги для проверки, были ли данные о позициях сохранены ---
    public static bool PlayerPositionSaved { get; set; } = false;
    public static bool GhostGirlPositionSaved { get; set; } = false;
    public static bool GhostGirlWaypointSaved { get; set; } = false;

    // --- Флаг, указывающий, что мы ТОЛЬКО ЧТО вернулись из сцены боя ---
    // Этот флаг помогает GameSceneManager понять, нужно ли обрабатывать исход боя и восстанавливать позиции,
    // или это просто загрузка сцены (например, при "Продолжить" из главного меню).
    public static bool JustReturnedFromBattle { get; set; } = false; 

    /// <summary>
    /// Вызывается из WolfEncounter.cs перед загрузкой BattleScene.
    /// Сохраняет всю необходимую информацию для инициализации боя и для последующего восстановления.
    /// </summary>
    public static void PrepareDataForBattleScene(string wolfId, string wolfGOName, Vector3 playerPos, GameObject ghostGirl = null, GirlMovement girlMovement = null)
    {
        CurrentWolfIdentifier = wolfId;
        ActiveWolfGameObjectName = wolfGOName;
        
        PlayerPositionBeforeBattle = playerPos;
        PlayerPositionSaved = true;

        if (ghostGirl != null && girlMovement != null)
        {
            GhostGirlPositionBeforeBattle = ghostGirl.transform.position;
            GhostGirlPositionSaved = true;
            GhostGirlCurrentWaypointIndex = girlMovement.GetCurrentWaypointIndex();
            GhostGirlWaypointSaved = true;
        }
        else
        {
            GhostGirlPositionSaved = false;
            GhostGirlWaypointSaved = false;
            GhostGirlCurrentWaypointIndex = 0; // Сброс на всякий случай
        }

        // JustReturnedFromBattle здесь НЕ устанавливаем в true. Он установится после завершения боя.
        // BattleOutcome и DamageToPlayer тоже здесь не трогаем.

        Debug.Log($"[BattleDataHolder] Данные для боя ПОДГОТОВЛЕНЫ: WolfID='{CurrentWolfIdentifier}', WolfGOName='{ActiveWolfGameObjectName}', PlayerPosSaved='{PlayerPositionSaved}', GhostGirlPosSaved='{GhostGirlPositionSaved}', GhostGirlWPSaved='{GhostGirlWaypointSaved}'");
    }
    
    /// <summary>
    /// Вызывается из DynamicBattleSceneController.cs (или FinalBattleController.cs) перед возвратом в GameScene.
    /// Устанавливает результат боя и помечает, что мы только что из боя.
    /// </summary>
    public static void SetBattleOutcome(string outcome, int damage)
    {
        BattleOutcome = outcome;
        DamageToPlayer = damage;
        JustReturnedFromBattle = true; // Помечаем, что мы ТОЛЬКО ЧТО вернулись из боя
        Debug.Log($"[BattleDataHolder] Исход боя УСТАНОВЛЕН: Outcome='{outcome}', Damage='{damage}', JustReturnedFromBattle='{JustReturnedFromBattle}'");
    }

    /// <summary>
    /// Сбрасывает все временные данные сессии боя и сохраненные позиции.
    /// Вызывается из MainMenuController.StartNewGame() и из GameSceneManager.OnSceneLoaded() после использования данных.
    /// Также вызывается из FinalBattleController.GoToGameOverScene() для полного сброса.
    /// </summary>
    public static void ResetSessionData() 
    {
        CurrentWolfIdentifier = null;
        ActiveWolfGameObjectName = null;
        BattleOutcome = null;
        DamageToPlayer = 0;

        PlayerPositionSaved = false;
        // PlayerPositionBeforeBattle = Vector3.zero; // Можно обнулять, но флаг важнее

        GhostGirlPositionSaved = false;
        // GhostGirlPositionBeforeBattle = Vector3.zero;
        GhostGirlCurrentWaypointIndex = 0; 
        GhostGirlWaypointSaved = false;   

        JustReturnedFromBattle = false; // Важно сбросить этот флаг
        Debug.Log("[BattleDataHolder] Все данные сессии (информация о бое, позициях, флаг JustReturnedFromBattle) сброшены.");
    }
}