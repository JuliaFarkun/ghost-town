using UnityEngine;
using UnityEngine.UI; // Все еще нужен, если используем компонент Image для цвета и т.д.
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("Элементы UI Здоровья")]
    public TextMeshProUGUI healthText;
    // Убираем ссылку на Image healthBarFill, если Image Type не работает
    // public Image healthBarFill; 
    public RectTransform healthBarFillRect; // Ссылка на RectTransform заполняющей части

    private float originalHealthBarWidth = -1f; // Инициализируем значением, указывающим на то, что ширина не была получена

    [Header("Панель Обучения")]
    public GameObject tutorialPanel;

    void OnEnable()
    {
        PlayerStats.OnHealthChanged += UpdateHealthUI;
    }

    void OnDisable()
    {
        PlayerStats.OnHealthChanged -= UpdateHealthUI;
    }

    void Start()
    {
        if (healthBarFillRect != null)
        {
            // Запоминаем оригинальную ширину один раз при старте
            // Это должна быть ширина, соответствующая 100% здоровья
            originalHealthBarWidth = healthBarFillRect.sizeDelta.x;
            if (originalHealthBarWidth <= 0)
            {
                Debug.LogWarning("GameUIManager: Original HealthBar width for healthBarFillRect is 0 or less. Health bar might not display correctly. Ensure it's set in Editor or its parent (HealthBar_Background) has a defined width.");
                // Попытка взять ширину родителя, если своя некорректна
                RectTransform parentRect = healthBarFillRect.parent as RectTransform;
                if (parentRect != null)
                {
                    originalHealthBarWidth = parentRect.rect.width;
                     // Устанавливаем начальную ширину такую же, как у родителя
                    healthBarFillRect.sizeDelta = new Vector2(originalHealthBarWidth, healthBarFillRect.sizeDelta.y);
                }

            }
        }
        else
        {
            Debug.LogWarning("GameUIManager: HealthBarFillRect не назначен!");
        }

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameUIManager: TutorialPanel не назначен!");
        }

        // Первичное обновление UI, если игрок уже существует
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                UpdateHealthUI(playerStats.CurrentHealth, playerStats.maxHealth);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleTutorialPanel();
        }
    }

    void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth} / {maxHealth}";
        }

        if (healthBarFillRect != null && originalHealthBarWidth > 0) // Убедимся, что оригинальная ширина была получена
        {
            float healthPercentage = 0f;
            if (maxHealth > 0) // Избегаем деления на ноль
            {
                healthPercentage = (float)currentHealth / maxHealth;
            }
            
            // Устанавливаем новую ширину для healthBarFillRect
            // Высоту оставляем прежней (sizeDelta.y)
            healthBarFillRect.sizeDelta = new Vector2(originalHealthBarWidth * healthPercentage, healthBarFillRect.sizeDelta.y);
        }
    }

    public void ToggleTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(!tutorialPanel.activeSelf);
        }
    }

    public void CloseTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
}