// GameSceneManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    [Header("Теги Объектов")]
    public string playerTag = "Player";
    public string ghostGirlTag = "GhostGirl"; 

    [Header("Настройки Сцен")]
    public string mainGameSceneName = "GameScene"; 

    private PlayerStats playerStats;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializePlayerStatsReference();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void InitializePlayerStatsReference()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null) {
            playerStats = playerObject.GetComponent<PlayerStats>();
            if (playerStats == null) Debug.LogError($"[GameSceneManager] PlayerStats НЕ НАЙДЕН на '{playerObject.name}'!");
        } else Debug.LogError($"[GameSceneManager] Игрок с тегом '{playerTag}' НЕ НАЙДЕН!");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainGameSceneName) 
        {
            Debug.Log("--------------------------------------------------");
            Debug.Log($"[GameSceneManager] ИГРОВАЯ СЦЕНА '{scene.name}' ЗАГРУЖЕНА.");
            Debug.Log("  ДАННЫЕ ИЗ BATTLEDATAHOLDER ПЕРЕД ОБРАБОТКОЙ:");
            Debug.Log($"    - Имя объекта волка для поиска: {BattleDataHolder.ActiveWolfGameObjectName}");
            Debug.Log($"    - Исход боя: {BattleDataHolder.BattleOutcome}");
            Debug.Log($"    - Позиция игрока сохранена: {BattleDataHolder.PlayerPositionSaved}, Значение: {BattleDataHolder.PlayerPositionBeforeBattle}");
            Debug.Log($"    - Позиция девочки сохранена: {BattleDataHolder.GhostGirlPositionSaved}, Значение: {BattleDataHolder.GhostGirlPositionBeforeBattle}");
            Debug.Log($"    - Индекс точки девочки сохранен: {BattleDataHolder.GhostGirlWaypointSaved}, Значение: {BattleDataHolder.GhostGirlCurrentWaypointIndex}");
            Debug.Log("--------------------------------------------------");

            PlayerController.isGamePaused = false; 
            Debug.Log("[GameSceneManager] PlayerController.isGamePaused установлен в false.");

            // Обновляем ссылку на playerStats на случай, если сцена была полностью перезагружена
            InitializePlayerStatsReference(); 

            GameObject playerObjectInstance = GameObject.FindGameObjectWithTag(playerTag); 
            if (playerObjectInstance != null) {
                if (BattleDataHolder.PlayerPositionSaved) {
                    playerObjectInstance.transform.position = BattleDataHolder.PlayerPositionBeforeBattle;
                    Debug.Log($"[GameSceneManager] Позиция игрока ВОССТАНОВЛЕНА на: {playerObjectInstance.transform.position}");
                }
            } else Debug.LogWarning($"[GameSceneManager] Игрок с тегом '{playerTag}' НЕ НАЙДЕН для восстановления позиции!");

            GameObject ghostGirlObject = GameObject.FindGameObjectWithTag(ghostGirlTag);
            if (ghostGirlObject != null) {
                if (BattleDataHolder.GhostGirlPositionSaved) {
                    ghostGirlObject.transform.position = BattleDataHolder.GhostGirlPositionBeforeBattle;
                    Debug.Log($"[GameSceneManager] Позиция девочки ВОССТАНОВЛЕНА на: {ghostGirlObject.transform.position}");
                }
                if (BattleDataHolder.GhostGirlWaypointSaved) {
                    GirlMovement girlMovement = ghostGirlObject.GetComponent<GirlMovement>();
                    if (girlMovement != null) {
                        girlMovement.SetCurrentWaypointIndex(BattleDataHolder.GhostGirlCurrentWaypointIndex);
                         Debug.Log($"[GameSceneManager] Индекс точки маршрута девочки ВОССТАНОВЛЕН на: {BattleDataHolder.GhostGirlCurrentWaypointIndex}");
                    } else Debug.LogWarning($"[GameSceneManager] Скрипт GirlMovement не найден на девочке '{ghostGirlObject.name}'.");
                }
            } else Debug.Log($"[GameSceneManager] Девочка-призрак с тегом '{ghostGirlTag}' не найдена (может быть нормально).");

            string outcome = BattleDataHolder.BattleOutcome;
            int damage = BattleDataHolder.DamageToPlayer;
            string activeWolfGOName = BattleDataHolder.ActiveWolfGameObjectName; 

            WolfEncounter wolfToHandle = null;
            if (!string.IsNullOrEmpty(activeWolfGOName)) {
                GameObject wolfGO = GameObject.Find(activeWolfGOName); 
                if (wolfGO != null) {
                    wolfToHandle = wolfGO.GetComponent<WolfEncounter>();
                    if (wolfToHandle == null) Debug.LogWarning($"[GameSceneManager] На объекте '{activeWolfGOName}' НЕТ скрипта WolfEncounter.");
                } else Debug.LogWarning($"[GameSceneManager] НЕ удалось найти GameObject волка с именем '{activeWolfGOName}'.");
            } else Debug.LogWarning("[GameSceneManager] Имя активного волка из BattleDataHolder пустое!");

            if (!string.IsNullOrEmpty(outcome)) {
                Debug.Log($"[GameSceneManager] Обработка исхода боя: {outcome}, Урон: {damage}");
                if (playerStats != null && (outcome == "Defeat" || outcome == "DefeatTimeOut") && damage > 0) {
                     playerStats.TakeDamage(damage);
                }
                // В любом случае (победа или поражение) вызываем ResolveEncounter, так как волк должен исчезнуть
                wolfToHandle?.ResolveEncounter();
            } else Debug.LogWarning("[GameSceneManager] Исход боя из BattleDataHolder пустой!");
            
            BattleDataHolder.ResetSessionData();

            if (playerStats != null && playerStats.CurrentHealth <= 0) {
                Debug.LogError("GAME OVER из GameSceneManager! Здоровье игрока <= 0.");
                PlayerController.isGamePaused = true; 
            }
        } else {
            Debug.Log($"[GameSceneManager] OnSceneLoaded вызвана для сцены: '{scene.name}', но ожидалась '{mainGameSceneName}'.");
        }
    }
}