// GameOverScreenHandler.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreenHandler : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu"; 

    public void GoToMainMenu()
    {
        Debug.Log("[GameOverScreenHandler] Возврат в главное меню из Game Over. Удаление файла сохранения.");
        
        SaveLoadManager.DeleteSaveFile(); // Удаляем файл сохранения, чтобы "Продолжить" было неактивно
        BattleDataHolder.ResetSessionData(); // Сбрасываем временные данные боя

        PlayerController.isGamePaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}