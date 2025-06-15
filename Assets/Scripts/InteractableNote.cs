using UnityEngine;
using UnityEngine.EventSystems; // Для проверки, не кликнули ли мы по UI

public class InteractableNote : MonoBehaviour
{
    public Sprite noteImageToShow; // Сюда перетащишь картинку записки в Inspector
    public NoteDisplayUI noteDisplay; // Ссылка на UI для показа записки

    void Start()
    {
        Debug.Log($"[InteractableNote] Start вызван для объекта: {gameObject.name}");
        // Пытаемся найти NoteDisplayUI на сцене, если не присвоено вручную
        if (noteDisplay == null)
        {
            noteDisplay = GameObject.FindAnyObjectByType<NoteDisplayUI>();
            if (noteDisplay == null)
            {
                Debug.LogError("[InteractableNote] NoteDisplayUI не найден на сцене! Убедитесь, что он существует и активен.");
            }
            else
            {
                Debug.Log("[InteractableNote] NoteDisplayUI найден автоматически.");
            }
        }
        else
        {
            Debug.Log("[InteractableNote] NoteDisplayUI назначен вручную.");
        }
        if (noteImageToShow == null) {
            Debug.LogWarning("[InteractableNote] noteImageToShow не назначен в Inspector для объекта: " + gameObject.name);
        }
    }

    // Этот метод будет вызван, если на GameObject есть Collider и по нему кликнули мышкой
    void OnMouseDown()
    {
        Debug.Log("[InteractableNote] OnMouseDown вызван.");
        // Проверяем, не кликнули ли мы по UI элементу (чтобы не срабатывало сквозь кнопки)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[InteractableNote] Клик был по UI элементу, игнорируем.");
            return;
        }
        Debug.Log("[InteractableNote] Клик не был по UI элементу.");

        if (noteDisplay != null && noteImageToShow != null)
        {
            Debug.Log("[InteractableNote] NoteDisplay и noteImageToShow назначены. Вызов ShowNote.");
            noteDisplay.ShowNote(noteImageToShow);
            PlayerController.isGamePaused = true; // Ставим игру на паузу
            Time.timeScale = 0f; // Останавливаем время в игре
            Debug.Log("[InteractableNote] Игра на паузе (Time.timeScale = 0f, PlayerController.isGamePaused = true).");
        }
        else
        {
            if (noteDisplay == null) Debug.LogWarning("[InteractableNote] NoteDisplay не назначен при клике!");
            if (noteImageToShow == null) Debug.LogWarning("[InteractableNote] Sprite для записки не назначен при клике!");
        }
    }
}