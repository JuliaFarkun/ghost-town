using UnityEngine;
using UnityEngine.UI; // Для Image и AspectRatioFitter
// using TMPro; // Если используешь TextMeshPro для кнопки

public class NoteDisplayUI : MonoBehaviour
{
    public GameObject notePanel;
    public Image noteImageComponent; // Твой NoteImageDisplay
    public Button closeButton;
    public GameObject healthUI; // Ссылка на UI здоровья
    public GameObject instructionsUI; // Ссылка на UI инструкций
    private AspectRatioFitter aspectRatioFitter; // Ссылка на компонент

    void Awake() // Используем Awake, чтобы получить компонент до первого Start
    {
        Debug.Log("[NoteDisplayUI] Awake вызван.");
        if (noteImageComponent != null)
        {
            aspectRatioFitter = noteImageComponent.GetComponent<AspectRatioFitter>();
            if (aspectRatioFitter == null)
            {
                Debug.LogWarning("[NoteDisplayUI] AspectRatioFitter не найден на NoteImageComponent. Картинка может не масштабироваться правильно.");
            }
            else
            {
                Debug.Log("[NoteDisplayUI] AspectRatioFitter найден на NoteImageComponent.");
            }
        }
        else
        {
            Debug.LogError("[NoteDisplayUI] NoteImageComponent не назначен в NoteDisplayUI!");
        }
    }

    void Start()
    {
        Debug.Log("[NoteDisplayUI] Start вызван.");
        if (notePanel) 
        {
            notePanel.SetActive(false);
            Debug.Log("[NoteDisplayUI] notePanel изначально установлен в false.");
        }
        else
        {
            Debug.LogWarning("[NoteDisplayUI] notePanel не назначен!");
        }

        if (closeButton) 
        {
            closeButton.onClick.AddListener(CloseNote);
            Debug.Log("[NoteDisplayUI] Слушатель на closeButton добавлен.");
        }
        else
        {
            Debug.LogWarning("[NoteDisplayUI] closeButton не назначен!");
        }

        // Скрываем UI здоровья и инструкций, если они назначены
        // if (healthUI != null)
        // {
        //     healthUI.SetActive(false);
        //     Debug.Log("[NoteDisplayUI] HealthUI скрыт.");
        // }
        // if (instructionsUI != null)
        // {
        //     instructionsUI.SetActive(false);
        //     Debug.Log("[NoteDisplayUI] InstructionsUI скрыт.");
        // }
    }

    public void ShowNote(Sprite image)
    {
        Debug.Log("[NoteDisplayUI] ShowNote вызван.");
        if (noteImageComponent != null && image != null)
        {
            noteImageComponent.sprite = image;
            Debug.Log($"[NoteDisplayUI] Sprite для NoteImageComponent установлен: {image.name}. Размеры: {image.rect.width}x{image.rect.height}");

            if (aspectRatioFitter != null)
            {
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                aspectRatioFitter.aspectRatio = (float)image.rect.width / (float)image.rect.height;
                Debug.Log("[NoteDisplayUI] AspectRatioFitter настроен.");
            }
        }
        else
        {
            if (noteImageComponent == null) Debug.LogWarning("[NoteDisplayUI] NoteImageComponent не назначен!");
            if (image == null) Debug.LogWarning("[NoteDisplayUI] Sprite для записки не назначен!");
            return; 
        }

        if (notePanel) 
        {
            notePanel.SetActive(true);
            Debug.Log("[NoteDisplayUI] notePanel активирован.");
        }

        // Скрываем UI здоровья и инструкций, если они назначены
        if (healthUI != null)
        {
            healthUI.SetActive(false);
            Debug.Log("[NoteDisplayUI] HealthUI скрыт.");
        }
        if (instructionsUI != null)
        {
            instructionsUI.SetActive(false);
            Debug.Log("[NoteDisplayUI] InstructionsUI скрыт.");
        }
    }

    public void CloseNote()
    {
        Debug.Log("[NoteDisplayUI] CloseNote вызван.");
        if (notePanel) 
        {
            notePanel.SetActive(false);
            Debug.Log("[NoteDisplayUI] notePanel деактивирован.");
        }
        PlayerController.isGamePaused = false;
        Time.timeScale = 1f;
        Debug.Log("[NoteDisplayUI] Игра возобновлена (Time.timeScale = 1f, PlayerController.isGamePaused = false).");

        // Показываем UI здоровья и инструкций обратно, если они назначены
        if (healthUI != null)
        {
            healthUI.SetActive(true);
            Debug.Log("[NoteDisplayUI] HealthUI показан.");
        }
        if (instructionsUI != null)
        {
            instructionsUI.SetActive(true);
            Debug.Log("[NoteDisplayUI] InstructionsUI показан.");
        }
    }
}