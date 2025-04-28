using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Sprites
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            startButton = GameObject.Find("StartButton").GetComponent<Button>();
            quitButton = GameObject.Find("QuitButton").GetComponent<Button>();

            startButton.onClick.AddListener(StartGame);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void StartGame()
        {
            GetComponent<SceneLoader>().LoadScene("GameScene"); 
        }

        private void QuitGame()
        {
            Application.Quit();
        }
    }
}