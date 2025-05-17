using UnityEngine;
using UnityEngine.SceneManagement; // Важно для управления сценами

public class SceneLoader : MonoBehaviour
{
    // Метод для загрузки главной игровой сцены
    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene"); // Убедись, что имя сцены совпадает
    }

    // Метод для загрузки сцены главного меню
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenu"); // Убедись, что имя сцены совпадает
    }

    // Метод для загрузки сцены настроек
    public void LoadSettingsScene()
    {
        SceneManager.LoadScene("SettingsScene"); // Убедись, что имя сцены совпадает
    }

    // Метод для выхода из игры
    public void QuitGame()
    {
        Debug.Log("Quitting game..."); // Для проверки в редакторе
        Application.Quit();
    }

    // --- Для будущего использования сценой настроек ---
    private static string previousSceneName;

    public static void LoadSettingsAndRemember()
    {
        previousSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("SettingsScene");
    }

    public void ReturnToPreviousScene()
    {
        if (!string.IsNullOrEmpty(previousSceneName))
        {
            SceneManager.LoadScene(previousSceneName);
        }
        else
        {
            // Если по какой-то причине предыдущая сцена не запомнилась,
            // возвращаемся в главное меню как запасной вариант
            LoadMainMenuScene();
        }
    }
    // -----------------------------------------------------
}