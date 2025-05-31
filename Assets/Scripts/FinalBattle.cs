using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FinalBattleController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int uiFontSize = 24;
    [SerializeField] private float uiSafeZonePadding = 50f;

    private float currentTime;
    private int score;
    private bool isGameRunning;
    private List<GameObject> activeButtons = new List<GameObject>();
    private Coroutine spawnCoroutine;
    private RectTransform canvasRect;
    private int totalPressedButtons;
    private int correctPressedButtons;
    private Rect timerTextRect;
    private Rect scoreTextRect;

    void Start()
    {
        canvasRect = FindAnyObjectByType<Canvas>().GetComponent<RectTransform>();
        if (canvasRect == null) Debug.LogError("Canvas not found!");
        if (buttonPrefab == null) Debug.LogError("Button prefab not assigned!");

        SetupTextPositions();
        CalculateExclusionZones();
        InitializeGame();
    }

    void SetupTextPositions()
    {
        countdownText.alignment = TextAlignmentOptions.Center;
        countdownText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        countdownText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        countdownText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        countdownText.rectTransform.anchoredPosition = Vector2.zero;
        countdownText.fontSize = uiFontSize * 1.5f;
        countdownText.textWrappingMode = TextWrappingModes.NoWrap;

        timerText.alignment = TextAlignmentOptions.Left;
        timerText.rectTransform.anchorMin = new Vector2(0f, 1f);
        timerText.rectTransform.anchorMax = new Vector2(0f, 1f);
        timerText.rectTransform.pivot = new Vector2(0f, 1f);
        timerText.rectTransform.anchoredPosition = new Vector2(20, 20);
        timerText.fontSize = uiFontSize;

        scoreText.alignment = TextAlignmentOptions.Right;
        scoreText.rectTransform.anchorMin = new Vector2(1f, 1f);
        scoreText.rectTransform.anchorMax = new Vector2(1f, 1f);
        scoreText.rectTransform.pivot = new Vector2(1f, 1f);
        scoreText.rectTransform.anchoredPosition = new Vector2(-20, 20);
        scoreText.fontSize = uiFontSize;
    }

    void CalculateExclusionZones()
    {
        Vector2 timerSize = new Vector2(
            timerText.preferredWidth + uiSafeZonePadding,
            timerText.preferredHeight + uiSafeZonePadding
        );
        
        Vector2 scoreSize = new Vector2(
            scoreText.preferredWidth + uiSafeZonePadding,
            scoreText.preferredHeight + uiSafeZonePadding
        );

        timerTextRect = new Rect(
            timerText.rectTransform.anchoredPosition.x - uiSafeZonePadding/2,
            timerText.rectTransform.anchoredPosition.y - timerSize.y + uiSafeZonePadding/2,
            timerSize.x,
            timerSize.y
        );

        scoreTextRect = new Rect(
            scoreText.rectTransform.anchoredPosition.x - scoreSize.x + uiSafeZonePadding/2,
            scoreText.rectTransform.anchoredPosition.y - scoreSize.y + uiSafeZonePadding/2,
            scoreSize.x,
            scoreSize.y
        );
    }

    void InitializeGame()
    {
        currentTime = gameDuration;
        score = 0;
        isGameRunning = false;
        totalPressedButtons = 0;
        correctPressedButtons = 0;

        countdownText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);

        UpdateScore();
        UpdateTimer();

        StartCoroutine(GameRoutine());
    }

    IEnumerator GameRoutine()
    {
        yield return StartCoroutine(CountdownPhase());
        yield return StartCoroutine(MainGamePhase());
        yield return StartCoroutine(EndGamePhase());
    }

    IEnumerator CountdownPhase()
    {
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);
        
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);
        
        countdownText.text = "1";
        yield return new WaitForSeconds(1f);
        
        countdownText.text = "START";
        yield return new WaitForSeconds(1f);
        
        countdownText.gameObject.SetActive(false);
    }

    IEnumerator MainGamePhase()
    {
        isGameRunning = true;
        spawnCoroutine = StartCoroutine(SpawnButtonsRoutine());
        
        while (currentTime > 0 && isGameRunning)
        {
            currentTime -= Time.deltaTime;
            UpdateTimer();
            yield return null;
        }
    }

    IEnumerator SpawnButtonsRoutine()
    {
        while (isGameRunning)
        {
            SpawnButton();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnButton()
    {
        GameObject newButton = Instantiate(buttonPrefab, canvasRect.transform);
        if (newButton == null) return;

        RectTransform rt = newButton.GetComponent<RectTransform>();
        rt.anchoredPosition = GetRandomSafePosition();
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;

        Image img = newButton.GetComponent<Image>();
        img.color = Color.white;

        TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
        char randomChar = GetRandomChar();
        buttonText.text = randomChar.ToString();
        buttonText.color = Color.black;

        Button button = newButton.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnButtonClick(newButton, randomChar));

        activeButtons.Add(newButton);
    }

    Vector2 GetRandomSafePosition()
    {
        float buttonWidth = buttonPrefab.GetComponent<RectTransform>().rect.width;
        float buttonHeight = buttonPrefab.GetComponent<RectTransform>().rect.height;
        
        float xMin = (-canvasRect.rect.width / 2) + (buttonWidth / 2);
        float xMax = (canvasRect.rect.width / 2) - (buttonWidth / 2);
        float yMin = (-canvasRect.rect.height / 2) + (buttonHeight / 2);
        float yMax = (canvasRect.rect.height / 2) - (buttonHeight / 2);

        Vector2 randomPosition;
        int attempts = 0;
        const int maxAttempts = 100;
        
        do
        {
            randomPosition = new Vector2(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );
            
            attempts++;
            if (attempts >= maxAttempts) 
            {
                Debug.LogWarning("Max attempts reached for safe position");
                break;
            }
            
        } while (IsPositionOverUI(randomPosition));

        return randomPosition;
    }

    bool IsPositionOverUI(Vector2 position)
    {
        // Преобразуем позицию в пространство экрана для проверки
        Vector2 screenPos = new Vector2(
            position.x + canvasRect.rect.width / 2,
            -position.y + canvasRect.rect.height / 2
        );
        
        return timerTextRect.Contains(screenPos) || 
               scoreTextRect.Contains(screenPos);
    }

    char GetRandomChar()
    {
        return (char)('A' + Random.Range(0, 26));
    }

    void OnButtonClick(GameObject button, char expectedChar)
    {
        if (!isGameRunning) return;

        totalPressedButtons++;
        
        if (Input.GetKey(expectedChar.ToString().ToLower()))
        {
            button.GetComponent<Image>().color = Color.green;
            score++;
            correctPressedButtons++;
            UpdateScore();
            Destroy(button, 0.3f);
            activeButtons.Remove(button);
        }
        else
        {
            button.GetComponent<Image>().color = Color.red;
        }
    }

    void UpdateTimer()
    {
        int seconds = Mathf.CeilToInt(currentTime);
        timerText.text = $"Time: {seconds}";

        if (seconds <= 5)
        {
            timerText.color = Color.red;
        }
    }

    void UpdateScore()
    {
        scoreText.text = $"Score: {score}";
    }

    IEnumerator EndGamePhase()
    {
        isGameRunning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        foreach (var button in activeButtons)
        {
            if (button != null) Destroy(button);
        }
        activeButtons.Clear();

        countdownText.gameObject.SetActive(true);
        
        if (totalPressedButtons == 0)
        {
            countdownText.text = "DEFEAT!\nYou didn't press any buttons";
            countdownText.color = Color.red;
        }
        else
        {
            float successRatio = (float)correctPressedButtons / totalPressedButtons;
            if (successRatio > 0.5f)
            {
                countdownText.text = $"VICTORY!\nScore: {score}\nCorrect: {correctPressedButtons}/{totalPressedButtons}";
                countdownText.color = Color.green;
            }
            else
            {
                countdownText.text = $"DEFEAT!\nScore: {score}\nCorrect: {correctPressedButtons}/{totalPressedButtons}";
                countdownText.color = Color.red;
            }
        }

        yield return new WaitForSeconds(5f);
    }

    void Update()
    {
        if (!isGameRunning) return;

        if (Input.anyKeyDown)
        {
            foreach (var button in activeButtons)
            {
                if (button == null) continue;
                
                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null && Input.GetKey(buttonText.text.ToLower()))
                {
                    OnButtonClick(button, buttonText.text[0]);
                    break;
                }
            }
        }
    }
}