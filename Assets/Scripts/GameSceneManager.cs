// GameSceneManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    [Header("Теги и Имена Объектов/Сцен")]
    public string playerTag = "Player";
    public string ghostGirlTag = "GhostGirl"; 
    public string mainGameSceneName = "GameScene"; 
    public string gameOverSceneName = "GameOverScene"; 

    [Header("Стартовые Позиции (для случая, если нет файла сохранения)")]
    public Vector3 defaultPlayerSpawnPosition = new Vector3(4.05f, -8.76f, -1.2f);
    public Vector3 defaultGhostGirlSpawnPosition = new Vector3(5.15f, -8.25f, 0f);
    public int defaultGhostGirlStartWaypoint = 0;
    public int defaultPlayerMaxHealth = 100;


    private PlayerStats playerStats;
    private GirlMovement girlMovement; // Ссылка на скрипт девочки
    private GameObject playerGameObject; // Ссылка на GameObject игрока

    // Синглтон для легкого доступа к методу сохранения из других скриптов (например, WolfEncounter)
    public static GameSceneManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Раскомментируй, если этот менеджер должен быть персистентным
                                          // Но тогда логика OnSceneLoaded должна быть более аккуратной
                                          // Пока предполагаем, что он на сцене GameScene и загружается с ней.
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainGameSceneName) 
        {
            Debug.Log($"[GameSceneManager] ИГРОВАЯ СЦЕНА '{scene.name}' ЗАГРУЖЕНА.");
            
            PlayerController.isGamePaused = false; 
            Time.timeScale = 1f;

            // Находим объекты и компоненты
            playerGameObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerGameObject != null) {
                playerStats = playerGameObject.GetComponent<PlayerStats>();
                if (playerStats == null) Debug.LogError($"[GSM] PlayerStats не найден на игроке '{playerGameObject.name}'!");
            } else Debug.LogError($"[GSM] Игрок с тегом '{playerTag}' не найден!");

            GameObject ghostGirlObject = GameObject.FindGameObjectWithTag(ghostGirlTag);
            if (ghostGirlObject != null) {
                girlMovement = ghostGirlObject.GetComponent<GirlMovement>();
                if (girlMovement == null) Debug.LogWarning($"[GSM] GirlMovement не найден на девочке '{ghostGirlObject.name}'.");
            } else Debug.Log($"[GSM] Девочка-призрак с тегом '{ghostGirlTag}' не найдена (может быть нормально).");


            // Загружаем данные или создаем новые, если файла нет
            PlayerData loadedData = SaveLoadManager.LoadGame();
            if (loadedData == null) // Файла нет или он поврежден
            {
                Debug.LogWarning("[GSM] Файл сохранения не найден или поврежден. Создание данных для новой игры.");
                Debug.Log($"[GSM] Используемые defaultPlayerSpawnPosition: {defaultPlayerSpawnPosition}");
                Debug.Log($"[GSM] Используемые defaultGhostGirlSpawnPosition: {defaultGhostGirlSpawnPosition}");
                loadedData = new PlayerData(
                    defaultPlayerMaxHealth, 
                    defaultPlayerSpawnPosition, 
                    defaultGhostGirlSpawnPosition, 
                    defaultGhostGirlStartWaypoint
                );
                Debug.Log($"[GSM] Созданы новые PlayerData с PlayerPos: {loadedData.GetPlayerPosition()} и GirlPos: {loadedData.GetGhostGirlPosition()}");
                // Сразу сохраняем эти "новые" данные, чтобы файл появился
                SaveLoadManager.SaveGame(loadedData);
            }
            else
            {
                Debug.Log($"[GSM] PlayerData УСПЕШНО ЗАГРУЖЕН из файла. PlayerPos: {loadedData.GetPlayerPosition()}, GirlPos: {loadedData.GetGhostGirlPosition()}");
            }
            
            // Применяем загруженные/новые данные
            Debug.Log($"[GSM] Перед применением данных к PlayerStats. PlayerData Pos: {loadedData.GetPlayerPosition()}");
            if (playerStats != null) playerStats.ApplySaveData(loadedData, defaultPlayerSpawnPosition);
            Debug.Log($"[GSM] Перед применением данных к GirlMovement. PlayerData Pos: {loadedData.GetGhostGirlPosition()}");
            if (girlMovement != null) girlMovement.ApplySaveData(loadedData, defaultGhostGirlSpawnPosition, defaultGhostGirlStartWaypoint);

            // Деактивируем уже побежденных волков
            if (loadedData.defeatedWolfIdentifiers != null && loadedData.defeatedWolfIdentifiers.Count > 0)
            {
                Debug.Log($"[GSM] Проверка побежденных волков. Список: {string.Join(", ", loadedData.defeatedWolfIdentifiers)}");
                WolfEncounter[] allWolvesOnScene = GameObject.FindObjectsByType<WolfEncounter>(FindObjectsSortMode.None);
                foreach (WolfEncounter wolf in allWolvesOnScene)
                {
                    if (loadedData.defeatedWolfIdentifiers.Contains(wolf.gameObject.name))
                    {
                        Debug.Log($"[GSM] Волк '{wolf.gameObject.name}' был побежден ранее. Деактивируем.");
                        wolf.gameObject.SetActive(false); // Или Destroy(wolf.gameObject) если они не нужны вообще
                    }
                }
            }

            // Обработка возврата из боя
            if (BattleDataHolder.JustReturnedFromBattle)
            {
                Debug.Log("[GSM] Обнаружен возврат из боя. Обработка исхода...");
                string outcome = BattleDataHolder.BattleOutcome;
                int damageFromBattle = BattleDataHolder.DamageToPlayer;
                string activeWolfGOName = BattleDataHolder.ActiveWolfGameObjectName; 
                WolfEncounter wolfToHandle = null;

                if (!string.IsNullOrEmpty(activeWolfGOName)) {
                    GameObject wolfGO = GameObject.Find(activeWolfGOName); 
                    if (wolfGO != null) wolfToHandle = wolfGO.GetComponent<WolfEncounter>();
                    else Debug.LogWarning($"[GSM] Не удалось найти GameObject волка '{activeWolfGOName}' после боя.");
                }

                if (!string.IsNullOrEmpty(outcome)) {  
                    Debug.Log($"[GSM] Исход боя: {outcome}, Урон от боя: {damageFromBattle}");
                    if (playerStats != null && (outcome == "Defeat" || outcome == "DefeatTimeOut") && damageFromBattle > 0 && outcome != "ErrorNoWolf") {
                         playerStats.TakeDamage(damageFromBattle);
                         // Обновляем loadedData после получения урона, перед финальным сохранением
                         loadedData.currentPlayerHealth = playerStats.CurrentHealth;
                    }
                    
                    if (wolfToHandle != null) {
                        if (outcome == "Victory") 
                        {
                            wolfToHandle.HandleBattleWon(); 
                            if (loadedData.defeatedWolfIdentifiers != null && !loadedData.defeatedWolfIdentifiers.Contains(activeWolfGOName)) 
                            {
                                loadedData.defeatedWolfIdentifiers.Add(activeWolfGOName);
                                Debug.Log($"[GSM] Волк '{activeWolfGOName}' добавлен в список побежденных.");
                                // SaveLoadManager.SaveGame(loadedData); // Убрали отсюда, будет общее сохранение ниже
                            }
                        }
                        else 
                        {
                            wolfToHandle.HandleBattleLostOrOther();
                        }
                    } else if (!string.IsNullOrEmpty(activeWolfGOName)) {
                         Debug.LogWarning($"[GSM] Ссылка на WolfEncounter для волка '{activeWolfGOName}' не найдена, не могу вызвать Handle методы.");
                    }
                }
                
                // Сохраняем состояние игры ПОСЛЕ обработки исхода боя, если это не был переход на GameOver
                // (GameOverScreenHandler сам удаляет сохранение при выходе в меню)
                if (!IsGameOverConditionMet()) // Добавим проверку, чтобы не сохранять, если сейчас будет GameOver
                {
                    // Убедимся, что loadedData содержит актуальные данные игрока
                    if (playerStats != null) 
                    {
                        loadedData.currentPlayerHealth = playerStats.CurrentHealth;
                        loadedData.maxPlayerHealth = playerStats.maxHealth; 
                        // Позиция игрока уже в loadedData из первоначальной загрузки, 
                        // и должна быть той, где он оказался после возврата из боя.
                        // Если есть PlayerController, который может двигать игрока сразу после возврата,
                        // то позицию тоже нужно было бы обновить здесь из playerStats.transform.position.
                        // Пока предполагаем, что позиция из loadedData актуальна.
                    }
                    Debug.Log($"[GSM] Сохранение состояния игры ПОСЛЕ обработки исхода боя. PlayerHP: {loadedData.currentPlayerHealth}, DefeatedWolves: {string.Join(", ", loadedData.defeatedWolfIdentifiers)}");
                    SaveLoadManager.SaveGame(loadedData); 
                }
                else
                {
                    Debug.Log("[GSM] Условие Game Over выполнено. Сохранение после боя не будет произведено.");
                }
            }
            
            BattleDataHolder.ResetSessionData();
            Debug.Log("[GSM] BattleDataHolder.ResetSessionData() вызван.");

            // Проверка на Game Over ПОСЛЕ применения урона от боя
            CheckForGameOver();
        } 
        else if (scene.name == gameOverSceneName) {
            Debug.Log($"[GSM] СЦЕНА '{gameOverSceneName}' ЗАГРУЖЕНА.");
            PlayerController.isGamePaused = false; 
            Time.timeScale = 1f; 
        } 
    }

    // Публичный метод для сохранения игры, который могут вызывать другие скрипты
    public void SaveCurrentGameState()
    {
        if (playerStats == null && playerGameObject != null) playerStats = playerGameObject.GetComponent<PlayerStats>();
        if (girlMovement == null) {
            GameObject ghostGirlObject = GameObject.FindGameObjectWithTag(ghostGirlTag);
            if (ghostGirlObject != null) girlMovement = ghostGirlObject.GetComponent<GirlMovement>();
        }

        if (playerStats == null) {
            Debug.LogError("[GSM] Не могу сохранить игру: PlayerStats не найден!");
            return;
        }

        PlayerData dataToSave = new PlayerData(); // Создаем пустой и заполняем
        
        playerStats.PopulateSaveData(dataToSave);
        if (girlMovement != null) {
            girlMovement.PopulateSaveData(dataToSave);
        } else {
            // Если девочки нет, можно пометить ее состояние как не сохраненное
            dataToSave.hasSavedGhostGirlState = false;
        }
        
        // Сюда можно добавить сохранение другого состояния мира, если нужно
        // dataToSave.defeatedWolfIdentifiers = ...

        SaveLoadManager.SaveGame(dataToSave);
    }

    // Вспомогательный метод для проверки условия Game Over без немедленного перехода на сцену
    private bool IsGameOverConditionMet()
    {
        if (playerStats != null && playerStats.CurrentHealth <= 0)
        {
            return true;
        }
        return false;
    }

    void CheckForGameOver()
    {
        Debug.Log($"[GSM] ПРОВЕРКА НА GAME OVER:");
        if (playerStats != null) {
            Debug.Log($"  - playerStats.CurrentHealth: {playerStats.CurrentHealth}");
            if (playerStats.CurrentHealth <= 0) {
                Debug.LogError($"  -- GAME OVER УСЛОВИЕ ВЫПОЛНЕНО!");
                PlayerController.isGamePaused = true; 
                if (string.IsNullOrEmpty(gameOverSceneName)) {
                    Debug.LogError("  -- ОШИБКА: Имя сцены Game Over НЕ УКАЗАНО!");
                } else {
                    Debug.Log($"  -- Загрузка сцены Game Over: '{gameOverSceneName}'...");
                    // Перед уходом на Game Over мы не удаляем файл сохранения,
                    // чтобы "Продолжить" было доступно (но начнется с 0 HP -> снова Game Over).
                    // Если нужно, чтобы "Продолжить" было недоступно, то здесь SaveLoadManager.DeleteSaveFile();
                    // ИЛИ MainMenuController.StartNewGame() должен вызываться из GameOverScreenHandler
                    BattleDataHolder.ResetSessionData(); // Сбрасываем временные данные
                    SceneManager.LoadScene(gameOverSceneName); 
                }
            }
        } else {
            Debug.LogWarning("[GSM] playerStats is NULL при проверке на Game Over.");
        }
    }
}