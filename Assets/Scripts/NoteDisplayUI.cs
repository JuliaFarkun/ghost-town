using UnityEngine;
using UnityEngine.UI; // Для Image
using TMPro; // Если используешь TextMeshPro для кнопки

public class NoteDisplayUI : MonoBehaviour
{
    public GameObject notePanel; // Перетащи сюда Panel из Hierarchy
    public Image noteImageComponent; // Перетащи сюда Image для показа записки
    public Button closeButton; // Перетащи сюда кнопку закрытия

    void Start()
    {
        if (notePanel) notePanel.SetActive(false); // Скрыть панель при старте
        if (closeButton) closeButton.onClick.AddListener(CloseNote); // Назначить действие кнопке
    }

    public void ShowNote(Sprite image)
    {
        if (noteImageComponent) noteImageComponent.sprite = image;
        if (notePanel) notePanel.SetActive(true);
        // Пауза игры уже должна быть установлена в InteractableNote
        // PlayerController.isGamePaused = true;
        // Time.timeScale = 0f;
    }

    public void CloseNote()
    {
        if (notePanel) notePanel.SetActive(false);
        PlayerController.isGamePaused = false; // Возобновляем игру
        Time.timeScale = 1f; // Возобновляем время
    }
}