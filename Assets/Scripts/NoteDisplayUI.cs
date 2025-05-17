using UnityEngine;
using UnityEngine.UI; // Для Image и AspectRatioFitter
// using TMPro; // Если используешь TextMeshPro для кнопки

public class NoteDisplayUI : MonoBehaviour
{
    public GameObject notePanel;
    public Image noteImageComponent; // Твой NoteImageDisplay
    public Button closeButton;
    private AspectRatioFitter aspectRatioFitter; // Ссылка на компонент

    void Awake() // Используем Awake, чтобы получить компонент до первого Start
    {
        if (noteImageComponent != null)
        {
            aspectRatioFitter = noteImageComponent.GetComponent<AspectRatioFitter>();
            if (aspectRatioFitter == null)
            {
                Debug.LogWarning("AspectRatioFitter не найден на NoteImageComponent. Картинка может не масштабироваться правильно.");
            }
        }
        else
        {
            Debug.LogError("NoteImageComponent не назначен в NoteDisplayUI!");
        }
    }

    void Start()
    {
        if (notePanel) notePanel.SetActive(false);
        if (closeButton) closeButton.onClick.AddListener(CloseNote);
    }

    public void ShowNote(Sprite image)
    {
        if (noteImageComponent && image != null)
        {
            noteImageComponent.sprite = image;

            if (aspectRatioFitter != null)
            {
                // Устанавливаем режим, если он вдруг сбился (хотя должен быть настроен в Inspector)
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                // Рассчитываем и устанавливаем соотношение сторон из спрайта
                aspectRatioFitter.aspectRatio = (float)image.rect.width / (float)image.rect.height;
            }
        }
        else
        {
            Debug.LogWarning("NoteImageComponent или Sprite для записки не назначены!");
            return; // Выходим, если нет картинки или компонента для ее отображения
        }

        if (notePanel) notePanel.SetActive(true);
        // Пауза игры уже должна быть установлена в InteractableNote
    }

    public void CloseNote()
    {
        if (notePanel) notePanel.SetActive(false);
        PlayerController.isGamePaused = false;
        Time.timeScale = 1f;
    }
}