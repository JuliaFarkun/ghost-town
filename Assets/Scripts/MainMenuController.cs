// MainMenuController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Ссылки на UI Элементы")]
    public Button continueButton; 

    [Header("Имена Сцен для Навигации")]
    public string gameSceneName = "GameScene";       
    public string settingsSceneName = "SettingsScene"; 
    public string mainMenuSceneName = "MainMenu";     

    [Header("Начальные Параметры для Новой Игры")]
    public int defaultPlayerMaxHealth = 100;
    public Vector3 defaultPlayerStartPosition = new Vector3(4.05f, -8.76f, -1.2f);
    public Vector3 defaultGhostGirlStartPosition = new Vector3(1.5f, -2.8f, 0f);
    public int defaultGhostGirlStartWaypoint = 0;

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.interactable = SaveLoadManager.DoesSaveFileExist();
            Debug.Log($"[MainMenuController] Кнопка 'Продолжить' interactable: {continueButton.interactable}");
        }
        else Debug.LogWarning("[MainMenuController] Кнопка 'Продолжить' (continueButton) не назначена!");

        PlayerController.isGamePaused = false;
        Time.timeScale = 1f;
    }

    public void StartNewGame()
    {
        Debug.Log("[MainMenuController] Нажата кнопка 'Новая игра'. Создание/перезапись файла сохранения.");
        Debug.Log($"[MainMenuController] Используемые defaultPlayerStartPosition: {defaultPlayerStartPosition}");
        Debug.Log($"[MainMenuController] Используемые defaultGhostGirlStartPosition: {defaultGhostGirlStartPosition} (из скрипта перед созданием PlayerData)");
        
        // Создаем данные для новой игры
        PlayerData newGameData = new PlayerData(
            defaultPlayerMaxHealth, 
            defaultPlayerStartPosition,
            defaultGhostGirlStartPosition,
            defaultGhostGirlStartWaypoint
        );
        Debug.Log($"[MainMenuController] Созданы newGameData с PlayerPos: {newGameData.GetPlayerPosition()} и GirlPos: {newGameData.GetGhostGirlPosition()}");
        SaveLoadManager.SaveGame(newGameData); // Сохраняем эти дефолтные данные в файл

        // BattleDataHolder сбрасывается в GameSceneManager при загрузке GameScene,
        // но можно и здесь для чистоты, если он мог содержать что-то от предыдущей сессии.
        BattleDataHolder.ResetSessionData(); 

        PlayerController.isGamePaused = false;
        Time.timeScale = 1f;
        
        Debug.Log($"[MainMenuController] Загрузка сцены: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    public void ContinueGame()
    {
        if (SaveLoadManager.DoesSaveFileExist()) 
        {
            Debug.Log("[MainMenuController] Нажата кнопка 'Продолжить'. Загрузка игры из файла.");
            // Данные будут загружены из файла в GameSceneManager.OnSceneLoaded()
            
            BattleDataHolder.ResetSessionData(); // Сбрасываем временные данные боя, чтобы они не конфликтовали
            PlayerController.isGamePaused = false;
            Time.timeScale = 1f;

            Debug.Log($"[MainMenuController] Загрузка сцены: {gameSceneName}");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("[MainMenuController] Попытка 'Продолжить' без файла сохранения. Запускаем как новую игру.");
            StartNewGame(); 
        }
    }

    public void LoadSettings()
    {
        if (string.IsNullOrEmpty(settingsSceneName)) {
            Debug.LogError("[MainMenuController] Имя сцены настроек (settingsSceneName) не указано!"); return;
        }
        Debug.Log($"[MainMenuController] Загрузка сцены настроек: {settingsSceneName}");
        // Для возврата можно использовать статический SceneLoader или передавать имя текущей сцены
        // SceneLoader.LoadSettingsAndRemember(); // Если используешь старый SceneLoader
        SceneManager.LoadScene(settingsSceneName);
    }
    
    public void GoToMainMenu() // Вызывается из GameOver или Settings
    {
        Debug.Log($"[MainMenuController] Возврат в главное меню: {mainMenuSceneName}");
        // При возврате в главное меню (особенно из Game Over), файл сохранения уже должен быть
        // либо удален (если мы хотим, чтобы "Продолжить" было недоступно),
        // либо перезаписан при "Новой игре".
        // Если из Game Over мы просто выходим в меню, то файл сохранения остается,
        // и "Продолжить" будет доступно, но игрок начнет с низким HP (что приведет к Game Over снова).
        // Поэтому GameOverScreenHandler должен либо удалять сохранение, либо перезаписывать его дефолтными данными.
        SceneManager.LoadScene(mainMenuSceneName); 
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenuController] Нажата кнопка 'Выход'.");
        // Здесь можно добавить автосохранение перед выходом, если GameSceneManager это умеет
        // GameSceneManager.Instance?.SaveCurrentGameState(); // Пример
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif
    }
}