using UnityEngine;
using UnityEngine.EventSystems; // Для проверки, не кликнули ли мы по UI

public class InteractableNote : MonoBehaviour
{
    public Sprite noteImageToShow; // Сюда перетащишь картинку записки в Inspector
    public NoteDisplayUI noteDisplay; // Ссылка на UI для показа записки

    void Start()
    {
        // Пытаемся найти NoteDisplayUI на сцене, если не присвоено вручную
        if (noteDisplay == null)
        {
            noteDisplay = FindObjectOfType<NoteDisplayUI>();
            if (noteDisplay == null)
            {
                Debug.LogError("NoteDisplayUI не найден на сцене!");
            }
        }
    }

    // Этот метод будет вызван, если на GameObject есть Collider и по нему кликнули мышкой
    void OnMouseDown()
    {
        // Проверяем, не кликнули ли мы по UI элементу (чтобы не срабатывало сквозь кнопки)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (noteDisplay != null && noteImageToShow != null)
        {
            noteDisplay.ShowNote(noteImageToShow);
            PlayerController.isGamePaused = true; // Ставим игру на паузу
            Time.timeScale = 0f; // Останавливаем время в игре
        }
        else
        {
            Debug.LogWarning("Note image or NoteDisplayUI не назначены для " + gameObject.name);
        }
    }
}