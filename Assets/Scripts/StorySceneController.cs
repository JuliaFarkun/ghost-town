using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySceneController : MonoBehaviour
{
    [Header("Имена Сцен для Навигации")]
    public string gameSceneName = "GameScene";

    void Update()
    {
        // Проверяем, нажата ли любая клавиша
        if (Input.anyKeyDown)
        {
            Debug.Log("[StorySceneController] Нажата любая клавиша. Загрузка GameScene.");
            SceneManager.LoadScene(gameSceneName);
        }
    }
} 