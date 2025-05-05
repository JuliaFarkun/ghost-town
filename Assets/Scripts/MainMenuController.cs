using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private LoadingController sceneLoader; // Ссылка на SceneLoader

    private void Start()
    {
        // Находим кнопки через Inspector (лучше использовать SerializeField)
        Button startButton = GameObject.Find("StartButton").GetComponent<Button>();
        Button quitButton = GameObject.Find("QuitButton").GetComponent<Button>();

        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);

        // Проверка наличия SceneLoader
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader не привязан к MainMenuController!");
        }
    }

    private void StartGame()
    {
        sceneLoader.LoadScene("GameScene"); // Используем явную ссылку
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}