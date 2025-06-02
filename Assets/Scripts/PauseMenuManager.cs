using UnityEngine;
using UnityEngine.SceneManagement; // Для SceneLoader

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    // public PlayerController playerController; // Если нужно прямое взаимодействие
    public SceneLoader sceneLoader; // Для навигации

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false); // Скрыть при старте

        // Пытаемся найти SceneLoader, если не присвоен
        if (sceneLoader == null)
        {
            // SceneLoader может быть на отдельном объекте или на этом же
            // sceneLoader = FindObjectOfType<SceneLoader>(); // Устаревший метод
            sceneLoader = GameObject.FindAnyObjectByType<SceneLoader>();
            if (sceneLoader == null)
            {
                // Если SceneLoader'а нет вообще, создадим временный объект с ним
                GameObject slGo = new GameObject("TempSceneLoader");
                sceneLoader = slGo.AddComponent<SceneLoader>();
                Debug.LogWarning("SceneLoader не был найден, создан временный. Лучше добавить его на сцену вручную.");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            ActivatePauseMenu();
        }
        else
        {
            DeactivatePauseMenu();
        }
    }

    void ActivatePauseMenu()
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Останавливаем время
        PlayerController.isGamePaused = true; // Устанавливаем флаг паузы для игрока
    }

    public void DeactivatePauseMenu() // Сделаем public, чтобы кнопка "Продолжить" могла его вызвать
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Возобновляем время
        PlayerController.isGamePaused = false;
        isPaused = false; // Сбрасываем локальный флаг
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Важно восстановить время перед сменой сцены
        PlayerController.isGamePaused = false;
        if (sceneLoader) sceneLoader.LoadMainMenuScene();
    }

    public void GoToSettings()
    {
        // Time.timeScale уже 0f, если меню паузы открыто.
        // SceneLoader.LoadSettingsAndRemember() запомнит GameScene
        if (sceneLoader) SceneLoader.LoadSettingsAndRemember();
    }
}