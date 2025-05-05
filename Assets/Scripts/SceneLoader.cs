// LoadingController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    [SerializeField] private Slider progressBar;

    void Start()
    {
        Debug.Log("--- LoadingController: Start() ---"); // <-- Лог начала

        string targetSceneName = PlayerPrefs.GetString("NextSceneToLoad", "GameScene_DEFAULT"); // Используем другое значение по умолчанию для отладки
        Debug.Log($"Целевая сцена из PlayerPrefs: {targetSceneName}"); // <-- Лог имени сцены

        if (string.IsNullOrEmpty(targetSceneName) || targetSceneName == "GameScene_DEFAULT")
        {
            Debug.LogError("Не удалось получить корректное имя целевой сцены из PlayerPrefs!");
            return;
        }

        // УКАЖИТЕ ЗДЕСЬ ПРАВИЛЬНЫЙ ПУТЬ К ПАПКЕ СО СЦЕНАМИ!
        string scenePath = "Assets/Scenes/" + targetSceneName + ".unity"; // <-- Пример пути, исправьте если нужно!
        Debug.Log($"Проверяем путь к сцене: {scenePath}"); // <-- Лог пути

        if (SceneUtility.GetBuildIndexByScenePath(scenePath) < 0)
        {
             Debug.LogError($"Сцена '{targetSceneName}' по пути '{scenePath}' НЕ НАЙДЕНА в Build Settings!");
             return; // <-- Выход, если сцена не найдена
        }
        else
        {
             Debug.Log($"Сцена '{targetSceneName}' найдена в Build Settings.");
        }

        Debug.Log($"Запускаем корутину LoadTargetSceneAsync для сцены: {targetSceneName}");
        StartCoroutine(LoadTargetSceneAsync(targetSceneName));
    }

    private IEnumerator LoadTargetSceneAsync(string targetSceneName)
    {
        Debug.Log("--- Корутина LoadTargetSceneAsync: Начало ---");
        yield return new WaitForSeconds(0.5f); // Небольшая пауза

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);

        if (asyncLoad == null)
        {
            Debug.LogError($"SceneManager.LoadSceneAsync не смог начать загрузку сцены {targetSceneName}!");
            yield break; // Выходим из корутины
        }

        Debug.Log("Асинхронная загрузка начата. Входим в цикл ожидания.");
        // asyncLoad.allowSceneActivation = false; // Пока не используем для простоты

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            // Логируем прогресс реже, чтобы не засорять консоль
            if (Time.frameCount % 30 == 0) // Лог каждые 30 кадров примерно
               Debug.Log($"Прогресс загрузки ({targetSceneName}): {asyncLoad.progress * 100f}% (Отображаемый: {progress * 100f}%)");

            if (progressBar != null)
                progressBar.value = progress;

            yield return null;
        }

         // Этот лог появится, когда asyncLoad.isDone станет true
        Debug.Log($"--- Корутина LoadTargetSceneAsync: Цикл завершен. Сцена {targetSceneName} загружена и должна быть активна. ---");
    }
}